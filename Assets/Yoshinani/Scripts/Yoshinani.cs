/*********************************
 * NCMB REST とのやり取りをするやつ
 * 
 * ## 参考 ##
 * http://mb.cloud.nifty.com/doc/current/rest/common/format.html
 * http://mb.cloud.nifty.com/doc/current/rest/common/query.html
*********************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;

namespace Xyz.Anzfactory.NCMBUtil
{
    public class Yoshinani
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

        #region "Public Methods"
        /// <summary>
        /// セットアップ（使う前に必ず呼ぶこと）
        /// </summary>
        /// <param name="applicationKey">Application key.</param>
        /// <param name="clientKey">Client key.</param>
        public void Setup(string applicationKey, string clientKey)
        {
            Assert.IsFalse(string.IsNullOrEmpty(applicationKey), "Application Keyが未設定！");
            Assert.IsFalse(string.IsNullOrEmpty(clientKey), "Client Keyが未設定！");

            this.applicationKey = applicationKey;
            this.clientKey = clientKey;
            this.timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            this.baseParamString = this.ParamString();
        }

        /// <summary>
        /// APIを叩く！！
        /// </summary>
        /// <param name="method">Method.</param>
        /// <param name="path">Path."classes"を含む形で指定する。Scoresというクラスを対象にするなら classes/Scores という感じ</param>
        /// <param name="queryData">Query data.</param>
        /// <param name="callback">Callback.</param>
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

        public void Upload(string fileName, byte[] bytes, Action<bool, string> callback)
        {
            string endpoint = this.Endpoint(string.Format("files/{0}", fileName));
            UnityWebRequest request;

            var formData = new WWWForm();
            formData.AddBinaryData("file", bytes, fileName, "image/png");
            request = UnityWebRequest.Post(endpoint, formData);
            request.SetRequestHeader(KEY_APPLICATION, this.applicationKey);
            request.SetRequestHeader(KEY_SIGNATURE, this.Signature(RequestType.POST.ToString(), endpoint, ""));
            request.SetRequestHeader(KEY_TIMESTAMP, this.timestamp);
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
        #endregion

        #region "Private Methods"
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
        #endregion

        #region "Inner Class/Struct"
        /// <summary>
        /// API叩くのに必要なもろもろをまとめて持つだけのやつ
        /// 渡す時に引数全部かくのがめんどかったので
        /// </summary>
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

        /// <summary>
        /// UnityWebRequestがコルーチンで処理するので、それを毎回かくのめんどくさいのでその処理のヘルパー的な
        /// </summary>
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
        #endregion
    }

}
