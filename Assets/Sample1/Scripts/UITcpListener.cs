using PinionCore.NetSync.Tcp;
using System;
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
        }

        public void OnDestroy()
        {
            StartListening.onClick.RemoveListener(_Click);
        
        }

        private void _Click()
        {
            if (!(Listener as IListenerEditor).IsActive)
            {
                if (!int.TryParse(Port.text, out var port))
                    return;

                Listener.Bind(port);
            }
            else
            {
                Listener.Close();
            }
        }
    }

}