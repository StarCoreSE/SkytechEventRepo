using System;
using VRage.ModAPI;
using VRageMath;

namespace AriUtils.Components.Networking.SimpleSync
{
    public class EntitySimpleSyncWrapper : ISyncableComponent
    {
        public long UniqueId { get; }
        public Vector3D Position => Entity.GetPosition();
        public event Action OnClose;

        public readonly IMyEntity Entity;

        public EntitySimpleSyncWrapper(IMyEntity entity)
        {
            Entity = entity;
            UniqueId = Entity.EntityId;
            entity.OnMarkForClose += OnEntityMFC;
        }

        private void OnEntityMFC(IMyEntity discard)
        {
            OnClose?.Invoke();
        }
    }
}
