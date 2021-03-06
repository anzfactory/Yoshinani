﻿/*********************************
 Sample
*********************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xyz.Anzfactory.NCMBUtil;
using Xyz.Anzfactory.NCMBUtil.Models;

namespace Sample
{

    public class NCMBRankingManager : MonoBehaviour
    {
        #region "Serialize Fields"
        [SerializeField] private NCMBRanking ncmbRanking;
        #endregion

        #region "Fields"
        #endregion

        #region "Properties"
        #endregion

        #region "Events"
        private void Awake()
        {
        }

        private void Start()
        {
        }

        #endregion

        #region "Public Methods"
        public void OnClickChangeNickname(Text text)
        {
            this.ncmbRanking.UpdateNickname(text.text, (isError) => {
                if (!isError) {
                    Debug.Log(string.Format("変えたよ！ {0}", this.ncmbRanking.HighScoreData.nickname));
                } else {
                    Debug.LogError("何らかの理由でニックネーム変更失敗！");
                }
            });
        }

        public void OnClickTop50()
        {
            this.ncmbRanking.Top50((scoreList) => {
                foreach (var score in scoreList) {
                    Debug.Log(string.Format("{0}: {1}", score.nickname, score.score.ToString()));
                }
            });
        }

        public void OnClickSendScore(Text text)
        {
            this.ncmbRanking.SendScore(float.Parse(text.text), false, (isError) => {
                if (!isError) {
                    Debug.Log("スコア送信したよ！");
                } else {
                    Debug.LogError("何らかの理由でスコア送信失敗！");
                }
            });
        }

        public void OnClickSelfRank()
        {
            this.ncmbRanking.SelfRank((isError, rank) => {
                if (!isError) {
                    Debug.Log(string.Format("順位：{0}", rank));
                } else {
                    Debug.LogError("何らかの理由で順位獲得失敗！");
                }
            });
        }
        #endregion

        #region "Private Methods"
        #endregion
    }

}
