using PinionCore.Network.Tcp;
using System.Net;
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

            
        }
        public void OnDestroy()
        {
            Connect.onClick.RemoveListener(_Click);
        }

        private void _Click()
        {
            if (!Connector.IsConnected)
            {
                Address.enabled = false;

                // parsw web socket connect param
                var addresss= Address.text;
                if (string.IsNullOrEmpty(addresss))
                    return;
                Connector.Connect(addresss);

            }
            else
            {
                Address.enabled = true;
                Connector.Close();
            }
        }
    }

}