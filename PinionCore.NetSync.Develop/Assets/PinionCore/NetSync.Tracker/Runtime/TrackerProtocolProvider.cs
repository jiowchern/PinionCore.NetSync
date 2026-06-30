using PinionCore.NetSync.Extensions;
using System.Linq;
using UnityEngine;

namespace PinionCore.NetSync.TrackerProtocol
{
    /// <summary>
    /// 提供含 Tracker（ITracker + 其繼承的 IObject）的完整 protocol。
    /// 建立一顆此資產，指派到 Server / Client 的 Provider 欄位即可。
    /// </summary>
    [CreateAssetMenu(menuName = "PinionCore/Tracker Protocol Provider", fileName = "TrackerProtocol")]
    public class TrackerProtocolProvider : PinionCore.NetSync.ProtocolProvider
    {

        readonly PinionCore.Remote.IProtocol _Protocol;

        public TrackerProtocolProvider() {
            _Protocol = TrackerProtocolCreator.Create();           
        }
       
        public override PinionCore.Remote.IProtocol Get()
        {
            return _Protocol;
        }
    }
}
