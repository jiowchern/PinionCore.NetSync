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
        private readonly System.Net.IPEndPoint _EndPoint;
        public System.Action<IStreamable> SuccessEvent;
        public System.Action<string> ErrorEvent;
        string _Error;
        Peer _Peer;
        public TcpSocketState(System.Net.IPEndPoint endPoint)
        {
            _EndPoint = endPoint;
        }



        void IStatus.Enter()
        {
            var ipEndPoint = _EndPoint;

            var connector = new PinionCore.Network.Tcp.Connector();

            connector.ConnectAsync(ipEndPoint).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    _Error = $"Connection failed: {task.Exception?.GetBaseException().Message}";


                }
                else
                {
                    var result = task.Result;
                    if (result.Exception != null)
                    {
                        _Error = $"Connection failed: {result.Exception.Message}";
                    }
                    else
                    {
                        _Peer = result.Peer;
                    }
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