/*********************************
 * Ranking Board
 * 
 * Yoshinaniを使ってランキングボード表示するよ
*********************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Xyz.Anzfactory.NCMBUtil.Models;

namespace Xyz.Anzfactory.NCMBUtil
{

    public class RankingBoard : MonoBehaviour
    {
        #region "Serialize Fields"
        [SerializeField] private NCMBRanking ranking;
        [SerializeField] private GameObject content;
        [SerializeField] private RankingBoardItem itemTemplate;
        [SerializeField] private GameObject footer;
        [SerializeField] private Text selfRank;
        #endregion

        #region "Fields"
        private List<RankingBoardItem> items;
        #endregion

        #region "Properties"
        #endregion

        #region "Events"
        private void Awake()
        {
            this.items = new List<RankingBoardItem>();
            this.selfRank.gameObject.SetActive(false);
            this.footer.SetActive(false);
        }

        private void Start()
        {
            this.itemTemplate.gameObject.SetActive(false);
            this.gameObject.SetActive(false);
        }

        private void Update()
        {
        }

        private void FixedUpdate()
        {
        }

        private void OnDestroy()
        {
        }
        #endregion

        #region "Public Methods"
        public void Show()
        {
            this.Show(null);
        }
        public void Show(Action fin)
        {
            if (this.gameObject.activeSelf) {
                return;
            }

            this.gameObject.SetActive(true);

            this.ranking.Top50(scoreList => {

                this.ranking.SelfRank((isError, rank) => {
                    this.selfRank.text = string.Format("あなたの順位は{0}位です", rank);
                    this.selfRank.gameObject.SetActive(!isError);
                });

                for (int i = 0; i < Math.Min(scoreList.Count, this.items.Count); i++) {
                    var score = scoreList[i];
                    var scoreItem = this.items[i];
                    scoreItem.Position = (i + 1);
                    scoreItem.Score = score;
                    scoreItem.gameObject.SetActive(true);
                }

                for (int i = this.items.Count; i < scoreList.Count; i++) {
                    var score = scoreList[i];
                    var scoreItem = GameObject.Instantiate<RankingBoardItem>(this.itemTemplate);
                    scoreItem.Position = (i + 1);
                    scoreItem.Score = score;
                    scoreItem.gameObject.transform.SetParent(this.content.transform, false);
                    scoreItem.gameObject.SetActive(true);
                    this.items.Add(scoreItem);
                }

                this.footer.SetActive(true);

                if (fin != null) {
                    fin();
                }

            });
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);

            foreach (var scoreItem in this.items) {
                scoreItem.gameObject.SetActive(false);
            }

            this.selfRank.gameObject.SetActive(false);
            this.footer.SetActive(false);
        }
        #endregion

        #region "Private Methods"
        #endregion
    }

}
