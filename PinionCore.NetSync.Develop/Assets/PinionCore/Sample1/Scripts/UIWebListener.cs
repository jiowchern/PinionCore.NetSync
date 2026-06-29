using UnityEngine;
namespace PinionCore.NetSync.Sample1
{
    public class UIWebListener : MonoBehaviour
    {
        public TMPro.TMP_InputField Bind;
        public Web.WebListener Listener;
        public UnityEngine.UI.Button StartListening;
        public void Start()
        {
            StartListening.onClick.AddListener(_Click);

            // 監聽連接埠來源已改為 Listener.Config (從 Url 解析 Port)。
            // InputField 僅作唯讀顯示。
            _ShowConfig();
        }

        public void OnDestroy()
        {
            StartListening.onClick.RemoveListener(_Click);

        }

        private void _ShowConfig()
        {
            if (Listener == null || Listener.Config == null || Bind == null)
                return;

            Bind.text = Listener.Config.Port.ToString();
            Bind.interactable = false;
        }

        private void _Click()
        {
            if (!Listener.IsListening)
            {
                Listener.Bind();
            }
            else
            {
                Listener.Close();
            }
        }
    }

}
