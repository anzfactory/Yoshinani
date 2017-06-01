/*********************************
 Sliderを動かし続ける
*********************************/
using UnityEngine;

namespace Sample.Game
{

    public class SliderLoop : MonoBehaviour
    {
        #region "Serialize Fields"
        [SerializeField] private UnityEngine.UI.Slider slider;
        #endregion

        #region "Fields"
        private int counter;
        private int direction;
        private bool isStop;
        #endregion

        #region "Properties"
        #endregion

        #region "Events"
        private void Awake()
        {
            this.counter = 1;
            this.direction = 1;
            this.isStop = false;
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (this.isStop) {
                return;
            }
            var val = Time.deltaTime * this.direction;
            this.slider.value += val;
            if (this.slider.value >= 1) {
                this.direction = -1;
            } else if (this.slider.value <= 0) {
                this.direction = 1;
                counter++;
            }
        }

        private void FixedUpdate()
        {
        }

        private void OnDestroy()
        {
        }
        #endregion

        #region "Public Methods"
        public void StopSlider(System.Action<float, int> callback)
        {
            this.isStop = true;
            callback(this.slider.value, counter);
            this.enabled = false;
        }
        #endregion

        #region "Private Methods"
        #endregion
    }

}
