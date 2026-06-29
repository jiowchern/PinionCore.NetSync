namespace PinionCore.NetSync.TrackerProtocol
{
    // 由 PinionCore.Remote.Tools.Protocol.Sources Source Generator 在本組件內掃描
    // ITracker（連同其繼承的 IObject）生成完整 protocol。
    // Tracker 為實驗性功能，協議定義因此放在 Develop 端，而非乾淨發佈的 Package。
    public static partial class TrackerProtocolCreator
    {
        public static PinionCore.Remote.IProtocol Create()
        {
            PinionCore.Remote.IProtocol protocol = null;
            _Create(ref protocol);
            return protocol;
        }

        [PinionCore.Remote.Protocol.Creator]
        static partial void _Create(ref PinionCore.Remote.IProtocol protocol);
    }
}
