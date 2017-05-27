/*********************************
 
*********************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityRandom = UnityEngine.Random;

namespace Xyz.Anzfactory.NCMBUtil
{

    public class NCMBRanking : MonoBehaviour
    {
        private static readonly string DEFAULT_USER_NICKNAME = "No Name";
        private static readonly string PREFS_KEY_USER_NAME = "ncmb.userName";
        private static readonly string PREFS_KEY_USER_PASSWORD = "ncmb.userPassword";
        private static readonly string PREFS_KEY_HIGH_SCORE = "ncmb.userHighScore";

        public enum ApiPath
        {
            Users,
            Login,
            Scores
        }


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
                Yoshinani.Instance.Call(Yoshinani.RequestType.POST, ApiPath.Users.Val(), postData, (isError, json) => {
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
                Yoshinani.Instance.Call(Yoshinani.RequestType.GET, ApiPath.Login.Val(), queryData, (isError, json) => {
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
            Yoshinani.Instance.Call(Yoshinani.RequestType.PUT, string.Format("{0}/{1}", ApiPath.Users.Val(), this.registeredUser.objectId), putData, (isError, json) => {
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
            Yoshinani.Instance.Call(Yoshinani.RequestType.GET, ApiPath.Scores.Val(), queryData, (isError, json) => {
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
                        Yoshinani.Instance.Call(Yoshinani.RequestType.POST, ApiPath.Scores.Val(), scoreData, (isError2, json2) => {
                            if (!isError2) {
                                PlayerPrefs.SetFloat(PREFS_KEY_HIGH_SCORE, newScore);
                            }
                            callback(isError2);
                        });
                    } else if (isForce || newScore > scores.results[0].score) {
                        // 更新
                        Yoshinani.Instance.Call(Yoshinani.RequestType.PUT, string.Format("{0}/{1}", ApiPath.Scores.Val(), scores.results[0].objectId), scoreData, (isError2, json2) => {
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
            Yoshinani.Instance.Call(Yoshinani.RequestType.GET, ApiPath.Scores.Val(), queryData, (isError, json) => {
                var scores = JsonUtility.FromJson<Scores>(json);
                callback(scores.results);
            });
        }

        public void SelfRank(Action<bool, int> callback)
        {
            Assert.IsTrue(this.registeredUser != null && !string.IsNullOrEmpty(this.registeredUser.objectId), "ユーザ認証が終わっていないみたい...");

            var queryData = new Yoshinani.RequestData();
            queryData.AddParam("userObjectId", this.registeredUser.objectId);
            Yoshinani.Instance.Call(Yoshinani.RequestType.GET, ApiPath.Scores.Val(), queryData, (isError, json) => {
                if (!isError) {
                    var scores = JsonUtility.FromJson<Scores>(json);
                    if (scores.results.Count == 1) {
                        queryData.Reset();
                        var w = new Dictionary<string, object>();
                        w.Add("$gt", scores.results[0].score);
                        queryData.AddParam("score", w);
                        queryData.Count = true;
                        Yoshinani.Instance.Call(Yoshinani.RequestType.GET, ApiPath.Scores.Val(), queryData, (isError2, json2) => {
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

        #region "Inner Classes"
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
        #endregion
    }

    /// <summary>
    /// 拡張
    /// </summary>
    public static class ApiPathExtension
    {
        public static string Val(this NCMBRanking.ApiPath self) 
        {
            switch (self) {
                case NCMBRanking.ApiPath.Users:
                    return "users";
                case NCMBRanking.ApiPath.Login:
                    return "login";
                case NCMBRanking.ApiPath.Scores:
                    return "classes/Scores";
                default:
                    return "";
            }
        }
    }

}
