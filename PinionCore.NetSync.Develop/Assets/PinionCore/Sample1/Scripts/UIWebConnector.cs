using UnityEngine;
using UnityEngine.UI;

namespace PinionCore.NetSync.Sample1
{
    public class UIWebConnector : MonoBehaviour
    {
        public Button Connect;
        public TMPro.TMP_InputField Address;
        public PinionCore.NetSync.Web.WebConnector Connector;
        public void Start()
        {
            Connect.onClick.AddListener(_Click);

            // 連線位址來源已改為 Connector.Config.Url (ScriptableObject)。
            // InputField 僅作唯讀顯示。
            _ShowConfig();
        }
        public void OnDestroy()
        {
            Connect.onClick.RemoveListener(_Click);
        }

        private void _ShowConfig()
        {
            if (Connector == null || Connector.Config == null || Address == null)
                return;

            Address.text = Connector.Config.Url;
            Address.interactable = false;
        }

        private void _Click()
        {
            if (!Connector.IsConnected)
            {
                Connector.Connect();
            }
            else
            {
                Connector.Disconnect();
            }
        }
    }

}
