/*********************************
 * http://mb.cloud.nifty.com/doc/current/rest/common/format.html
 * http://mb.cloud.nifty.com/doc/current/rest/common/query.html
*********************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityRandom = UnityEngine.Random;

namespace Xyz.Anzfactory.NCMBUtil
{

    public class NCMBRanking : MonoBehaviour
    {
        private static readonly string DEFAULT_USER_NICKNAME = "No Name";
        private static readonly string PREFS_KEY_USER_NAME = "ncmb.userName";
        private static readonly string PREFS_KEY_USER_PASSWORD = "ncmb.userPassword";
        private static readonly string PREFS_KEY_HIGH_SCORE = "ncmb.userHighScore";


        #region "Serialize Fields"
        [SerializeField] private string ApplicationKey;
        [SerializeField] private string ClientKey;
        #endregion

        #region "Fields"
        private User registeredUser;
        public User RegisteredUser { get { return this.registeredUser; } }
        #endregion

        #region "Properties"
        #endregion

        #region "Lifecycle"
        private void Awake()
        {
            Yoshinani.Instance.Setup(this.ApplicationKey, this.ClientKey);
        }
        #endregion

        #region "Events"
        public void OnClickUpdateNickname()
        {
            
        }
        #endregion

        #region "Public Methods"
        public void RegisterUser(Action<bool, User> callback)
        {
            string userName = PlayerPrefs.GetString(PREFS_KEY_USER_NAME, "");
            string password = PlayerPrefs.GetString(PREFS_KEY_USER_PASSWORD, "");

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)) {
                // 新規
                userName = Guid.NewGuid().ToString("N");
                password = PasswordGenerator.GeneratePassword(16);
                var postData = new Yoshinani.RequestData();
                postData.AddParam("password", password);
                postData.AddParam("userName", userName);
                postData.AddParam("nickname", DEFAULT_USER_NICKNAME);
                Yoshinani.Instance.Call(Yoshinani.RequestType.POST, "users", postData, (isError, json) => {
                    if (!isError) {
                        this.registeredUser = JsonUtility.FromJson<User>(json);
                        if (!string.IsNullOrEmpty(this.registeredUser.objectId)) {
                            // 返ってくるjsonに追加したnicknameがないのでしぶしぶ手動せっとします...
                            // apiコールに気を使いたいんだからケチケチせずにフルフルで返してほしいわー
                            this.registeredUser.nickname = DEFAULT_USER_NICKNAME;
                            Yoshinani.Instance.SessionToken = this.registeredUser.sessionToken;
                            PlayerPrefs.SetString(PREFS_KEY_USER_NAME, userName);
                            PlayerPrefs.SetString(PREFS_KEY_USER_PASSWORD, password);
                        } else {
                            isError = true;
                        }
                    }

                    callback(isError, this.registeredUser);
                });
            } else {
                // 取得
                var queryData = new Yoshinani.RequestData();
                queryData.AddParam("userName", userName);
                queryData.AddParam("password", password);
                Yoshinani.Instance.Call(Yoshinani.RequestType.GET, "login", queryData, (isError, json) => {
                    if (!isError) {
                        this.registeredUser = JsonUtility.FromJson<User>(json);
                        Yoshinani.Instance.SessionToken = this.registeredUser.sessionToken;
                    }

                    callback(isError, this.registeredUser);
                });
            }

        }

        public void UpdateNickname(string nickname, Action<bool> callback)
        {
            Assert.IsTrue(this.registeredUser != null && !string.IsNullOrEmpty(this.registeredUser.objectId), "ユーザ認証が終わっていないみたい...");

            var putData = new Yoshinani.RequestData();
            putData.AddParam("nickname", nickname);
            Yoshinani.Instance.Call(Yoshinani.RequestType.PUT, string.Format("users/{0}", this.registeredUser.objectId), putData, (isError, json) => {
                this.registeredUser.nickname = nickname;
                callback(isError);
            });
        }

        public void SendScore(float newScore, bool isForce, Action<bool> callback)
        {
            Assert.IsTrue(this.registeredUser != null && !string.IsNullOrEmpty(this.registeredUser.objectId), "ユーザ認証が終わっていないみたい...");

            float currentHighScore = PlayerPrefs.GetFloat(PREFS_KEY_HIGH_SCORE, 0f);
            if (!isForce && currentHighScore >= newScore) {
                Debug.Log("送信する必要ないよ！");
                // 更新必要なし
                callback(false);
                return;
            }

            var queryData = new Yoshinani.RequestData();
            queryData.AddParam("userObjectId", this.registeredUser.objectId);
            Yoshinani.Instance.Call(Yoshinani.RequestType.GET, "classes/Scores", queryData, (isError, json) => {
                if (!isError) {
                    var scores = JsonUtility.FromJson<Scores>(json);
                    var scoreData = new Yoshinani.RequestData();
                    scoreData.AddParam("score", newScore);
                    scoreData.AddParam("nickname", this.registeredUser.nickname);
                    if (scores.results != null && scores.results.Count == 0) {
                        // 新規
                        scoreData.AddParam("userObjectId", this.registeredUser.objectId);
                        // ACL
                        var aclFormat = @"{""*"":{""read"":true},""{0}"":{""read"":true,""write"":true}}";
                        var acl = MiniJSON.Json.Deserialize(aclFormat.Replace("{0}", this.registeredUser.objectId));
                        scoreData.AddParam("acl", acl);
                        Yoshinani.Instance.Call(Yoshinani.RequestType.POST, "classes/Scores", scoreData, (isError2, json2) => {
                            if (!isError2) {
                                PlayerPrefs.SetFloat(PREFS_KEY_HIGH_SCORE, newScore);
                            }
                            callback(isError2);
                        });
                    } else if (isForce || newScore > scores.results[0].score) {
                        // 更新
                        Yoshinani.Instance.Call(Yoshinani.RequestType.PUT, string.Format("classes/Scores/{0}", scores.results[0].objectId), scoreData, (isError2, json2) => {
                            if (!isError2) {
                                PlayerPrefs.SetFloat(PREFS_KEY_HIGH_SCORE, newScore);
                            }
                            callback(isError2);
                        });
                    } else {
                        // 更新必要なし
                        callback(true);
                    }
                } else {
                    callback(false);
                }
            });
        }

        public void Top50(Action<List<Score>> callback)
        {
            var queryData = new Yoshinani.RequestData();
            queryData.Limit = 50;
            queryData.SortColumn = "-score";    // マイナスをつけると降順 つけないと昇順
            Yoshinani.Instance.Call(Yoshinani.RequestType.GET, "classes/Scores", queryData, (isError, json) => {
                var scores = JsonUtility.FromJson<Scores>(json);
                callback(scores.results);
            });
        }

        public void SelfRank(Action<bool, int> callback)
        {
            Assert.IsTrue(this.registeredUser != null && !string.IsNullOrEmpty(this.registeredUser.objectId), "ユーザ認証が終わっていないみたい...");

            var queryData = new Yoshinani.RequestData();
            queryData.AddParam("userObjectId", this.registeredUser.objectId);
            Yoshinani.Instance.Call(Yoshinani.RequestType.GET, "classes/Scores", queryData, (isError, json) => {
                if (!isError) {
                    var scores = JsonUtility.FromJson<Scores>(json);
                    if (scores.results.Count == 1) {
                        queryData.Reset();
                        var w = new Dictionary<string, object>();
                        w.Add("$gt", scores.results[0].score);
                        queryData.AddParam("score", w);
                        queryData.Count = true;
                        Yoshinani.Instance.Call(Yoshinani.RequestType.GET, "classes/Scores", queryData, (isError2, json2) => {
                            if (!isError2) {
                                Dictionary<string, object> result = MiniJSON.Json.Deserialize(json2) as Dictionary<string, object>;
                                callback(false, int.Parse(result["count"].ToString()) + 1);
                            } else {
                                callback(false, 0);
                            }
                        });
                    } else {
                        callback(false, 0);
                    }
                } else {
                    callback(false, 0);
                }
            });
        }
        #endregion

        #region "Private Methods"
        #endregion

        private class Yoshinani
        {
            public enum RequestType
            {
                GET,
                POST,
                PUT
            }

            private static Yoshinani instance = new Yoshinani();
            public static Yoshinani Instance { get { return instance; } }

            private static readonly string API_PROTOCOL = "https";
            private static readonly string API_DOMAIN = "mb.api.cloud.nifty.com";
            private static readonly string API_VERSION = "2013-09-01";

            private static readonly string KEY_SIGNATURE_METHOD = "SignatureMethod";
            private static readonly string KEY_SIGNATURE_VERSION= "SignatureVersion";
            private static readonly string KEY_APPLICATION = "X-NCMB-Application-Key";
            private static readonly string KEY_TIMESTAMP = "X-NCMB-Timestamp";
            private static readonly string KEY_SESSION_TOKEN = "X-NCMB-Apps-Session-Token";
            private static readonly string KEY_SIGNATURE = "X-NCMB-Signature";
            private static readonly string KEY_CONTENT_TYPE = "Content-Type";

            private static readonly string VAL_SIGNATURE_METHOD = "HmacSHA256";
            private static readonly string VAL_SIGNATURE_VERSION = "2";
            private static readonly string VAL_CONTENT_TYPE = "application/json";

            private string applicationKey;
            private string clientKey;
            private string timestamp;
            private string baseParamString;
            private Requestor requestor;

            public string SessionToken { get; set; }

            public Yoshinani()
            {
                var requestorObject = new GameObject("Yoshinani Requestor");
                this.requestor = requestorObject.AddComponent<Requestor>();
            }

            public void Setup(string applicationKey, string clientKey)
            {
                Assert.IsFalse(string.IsNullOrEmpty(applicationKey), "Application Keyが未設定！");
                Assert.IsFalse(string.IsNullOrEmpty(clientKey), "Client Keyが未設定！");

                this.applicationKey = applicationKey;
                this.clientKey = clientKey;
                this.timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                this.baseParamString = this.ParamString();
            }
                
            public void Call(RequestType method, string path, RequestData queryData, Action<bool, string> callback)
            {
                bool isAuthPath = (path.IndexOf("users") == 0 || path == "login");
                string endpoint = this.Endpoint(path);
                string queryString = this.QueryString(queryData, !isAuthPath);
                UnityWebRequest request;
                switch (method) {
                    case RequestType.GET:
                        if (!string.IsNullOrEmpty(queryString)) {
                            endpoint += "?" + queryString.Trim('&');
                        }
                        request = UnityWebRequest.Get(endpoint);
                        break;
                    case RequestType.POST:
                    case RequestType.PUT:
                    default:
                        request = new UnityWebRequest(endpoint, method.ToString());
                        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(MiniJSON.Json.Serialize(queryData.Parameters)));
                        break;
                }
                request.SetRequestHeader(KEY_APPLICATION, this.applicationKey);
                request.SetRequestHeader(KEY_SIGNATURE, this.Signature(method.ToString(), endpoint, queryString));
                request.SetRequestHeader(KEY_TIMESTAMP, this.timestamp);
                request.SetRequestHeader(KEY_CONTENT_TYPE, VAL_CONTENT_TYPE);
                if (!string.IsNullOrEmpty(this.SessionToken)) {
                    request.SetRequestHeader(KEY_SESSION_TOKEN, this.SessionToken);
                }
                request.downloadHandler = new DownloadHandlerBuffer();

                this.requestor.Done(request, (requested) => {
                    if (requested.isError) {
                        Debug.LogError(requested.error);
                        callback(true, null);
                    } else {
                        callback(false, requested.downloadHandler.text);
                    }
                });
            }

            private string Endpoint(string path)
            {
                return string.Format("{0}://{1}/{2}/{3}", API_PROTOCOL, API_DOMAIN, API_VERSION, path);
            }

            private string Signature(string method, string endpoint, string queryString)
            {
                string signatureString = this.SignatureString(method, endpoint, queryString);
                HMACSHA256 sha256 = new HMACSHA256(Encoding.UTF8.GetBytes(this.clientKey));
                byte[] signatureBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(signatureString));
                var signature = Convert.ToBase64String(signatureBytes);
                return signature;
            }

            private string SignatureString(string method, string endpoint, string queryString)
            {
                var pathAndQuery = endpoint.Replace(string.Format("{0}://{1}", API_PROTOCOL, API_DOMAIN), "");
                var parts = pathAndQuery.Split('?');
                StringBuilder builder = new StringBuilder(this.baseParamString);
                if (parts.Length == 2) {
                    builder.Append(queryString);
                }
                return string.Format(
                    "{0}\n{1}\n{2}\n{3}",
                    method,
                    API_DOMAIN,
                    parts[0],
                    builder.ToString()
                );
            }

            private string QueryString(RequestData queryData, bool isConvertJson)
            {
                StringBuilder builder = new StringBuilder();
                if (queryData.Count) {
                    builder.Append("&count=1");
                }
                if (queryData.Limit > 0) {
                    builder.Append(string.Format("&limit={0}", queryData.Limit));
                }
                if (!string.IsNullOrEmpty(queryData.SortColumn)) {
                    builder.Append(string.Format("&order={0}", WWW.EscapeURL(queryData.SortColumn)));
                }
                if (queryData.Parameters.Keys.Count > 0) {
                    string whereString = "";
                    if (isConvertJson) {
                        whereString = MiniJSON.Json.Serialize(queryData.Parameters);
                        builder.Append(string.Format("&where={0}", WWW.EscapeURL(whereString)));
                    } else {
                        var keys = new string[queryData.Parameters.Keys.Count];
                        queryData.Parameters.Keys.CopyTo(keys, 0);
                        Array.Sort(keys);
                        whereString = string.Join("&", Array.ConvertAll(keys, key => string.Format("{0}={1}", key, WWW.EscapeURL(queryData.Parameters[key].ToString()))));
                        builder.Append(string.Format("&{0}", whereString));
                    }
                }
                return builder.ToString();
            }

            private string ParamString()
            {
                string[] paramString = new string[] {
                    string.Format("{0}={1}", KEY_SIGNATURE_METHOD, VAL_SIGNATURE_METHOD),
                    string.Format("{0}={1}", KEY_SIGNATURE_VERSION, VAL_SIGNATURE_VERSION),
                    string.Format("{0}={1}", KEY_APPLICATION, this.applicationKey),
                    string.Format("{0}={1}", KEY_TIMESTAMP, this.timestamp),
                };
                return string.Join("&", paramString);
            }

            public struct RequestData
            {
                private Dictionary<string, object> parameters;
                public Dictionary<string, object> Parameters
                {
                    get {
                        if (this.parameters == null) {
                            this.parameters = new Dictionary<string, object>();
                        }
                        return this.parameters;
                    }
                }
                public string SortColumn { get; set; }
                public int Limit { get; set; }
                public bool Count { get; set; }

                public void AddParam(string key, object value)
                {
                    this.Parameters.Add(key, value);
                }

                public void Reset()
                {
                    this.Parameters.Clear();
                    this.SortColumn = null;
                    this.Limit = 0;
                    this.Count = false;
                }
            }

            private class Requestor: MonoBehaviour
            {
                void Awake()
                {
                    DontDestroyOnLoad(this.gameObject);
                }

                public void Done(UnityWebRequest request, Action<UnityWebRequest> callback)
                {
                    StartCoroutine(_done(request, callback));
                }
                    
                private IEnumerator _done(UnityWebRequest request, Action<UnityWebRequest> callback)
                {
                    yield return request.Send();
                    callback(request);
                }
            }
        }

        private class PasswordGenerator
        {
            private static readonly string passwordChars = "0123456789abcdefghijklmnopqrstuvwxyz";
            public static string GeneratePassword(int length)
            {
                StringBuilder sb = new StringBuilder(length);

                for (int i = 0; i < length; i++)
                {
                    //文字の位置をランダムに選択
                    int pos = UnityRandom.Range(0, passwordChars.Length);
                    //選択された位置の文字を取得
                    char c = passwordChars[pos];
                    //パスワードに追加
                    sb.Append(c);
                }

                return sb.ToString();
            }
        }

        [Serializable]
        public class User
        {
            public string objectId;
            public string userName;
            public string password;
            public string nickname;
            public string sessionToken;
        }

        [Serializable]
        public class Scores
        {
            public List<Score> results;
        }

        [Serializable]
        public class Score
        {
            public string objectId;
            public float score;
            public string userObjectId;
            public string nickname;
            [NonSerialized]
            public bool isSelf;
        }
    }

}
