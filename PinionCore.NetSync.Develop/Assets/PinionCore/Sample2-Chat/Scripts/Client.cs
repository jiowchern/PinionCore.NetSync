using PinionCore.Consoles.Chat1.Common;
using PinionCore.Extensions;
using PinionCore.Network;
using PinionCore.Remote;
using PinionCore.Remote.Ghost;
using PinionCore.Remote.Standalone;
using PinionCore.Utility;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
namespace PinionCore.NetSync.Samples.Chat
{
    public class Client : MonoBehaviour , IConnect , IStatus
    {
        

        readonly PinionCore.Utility.StatusMachine _Machine;

        public UnityEvent<float> OnPing;
        public UnityEvent<string> OnErrorMessage;
        public UnityEvent<ILogin> OnLoginSupply;
        public UnityEvent<ILogin> OnLoginUnsupply;
        public UnityEvent<IPlayer> OnPlayerSupply;
        public UnityEvent<IPlayer> OnPlayerUnsupply;
        public UnityEvent<IConnect> OnConnectSupply;
        public UnityEvent<IConnect> OnConnectUnsupply;
        public Client()
        {
            _Machine = new Utility.StatusMachine();
        }
        private void Start()
        {
            

            _ToConnect();
        }

        

        private void Update()
        {
            _Machine.Update();
        }
        private void OnDestroy()
        {
            _Machine.Termination();
        }
        
        void IConnect.Connect(PinionCore.NetSync.ConnectionConfig config, bool gate)
        {
            if (config == null)
            {
                OnErrorMessage.Invoke("Connection config is null.");
                _ToConnect();
                return;
            }

            // 由 config 的子型別決定傳輸層,不再依賴端點字串與平台判斷。
            if (config is PinionCore.NetSync.Web.WebConnectionConfig web)
            {
                _ToWebConnect(web.Url, gate);
            }
            else if (config is PinionCore.NetSync.Tcp.TcpConnectionConfig tcp)
            {
                var endPoint = tcp.ToEndPoint();
                if (endPoint == null)
                {
                    OnErrorMessage.Invoke($"Invalid TCP address: {tcp.Host}");
                    _ToConnect();
                    return;
                }
                _ToTcpConnect(endPoint, gate);
            }
            else
            {
                OnErrorMessage.Invoke($"Unsupported connection config type: {config.GetType().Name}");
                _ToConnect();
            }
        }

        private void _ToTcpConnect(System.Net.IPEndPoint endPoint, bool gate)
        {

            var state = new TcpSocketState(endPoint);
            state.SuccessEvent += (stream) =>
            {
                _ToSetupMode(stream, gate);
            };
            state.ErrorEvent += (err) =>
            {
                _ToConnect();
                OnErrorMessage.Invoke(err);
            };
            _Machine.Push(state);
        }

        private void _ToWebConnect(string endpoint, bool gate)
        {
            var state = new WebSocketState(endpoint);
            state.SuccessEvent += (stream) =>
            {
                _ToSetupMode(stream , gate);
            };
            state.ErrorEvent += (err) =>
            {
                
                OnErrorMessage.Invoke(err);
                _ToConnect();
            };
            _Machine.Push(state);
        }

        private void _ToSetupMode(IStreamable stream, bool gate)
        {
            var protocol = PinionCore.Consoles.Chat1.Common.ProtocolCreator.Create();
            UnityEngine.Debug.Log($"Use Protocol Version: {protocol.VersionCode.ToMd5String()}");
            IAgent agent = null;
            if (gate)
            {
                var pool = new PinionCore.Remote.Gateway.Hosts.AgentPool(protocol);
                agent = new PinionCore.Remote.Gateway.Agent(pool);
            }
            else
            {
                agent = new PinionCore.Remote.Ghost.Agent(protocol);
            }
            _ToGame(agent,stream);
        }

        private void _ToGame(IAgent agent, IStreamable stream)
        {
            var state = new LoopState(this,agent , stream);
            state.OnPingChanged += (ping) =>
            {
                OnPing.Invoke(ping);
            };
            _Machine.Push(state);
        }

        void IStatus.Enter()
        {
            OnConnectSupply?.Invoke(this);
        }

        void IStatus.Leave()
        {
            OnConnectUnsupply?.Invoke(this);
        }

        void IStatus.Update()
        {
            
        }

        private void _ToConnect()
        {
            
            _Machine.Push(this);
        }
    }

}
