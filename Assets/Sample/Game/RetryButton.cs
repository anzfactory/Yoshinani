/*********************************
 リトライ!
*********************************/
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Sample.Game
{

    public class RetryButton : MonoBehaviour
    {
        #region "Serialize Fields"
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
            SceneManager.LoadScene("StopBarGame");
        }
        #endregion

        #region "Private Methods"
        #endregion
    }

}
