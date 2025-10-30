using PinionCore.NetSync.Tcp;

using System;
using System.Net;
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
            
            
        }
        public void OnDestroy()
        {
            Connect.onClick.RemoveListener(_Click);
        }
        private void _Click()
        {
            if (Connector.CurrentStatus == TcpConnector.ConnectorStatus.Offline)
            {
                if (!System.Net.IPAddress.TryParse(Address.text, out var ip))
                    return;
                if (!int.TryParse(Port.text, out var port))
                    return;

                Connector.Connect(new System.Net.IPEndPoint(ip, port));

            }
            else
            {
                Connector.Disconnect();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}