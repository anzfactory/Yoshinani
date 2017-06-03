/*********************************
 
*********************************/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Xyz.Anzfactory.NCMBUtil;

namespace Sample.Game
{

    public class SampleGame : MonoBehaviour
    {
        #region "SerializeFields"
        [SerializeField] private SliderLoop sliderLoop;
        [SerializeField] private RankingBoard rankingBoard;
        [SerializeField] private GameObject nicknamePanel;
        [SerializeField] private InputField nicknameField;
        #endregion

        #region "LifeCycle"
        private void Start()
        {
            this.ShowNicknamePanelIfNeed();
        }
        private void Update()
        {
            
        }
        #endregion

        #region "Events"
        /// <summary>
        /// ストップボタンクリックイベント
        /// </summary>
        public void OnClickStopButton()
        {
            // スライダー止める
            this.sliderLoop.StopSlider((stopPosition, loopCount) => {
                // 返ってきた値でスコア算出
                var score = this.CalcScore(stopPosition, loopCount);
                // スコア送信（ハイスコア更新時のみ）
                this.rankingBoard.ranking.SendScore(score, false, (isError) => {
                    // スコア送信後にランキングボードなど表示
                    this.ShowRankingBoardAndNicknamePanel();
                    this.enabled = false;
                });
            });
        }

        /// <summary>
        /// ニックネーム更新ボタンクリックイベント
        /// </summary>
        public void OnClickUpdateNickname(Text text)
        {
            // ニックネーム更新
            this.rankingBoard.ranking.UpdateNickname(text.text, (isError) => {
                // 完了後にランキングボードリロード
                this.rankingBoard.Reload(null);
            });
        }

        /// <summary>
        /// リトライボタンクリックイベント
        /// </summary>
        public void OnClickRetryButton()
        {
            SceneManager.LoadScene("StopBarGame");
        }
        #endregion

        #region "Private Methods"
        /// <summary>
        /// 必要ならニックネーム表示
        /// </summary>
        private void ShowNicknamePanelIfNeed()
        {
            this.nicknamePanel.SetActive(this.rankingBoard.ranking.HighScoreData != null && !string.IsNullOrEmpty(this.rankingBoard.ranking.HighScoreData.objectId));
            if (this.nicknamePanel.activeSelf) {
                this.nicknameField.text = this.rankingBoard.ranking.HighScoreData.nickname;
            }
        }

        /// <summary>
        /// ランキングボードとニックネームパネルの表示
        /// </summary>
        private void ShowRankingBoardAndNicknamePanel()
        {
            this.rankingBoard.Show();
            this.nicknamePanel.SetActive(true);
        }

        /// <summary>
        /// スコア計算
        /// </summary>
        /// <returns>The score.</returns>
        /// <param name="position">SliderのHandleポジション(0.0f - 1.0f)</param>
        /// <param name="loopCount">SliderのHandlerの往復カウント</param>
        private float CalcScore(float position, int loopCount)
        {
            var diff = Mathf.Abs(position - 0.5f);
            var minus = 1000f * diff;
            return 1000f - minus * loopCount;
        }
        #endregion
    }

}
