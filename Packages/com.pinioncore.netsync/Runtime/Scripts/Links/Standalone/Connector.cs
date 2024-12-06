using PinionCore.Network;
using PinionCore.Remote;
using System;
using UnityEngine;
namespace PinionCore.NetSync.Standalone
{
    [RequireComponent(typeof(Client))]
    public class Connector : MonoBehaviour 
    {
        public Listener Listener;
        private readonly Stream _Stream;
        readonly ReverseStream _ReverseStream;

        bool _Connecting;
        public Connector()
        {
            _Stream = new PinionCore.Network.Stream();
            _ReverseStream = new ReverseStream(_Stream);
        }
        public void Connect()
        {
            this.Listener.Add(_ReverseStream);
            var agent = GetComponent<Client>();
            agent.Enable(_Stream);
            _Connecting = true;
        }

        public void Disconnect()
        {
            var agent = GetComponent<Client>();
            agent.Disable();
            this.Listener.Remove(_ReverseStream);
            _Connecting = false;
        }

        public bool IsConnect()
        {
            return _Connecting;
        }

       
    }

}
