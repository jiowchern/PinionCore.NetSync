using PinionCore.Network;
using PinionCore.Remote.Soul;
using System.Net.WebSockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace PinionCore.NetSync.Web
{

    [RequireComponent(typeof(Server))]
    public class WebListener : MonoBehaviour
    {
        private readonly Listener _Listener;
        public int Users;
        public bool IsListening { get; private set; }

        public WebListener()
        {
            _Listener = new Listener();
        }
        public void Bind(int port)
        {
            
            IListenable listenable = _Listener;
            listenable.StreamableEnterEvent += _Join;
            listenable.StreamableLeaveEvent += _Leave;
            var server = GetComponent<Server>();
            server.Listener.Add(_Listener);
            _Listener.Tcp.Bind(port);

            IsListening = true;
        }

        private void _Leave(IStreamable obj)
        {
            UnityEngine.Debug.Log("Leave");
            Users--;
        }

        private void _Join(IStreamable streamable)
        {
            UnityEngine.Debug.Log("Join");
            Users++;
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
