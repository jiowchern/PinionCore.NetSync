using PinionCore.NetSync.Syncs.Protocols;
using PinionCore.NetSync.Syncs.Protocols.Trackers;
using PinionCore.Remote;
using System;
using UnityEngine;



namespace PinionCore.NetSync.Syncs.Souls.Trackers
{
    public class TrackerSender : MonoBehaviour, ITracker
    {
        public float Interval;
        public uint Scale;
        private ISoul _Soul;

        System.Action _Update;


        [SerializeField]
        public Step[] Steps;
        Property<int> IObject.Id => new Property<int>(gameObject.GetInstanceID());

        event Action<ZipTracker> _OnTrackerEvent;
        event Action<ZipTracker> ITracker.OnTrackerEvent
        {
            add
            {
                UnityEngine.Debug.Log("TrackerSender.OnTrackerEvent+=");
                _OnTrackerEvent += value;
            }

            remove
            {
                UnityEngine.Debug.Log("TrackerSender.OnTrackerEvent-=");
                _OnTrackerEvent -= value;

            }
        }
        public TrackerSender()
        {
            _Update = ()=> { };
        }
        public void Update()
        {
            _Update();
        }
        public void Run(System.Collections.Generic.IEnumerable<Step> path)
        {
            var tracker = new Tracker(Interval, path, System.DateTime.Now.Ticks);
            _Run(tracker);
        }
        public void Run(System.Collections.Generic.IEnumerable<Vector3> path)
        {

            var tracker = new Tracker(Interval, path, System.DateTime.Now.Ticks);
            _Run(tracker);
        }

        private void _Run(Tracker tracker)
        {
            var zip = tracker.Zip(Scale);

            PinionCore.Remote.ISerializable trackerSerializer = new PinionCore.Remote.Serializer(ProtocolCreator.Create().SerializeTypes);
            var buf = trackerSerializer.Serialize(typeof(ZipTracker), zip);
            UnityEngine.Debug.Log("TrackerSender.Run: " + buf.Count);

            _Update = () =>
            {
                var ticks = System.DateTime.Now.Ticks;
                var pos = tracker.Sample(ticks, FinalState.Stop);
                transform.localPosition = pos;
                if (ticks > tracker.EndTime)
                {
                    _Update = () => { };
                }
            };
            _OnTrackerEvent?.Invoke(zip);
        }

        public void Start()
        {
            _Soul = gameObject.Bind<ITracker>(this);
        }
        public void OnDestroy()
        {
            gameObject.Unbind(_Soul);
        }


    }
}
