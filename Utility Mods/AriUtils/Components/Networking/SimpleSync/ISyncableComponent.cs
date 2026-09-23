using System;
using VRageMath;

namespace AriUtils.Components.Networking.SimpleSync
{
    public interface ISyncableComponent
    {
        /// <summary>
        /// Unique ID (among this component type); i.e. EntityId
        /// </summary>
        long UniqueId { get; }
        /// <summary>
        /// Component position; data is sent in sync range
        /// </summary>
        Vector3D Position { get; }
        /// <summary>
        /// Action triggered when the component is closed
        /// </summary>
        event Action OnClose;
    }
}
