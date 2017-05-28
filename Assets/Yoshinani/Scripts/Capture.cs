/*********************************
 
*********************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Xyz.Anzfactory.NCMBUtil
{
    public class Capture : MonoBehaviour
    {
        #region "Serialize Fields"
        [SerializeField] private string applicationKey;
        [SerializeField] private string clientKey;
        #endregion

        #region "Fields"
        #endregion

        #region "Properties"
        #endregion

        #region "Events"
        private void Awake()
        {
            if (!string.IsNullOrEmpty(this.applicationKey) && !string.IsNullOrEmpty(this.clientKey)) {
                Yoshinani.Instance.Setup(this.applicationKey, this.clientKey);
            }
        }
        #endregion

        #region "Public Methods"
        public void TakeAndUpload(Action<string> callback)
        {
            StartCoroutine(this.TakeScreenshot((texture) => {
                var fileName = Guid.NewGuid().ToString("N");
                Yoshinani.Instance.Upload(fileName, texture.EncodeToPNG(), (isError, json) => {
                    if (isError) {
                        Debug.LogError("Error!");
                    } else {
                        var response = MiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
                        if (response.ContainsKey("fileName")) {
                            callback(response["fileName"].ToString());
                        } else {
                            Debug.LogError(json);
                        }
                    }
                });
            }));
        }

        public void TakeAndOpenImageWindow()
        {
            StartCoroutine(this.TakeScreenshot((texture) => {
                var textureBase64 = Convert.ToBase64String(texture.EncodeToPNG());
                var image = string.Format("data:image/png;base64,{0}", textureBase64);
                Application.ExternalEval(string.Format("window.open('{0}', '_blank')", image));
            }));
        }
        #endregion

        #region "Private Methods"

        private IEnumerator TakeScreenshot(Action<Texture2D> callback)
        {
            yield return new WaitForEndOfFrame();

            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();
            callback(texture);
        }

        #endregion
    }

}
