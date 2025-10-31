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
        
        void IConnect.Connect(string endpoint, bool gate)
        {

            
            if (Application.platform == RuntimePlatform.WebGLPlayer && !Application.isEditor )
            {
                _ToWebConnect(endpoint , gate);
            }
            else
            {
                _ToTcpConnect(endpoint, gate);
            }


        }

        private void _ToTcpConnect(string endpoint, bool gate)
        {
            
            var state = new TcpSocketState(endpoint);
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
                agent = PinionCore.Remote.Client.Provider.CreateAgent(protocol);
            }
            _ToGame(agent,stream);
        }

        private void _ToGame(IAgent agent, IStreamable stream)
        {
            var state = new LoopState(this,agent , stream);
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
