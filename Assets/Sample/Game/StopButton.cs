/*********************************
 Stopボタン
*********************************/
using UnityEngine;
using Xyz.Anzfactory.NCMBUtil;

namespace Sample.Game
{

    public class StopButton : MonoBehaviour
    {
        #region "Serialize Fields"
        [SerializeField] private SliderLoop sliderLoop;
        [SerializeField] private Canvas scoreCanvas;
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
        public void OnClick()
        {
            this.sliderLoop.StopSlider((stopPosition, loopCount) => {
                this.rankingBoard.ranking.SendScore(this.CalcScore(stopPosition, loopCount), false, (isError) => {
                    this.scoreCanvas.GetComponent<ScoreCanvas>().Show();
                    this.enabled = false;
                });
            });
        }
        #endregion

        #region "Private Methods"
        private float CalcScore(float position, int loopCount) {
            var diff = Mathf.Abs(position - 0.5f);
            var minus = 1000f * diff;
            return 1000f - minus * loopCount;
        }
        #endregion
    }

}
