using PinionCore.Network;
using PinionCore.Remote;
using PinionCore.Remote.Soul;
using System;
using UnityEngine;
namespace PinionCore.NetSync.Standalone
{
    [RequireComponent(typeof(Server))]

    public class Listener : MonoBehaviour , IListenable
    {
        private readonly NotifiableCollection<IStreamable> _Notice;
        public readonly System.Collections.Generic.ICollection<IStreamable> Streams;
        bool _Connect;
        public Listener()
        {
            _Notice = new PinionCore.Remote.NotifiableCollection<IStreamable>();
            Streams = _Notice.Items;
        }

        event Action<IStreamable> IListenable.StreamableEnterEvent
        {
            add
            {
                _Notice.Notifier.Supply += value;
            }

            remove
            {
                _Notice.Notifier.Supply -= value;
            }
        }

        event Action<IStreamable> IListenable.StreamableLeaveEvent
        {
            add
            {
                _Notice.Notifier.Unsupply += value;
            }

            remove
            {
                _Notice.Notifier.Unsupply -= value;
            }
        }

        public void Bind()
        {
            var server = GetComponent<Server>();
            server.Listener.Add(this);
            _Connect = true;
        }

        public void Close()
        {
            _Connect = false;
            var server = GetComponent<Server>();
            server.Listener.Remove(this);
        }

        public bool IsConnect()
        {
            return _Connect;
        }
    }

}
