/*********************************
 * Ranking Board Item
*********************************/
using UnityEngine;
using UnityEngine.UI;
using Xyz.Anzfactory.NCMBUtil.Models;

namespace Xyz.Anzfactory.NCMBUtil
{

    public class RankingBoardItem : MonoBehaviour
    {
        #region "Serialize Fields"
        [SerializeField] private Text rank;
        [SerializeField] private Image selfBackground;
        [SerializeField] private Text nickname;
        [SerializeField] private Text pt;
        #endregion

        #region "Fields"
        private int position;
        private Score score;
        #endregion

        #region "Properties"
        public int Position
        {
            get { return this.position; }
            set {
                this.position = value;
                this.rank.text = this.position.ToString();
            }
        }
        public Score Score
        {
            get { return this.score; }
            set {
                this.score = value;
                this.nickname.text = this.score.nickname;
                this.pt.text = this.score.score.ToString();
                this.selfBackground.gameObject.SetActive(score.isSelf);
            }
        }
        #endregion

        #region "Events"
        private void Awake()
        {
        }

        private void Start()
        {
            
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
        #endregion

        #region "Private Methods"
        #endregion
    }

}
