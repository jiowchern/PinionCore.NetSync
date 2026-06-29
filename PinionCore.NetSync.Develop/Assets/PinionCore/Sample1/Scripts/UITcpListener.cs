using PinionCore.NetSync.Tcp;
using UnityEngine;
namespace PinionCore.NetSync.Sample1
{
    public class UITcpListener : MonoBehaviour
    {
        public TMPro.TMP_InputField Port;
        public TcpListener Listener;
        public UnityEngine.UI.Button StartListening;
        public void Start()
        {
            StartListening.onClick.AddListener(_Click);

            // 監聽連接埠來源已改為 Listener.Config.Port (ScriptableObject)。
            // InputField 僅作唯讀顯示。
            _ShowConfig();
        }

        public void OnDestroy()
        {
            StartListening.onClick.RemoveListener(_Click);

        }

        private void _ShowConfig()
        {
            if (Listener == null || Listener.Config == null || Port == null)
                return;

            Port.text = Listener.Config.Port.ToString();
            Port.interactable = false;
        }

        private void _Click()
        {
            IListenerEditor editor = Listener;
            if (!editor.IsActive)
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
