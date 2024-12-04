
using PinionCore.NetSync.Syncs.Protocols;
using PinionCore.Remote;

using UnityEngine;



namespace PinionCore.NetSync.Syncs.Souls
{
    public class Soul : MonoBehaviour , IObject
    {
        IBinder _Binder;
        private ISoul _Soul;
        readonly System.Collections.Generic.HashSet<ISoul> _SoulSet;  

        Property<int> IObject.Id => new Property<int>(gameObject.GetInstanceID());

        public Soul()
        {
            _SoulSet = new System.Collections.Generic.HashSet<ISoul>();
        }

        public void Initial(IBinder binder)
        {
            _Binder = binder;
            _Soul = _Binder.Bind<IObject>(this);
        }

        public void Final(IBinder binder)
        {
            if(_Binder == binder)
            {
                _Binder.Unbind(_Soul);
                _Binder = null;
            }
        }

        public ISoul Bind<T>(T soul) where T : class , IObject
        {
            if (soul.Id != gameObject.GetInstanceID())
                return null;
            var s= _Binder.Bind(soul);
            _SoulSet.Add(s);
            return s;
        }

        public void Unbind(ISoul soul)
        {
            if (_SoulSet.Contains(soul))
            {
                return;
            }
            _Binder.Unbind(soul);
            _SoulSet.Remove(soul);
        }

    }

}
