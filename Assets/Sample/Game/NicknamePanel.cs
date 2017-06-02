/*********************************
 ニックネーム更新パネル
*********************************/
using UnityEngine;
using Xyz.Anzfactory.NCMBUtil;

namespace Sample.Game
{

    public class NicknamePanel : MonoBehaviour
    {
        #region "Serialize Fields"
        [SerializeField] private RankingBoard rankingBoard;
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
            this.gameObject.SetActive(this.rankingBoard.ranking.HighScoreData != null && !string.IsNullOrEmpty(this.rankingBoard.ranking.HighScoreData.objectId));
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
            this.gameObject.SetActive(true);
        }
        #endregion

        #region "Private Methods"
        #endregion
    }

}
