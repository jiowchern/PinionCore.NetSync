using PinionCore.NetSync.Standalone;
using UnityEngine;


namespace PinionCore.NetSync.Sample1
{
    public class UIStandaloneConnector : MonoBehaviour
    {
        public Connector Connector;
        public UnityEngine.UI.Button Connect;

        public void Start()
        {
            Connect.onClick.AddListener(_Click);
        }

        private void _Click()
        {
            if (Connector.IsConnect())
            {
                Connector.Disconnect();
                
            }
            else
            {
                Connector.Connect();
            }
        }
    }

}
