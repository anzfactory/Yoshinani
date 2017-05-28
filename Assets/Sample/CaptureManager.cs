/*********************************
 *
*********************************/
using UnityEngine;
using Xyz.Anzfactory.NCMBUtil;

namespace Sample
{

    public class CaptureManager : MonoBehaviour
    {
        #region "Serialize Fields"
        [SerializeField] private Capture capture;
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
        public void SaveScreenshot()
        {
            this.capture.TakeAndOpenImageWindow();
        }
        #endregion

        #region "Private Methods"
        #endregion
    }

}
