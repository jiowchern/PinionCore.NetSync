using PinionCore.Network;
using PinionCore.Remote.Soul;
using System.Net.WebSockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System;

namespace PinionCore.NetSync.Web
{

    [RequireComponent(typeof(Server))]
    public class WebListener : MonoBehaviour , IListenerEditor
    {
        private readonly Listener _Listener;

        public bool IsListening { get; private set; }

        bool IListenerEditor.IsActive => IsListening;

        public WebListener()
        {
            _Listener = new Listener();
        }

        event Action<int> IListenerEditor.DataReceivedEvent
        {
            add
            {
                _Listener.DataReceivedEvent += value;
            }

            remove
            {
                _Listener.DataReceivedEvent -= value;
            }
        }

        event Action<int> IListenerEditor.DataSendEvent
        {
            add
            {
                _Listener.DataSentEvent += value;
            }

            remove
            {
                _Listener.DataSentEvent -= value;
            }
        }

        public void Bind(int port)
        {
            
            IListenable listenable = _Listener;
            
            var server = GetComponent<Server>();
            server.Listener.Add(_Listener);
            _Listener.Tcp.Bind(port);

            IsListening = true;
        }

        

        public void Close()
        {
            IsListening = false;
            _Listener.Tcp.Close();
            var server = GetComponent<Server>();
            server.Listener.Remove(_Listener);
        }
    }
}
