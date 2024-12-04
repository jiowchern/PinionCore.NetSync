using PinionCore.Remote.Server.Tcp;
using Unity.Properties;
using UnityEngine;

namespace PinionCore.NetSync.Tcp
{
    [RequireComponent(typeof(Server))]
    public class TcpListener : MonoBehaviour
    {
        private Listener _Listener;
        private readonly System.Collections.Concurrent.ConcurrentBag<int> _Receives;
        [CreateProperty] public long BytesReceived { get; private set; }

        private readonly System.Collections.Concurrent.ConcurrentBag<int> _Sends;
        [CreateProperty] public long BytesSent { get; private set; }

        public enum ListenerStatus
        {
            Offline,
            Online,
        }
        [CreateProperty] public ListenerStatus CurrentStatus { get; private set; }
        public TcpListener()
        {
            _Receives = new System.Collections.Concurrent.ConcurrentBag<int>();
            _Sends = new System.Collections.Concurrent.ConcurrentBag<int>();
            _Listener = new Listener();
        }
        public void Bind(int port)
        {
            UnityEngine.Debug.Log($"Bind {port}");
            if (CurrentStatus == ListenerStatus.Online)
            {
                return;
            }
            
            CurrentStatus = ListenerStatus.Online;
            BytesReceived = 0;
            BytesSent = 0;            
            _Listener.DataReceivedEvent += _Receive;
            _Listener.DataSentEvent += _Send;
            var server = GetComponent<Server>();
            server.Listener.Add(_Listener);
            _Listener.Bind(port);
        }       

        public void Close()
        {
            if (CurrentStatus == ListenerStatus.Offline)
            {
                return;
            }
            _Listener.Close();
            var server = GetComponent<Server>();
            server.Listener.Remove(_Listener);
            _Listener.DataReceivedEvent -= _Receive;
            _Listener.DataSentEvent -= _Send;
            CurrentStatus = ListenerStatus.Offline;
            
        }
        void _Receive(int receive)
        {
            _Receives.Add(receive);
        }

        void _Send(int send)
        {
            _Sends.Add(send);
        }
        void Update()
        {
            while (_Receives.TryTake(out var receive))
            {
                BytesReceived += receive;
            }

            while (_Sends.TryTake(out var send))
            {
                BytesSent += send;
            }
        }
    }
}
