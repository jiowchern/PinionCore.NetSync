using PinionCore.Network;
using PinionCore.Network.Tcp;
using PinionCore.Utility;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PinionCore.NetSync.Samples.Chat
{
   
    internal class TcpSocketState : IStatus
    {
        private readonly string endpoint;
        public System.Action<IStreamable> SuccessEvent;
        public System.Action<string> ErrorEvent;
        string _Error;
        Peer _Peer;
        public TcpSocketState(string endpoint)
        {
            this.endpoint = endpoint;
        }

        

        void IStatus.Enter()
        {
            // parse endpoint to IpEndPoint
            var parts = endpoint.Split(':');
            if (parts.Length != 2)
            {
                ErrorEvent?.Invoke("Invalid endpoint format");
                return;
            }
            string host = parts[0];
            var rawPort = parts[1];//.Trim().Replace("\u200B", "").Replace("\uFEFF", "");
            if (!int.TryParse(rawPort, out int port) || port < 1 || port > 65535) {
                ErrorEvent?.Invoke("Invalid port number");
                return;
            }
            
            var ipEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(host), port);

            var connector = new PinionCore.Network.Tcp.Connector();
            
            connector.Connect(ipEndPoint).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    _Error = $"Connection failed: {task.Exception?.GetBaseException().Message}";
                    
                    
                }
                else
                {
                    var peer = task.Result;
                    _Peer = peer;
                }
            });
        }

        void IStatus.Leave()
        {
            
        }

        void IStatus.Update()
        {
            if(_Error!=null)
            {
                ErrorEvent?.Invoke(_Error);
                _Error = null;
                return;
            }

            if (_Peer != null)
                            {
                SuccessEvent?.Invoke(_Peer);
                _Peer = null;
                return;
            }


        }
    }
}