/*********************************
 Score Canvas
*********************************/
using UnityEngine;
using Xyz.Anzfactory.NCMBUtil;

namespace Sample.Game
{

    public class ScoreCanvas : MonoBehaviour
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
            this.rankingBoard.Show();
        }
        #endregion

        #region "Private Methods"
        #endregion
    }

}
