using PinionCore.NetSync.Web;
using PinionCore.Network;
using PinionCore.Utility;
namespace PinionCore.NetSync.Samples.Chat
{
    class WebSocketState : PinionCore.Utility.IStatus
    {
        private readonly string _Endpoint;
        private WebSocketStream _Stream;
        public WebSocketState(string endpoint)
        {
            this._Endpoint = endpoint;
        }

        public System.Action<IStreamable> SuccessEvent;
        public System.Action<string> ErrorEvent;


        void IStatus.Enter()
        {
            _Stream = new WebSocketStream(_Endpoint);
            _Stream.OnOpen += _Opened;
            _Stream.OnClose += _Closed;
            _Stream.OnError += _Error;
            _Stream.Connect();  // 實際建立 WebSocket 連接
        }

        private void _Error(string message)
        {
            ErrorEvent(message);
        }

        private void _Closed()
        {
            ErrorEvent("close");
        }

        private void _Opened()
        {
            SuccessEvent(_Stream);
        }

        void IStatus.Leave()
        {
            _Stream.OnOpen -= _Opened;
            _Stream.OnClose -= _Closed;
            _Stream.OnError -= _Error;
        }

        void IStatus.Update()
        {
            
        }
    }

}
