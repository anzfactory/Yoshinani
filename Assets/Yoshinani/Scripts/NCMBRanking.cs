/*********************************
 
*********************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityRandom = UnityEngine.Random;
using Xyz.Anzfactory.NCMBUtil.Models;

namespace Xyz.Anzfactory.NCMBUtil
{

    public class NCMBRanking : MonoBehaviour
    {
        private static readonly string PREFS_KEY_HIGH_SCORE_DATA = "ncmb.userHighScoreData";

        public enum ApiPath
        {
            Scores
        }


        #region "Serialize Fields"
        [SerializeField] private string ApplicationKey;
        [SerializeField] private string ClientKey;
        #endregion

        #region "Fields"
        private Score highScoreData;
        #endregion

        #region "Properties"
        public Score HighScoreData
        {
            get { return this.highScoreData; }
        }
        #endregion

        #region "Lifecycle"
        private void Awake()
        {
            Yoshinani.Instance.Setup(this.ApplicationKey, this.ClientKey);
            var json = PlayerPrefs.GetString(PREFS_KEY_HIGH_SCORE_DATA, "");
            if (!string.IsNullOrEmpty(json)) {
                this.highScoreData = JsonUtility.FromJson<Score>(json);
            }
        }

        public void Start()
        {
        }
        #endregion

        #region "Public Methods"
        public void UpdateNickname(string nickname, Action<bool> callback)
        {
            if (this.highScoreData == null) {
                Debug.LogError("まだ一度もスコア送信していないみたい。まずはスコアを送信してくださいね！");
                return;
            }

            var putData = new Yoshinani.RequestData();
            putData.AddParam("nickname", nickname);
            Yoshinani.Instance.Call(Yoshinani.RequestType.PUT, string.Format("{0}/{1}", ApiPath.Scores.Val(), this.highScoreData.objectId), putData, (isError, json) => {
                this.highScoreData.nickname = nickname;
                PlayerPrefs.SetString(PREFS_KEY_HIGH_SCORE_DATA, JsonUtility.ToJson(this.highScoreData));
                PlayerPrefs.Save();
                callback(isError);
            });
        }

        public void SendScore(float newScore, bool isForce, Action<bool> callback)
        {
            var nickname = this.highScoreData == null ? "No Name" : this.highScoreData.nickname;
            this.SendScore(newScore, nickname, isForce, callback);
        }
        public void SendScore(float newScore, string nickname, bool isForce, Action<bool> callback)
        {
            if (!isForce && this.highScoreData != null && this.highScoreData.score >= newScore) {
                if (this.highScoreData != null && this.highScoreData.nickname != nickname) {
                    UpdateNickname(nickname, callback);
                } else {
                    // 更新必要なし
                    callback(false);
                }
                return;
            }

            if (string.IsNullOrEmpty(nickname.Trim())) {
                nickname = "No Name";
            }
            var scoreData = new Yoshinani.RequestData();
            scoreData.AddParam("nickname", nickname);
            scoreData.AddParam("score", newScore);
            System.Action<float, string> fin = (float s, string n) => {
                this.highScoreData.score = s;
                this.highScoreData.nickname = n;
                PlayerPrefs.SetString(PREFS_KEY_HIGH_SCORE_DATA, JsonUtility.ToJson(this.highScoreData));
                PlayerPrefs.Save();
            };

            if (this.highScoreData == null) {
                // 新規
                Yoshinani.Instance.Call(Yoshinani.RequestType.POST, ApiPath.Scores.Val(), scoreData, (isError, json) => {
                    var result = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
                    if (!isError && result.ContainsKey("objectId")) {
                        this.highScoreData = new Score();
                        this.highScoreData.objectId = result["objectId"].ToString();
                        fin(newScore, nickname);
                    } else {
                        isError = true;
                    }
                    callback(isError);
                });
            } else {
                // 更新
                Yoshinani.Instance.Call(Yoshinani.RequestType.PUT, string.Format("{0}/{1}", ApiPath.Scores.Val(), this.highScoreData.objectId), scoreData, (isError, json) => {
                    if (!isError) {
                        fin(newScore, nickname);
                    }
                    callback(isError);
                });
            }
        }

        public void Top50(Action<List<Score>> callback)
        {
            var queryData = new Yoshinani.RequestData();
            queryData.Limit = 50;
            queryData.SortColumn = "-score";    // マイナスをつけると降順 つけないと昇順
            Yoshinani.Instance.Call(Yoshinani.RequestType.GET, ApiPath.Scores.Val(), queryData, (isError, json) => {
                var scores = JsonUtility.FromJson<Scores>(json);
                foreach (var score in scores.results) {
                    score.isSelf = (this.highScoreData != null && score.objectId == this.highScoreData.objectId);
                }
                callback(scores.results);
            });
        }

        public void SelfRank(Action<bool, int> callback)
        {
            if (this.highScoreData == null) {
                callback(true, 0);
                return;
            }
            var requestData = new Yoshinani.RequestData();
            var w = new Dictionary<string, object>();
            w.Add("$gt", this.highScoreData.score);
            requestData.AddParam("score", w);
            requestData.Count = true;    // Count()するという指示
            Yoshinani.Instance.Call(Yoshinani.RequestType.GET, ApiPath.Scores.Val(), requestData, (isError, json) => {
                if (!isError) {
                    Dictionary<string, object> result = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
                    callback(false, int.Parse(result["count"].ToString()) + 1);
                } else {
                    callback(true, 0);
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
                case NCMBRanking.ApiPath.Scores:
                    return "classes/Scores";
                default:
                    return "";
            }
        }
    }

}
