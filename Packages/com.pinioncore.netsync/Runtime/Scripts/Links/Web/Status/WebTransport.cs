using PinionCore.Remote.Ghost;
using PinionCore.Utility;
using System;
using System.Threading.Tasks;
namespace PinionCore.NetSync.Web.Status
{
    class WebTransport : IStatus
    {
        private readonly WebSocketStream stream;
        private readonly Client agent;
        

        public event Action<string> OfflineEvent;

        public WebTransport(WebSocketStream stream , Client agent)
        {
        
            this.stream = stream;
            this.agent = agent;
        }

        

        void IStatus.Enter()
        {            
            
            PinionCore.Utility.Log.Instance.WriteInfoImmediate("WebTransport Enter");
            stream.OnError += _Error;
            UnityEngine.Debug.Log("WebTransport Enter");    
            agent.Enable(stream);
            UnityEngine.Debug.Log("WebTransport Enter Done");
        }

        private void _Error(string obj)
        {
            OfflineEvent(obj);
        }

        void IStatus.Leave()
        {
            
            agent.Disable();
            stream.Close();
        }

        void IStatus.Update()
        {
            
        }
    }
}
