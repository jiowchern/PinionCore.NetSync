using PinionCore.Network;
using PinionCore.Remote;
using PinionCore.Remote.Soul;
using System;
using UnityEngine;
namespace PinionCore.NetSync.Standalone
{
    [RequireComponent(typeof(Server))]

    public class Listener : MonoBehaviour , IListenable , IListenerEditor
    {
        private readonly NotifiableCollection<IStreamable> _Notice;
        public readonly System.Collections.Generic.ICollection<IStreamable> Streams;
        bool _Listening;

        bool IListenerEditor.IsActive => _Listening;

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

        event Action<int> IListenerEditor.DataReceivedEvent
        {
            add
            {
                
            }

            remove
            {
                
            }
        }

        event Action<int> IListenerEditor.DataSendEvent
        {
            add
            {
                
            }

            remove
            {
                
            }
        }

        public void Bind()
        {
            var server = GetComponent<Server>();
            server.Listener.Add(this);
            _Listening = true;
        }

        public void Close()
        {
            _Listening = false;
            var server = GetComponent<Server>();
            server.Listener.Remove(this);
        }

        public bool IsConnect()
        {
            return _Listening;
        }
    }

}
