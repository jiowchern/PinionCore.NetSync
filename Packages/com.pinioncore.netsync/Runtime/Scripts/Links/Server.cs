﻿using PinionCore.NetSync.Extensions;
using PinionCore.Remote;
using PinionCore.Remote.Soul;

using Unity.Properties;
using UnityEngine;

namespace PinionCore.NetSync
{

    public class Server : MonoBehaviour , PinionCore.Remote.IEntry
    {
        public readonly IProtocol Protocol;
        public readonly Linstener Listener;
        public struct BinderCommand
        {
            public enum OperatorStatus
            {
                Add,
                Remove
            }
            public OperatorStatus Status;
            public IBinder Binder;
        }
        private readonly System.Collections.Concurrent.ConcurrentQueue<BinderCommand> _BinderOperator;
        public UnityEngine.Events.UnityEvent<BinderCommand> BinderEvent;                
        private readonly SyncService _Service;
        
        public Server() {
            PinionCore.Utility.Log.Instance.RecordEvent += (msg) => Debug.Log(msg);
            _BinderOperator = new System.Collections.Concurrent.ConcurrentQueue<BinderCommand>();
            Listener = new Linstener();
            Protocol = ProtocolCreator.Create();
            _Service = new PinionCore.Remote.Soul.SyncService(this, new UserProvider(Protocol, new Serializer(Protocol.SerializeTypes), Listener, new PinionCore.Remote.InternalSerializer(), PinionCore.Memorys.PoolProvider.Shared));            
        }
         [CreateProperty] public string Hash => Protocol.VersionCode.ToHexString();

        void IBinderProvider.RegisterClientBinder(IBinder binder)
        {
            UnityEngine.Debug.Log($"RegisterClientBinder {binder.GetHashCode()}");
            _BinderOperator.Enqueue(new BinderCommand { Status = BinderCommand.OperatorStatus.Add, Binder = binder });
        }

        void IBinderProvider.UnregisterClientBinder(IBinder binder)
        {
            
            UnityEngine.Debug.Log($"UnregisterClientBinder {binder.GetHashCode()} ");
            _BinderOperator.Enqueue(new BinderCommand { Status = BinderCommand.OperatorStatus.Remove, Binder = binder });
        }

        void IEntry.Update()
        {
            
        }
        
        public void Update()
        {            
            _Service.Update();
            while (_BinderOperator.TryDequeue(out var op))
            {
                BinderEvent.Invoke(op);                
            }
        }
    }
}
