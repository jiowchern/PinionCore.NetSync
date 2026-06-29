using PinionCore.NetSync.Tcp;

using UnityEngine;
using UnityEngine.UI;

namespace PinionCore.NetSync.Sample1
{
    public class UITcpConnector : MonoBehaviour
    {
        public Button Connect;
        public TMPro.TMP_InputField Address;
        public TMPro.TMP_InputField Port;
        public TcpConnector Connector;
        public void Start()
        {
            Connect.onClick.AddListener(_Click);

            // 連線參數來源已改為 Connector.Config (ScriptableObject)。
            // InputField 僅作唯讀顯示,讓使用者看到實際連線目標。
            _ShowConfig();
        }
        public void OnDestroy()
        {
            Connect.onClick.RemoveListener(_Click);
        }

        private void _ShowConfig()
        {
            if (Connector == null || Connector.Config == null)
                return;

            if (Address != null)
            {
                Address.text = Connector.Config.Host;
                Address.interactable = false;
            }
            if (Port != null)
            {
                Port.text = Connector.Config.Port.ToString();
                Port.interactable = false;
            }
        }

        private void _Click()
        {
            if (Connector.CurrentStatus == TcpConnector.ConnectorStatus.Offline)
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
