/*********************************
 ニックネーム更新
*********************************/
using UnityEngine;
using Xyz.Anzfactory.NCMBUtil;

namespace Sample.Game
{

    public class UpdateNicknameButton : MonoBehaviour
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
        public void OnClick(UnityEngine.UI.Text text)
        {
            this.rankingBoard.ranking.UpdateNickname(text.text, (isError) => {
                this.rankingBoard.Reload(null);
            });
        }
        #endregion

        #region "Private Methods"
        #endregion
    }

}
