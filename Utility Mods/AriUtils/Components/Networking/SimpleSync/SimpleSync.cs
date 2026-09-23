using System;
using Sandbox.ModAPI;

namespace AriUtils.Components.Networking.SimpleSync
{
    /// <summary>
    /// Simple automatic sync class. Similar to MySync, but works with any serializable type.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TComponent"></typeparam>
    public class SimpleSync<TValue, TComponent> : ISimpleSync where TComponent : ISyncableComponent
    {
        /// <summary>
        /// Unique SyncId for this SimpleSync
        /// </summary>
        public long SyncId { get; set; }
        /// <summary>
        /// Invoked whenever <see cref="Value"/> is modified. Argument 1 is the new value, argument 2 is true if the change was from network.
        /// </summary>
        public Action<TValue, bool> OnValueChanged = null;

        public Func<TValue, TValue> Validate = null;

        private ISyncableComponent _component;
        public ISyncableComponent Component
        {
            get
            {
                return _component;
            }
            set
            {
                if (value?.Equals(_component) ?? true) // this is so gonna cause problems hehe
                    return;

                if (_component != null)
                {
                    SimpleSyncManager.I.UnregisterSync(this);
                    _component.OnClose -= OnComponentClosed;
                }

                _component = value;
                SimpleSyncManager.I.RegisterSync(this, _component.UniqueId);
                _component.OnClose += OnComponentClosed;

                if (MyAPIGateway.Session.IsServer)
                    SendUpdate();
            }
        }

        private void OnComponentClosed() => SimpleSyncManager.I.UnregisterSync(this);

        private TValue _value;

        public TValue Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (value.Equals(_value))
                    return;
                _value = Validate != null ? Validate.Invoke(value) : value;

                SendUpdate();
                OnValueChanged?.Invoke(value, false);
            }
        }

        public SimpleSync(TValue value, Action<TValue, bool> onValueChanged) : this(value)
        {
            OnValueChanged = onValueChanged;
        }

        public SimpleSync(TValue value)
        {
            _value = value;
        }

        /// <summary>
        /// Updates the SimpleSync from network.
        /// </summary>
        /// <param name="contents"></param>
        public void UpdateFromNetwork(byte[] contents)
        {
            _value = MyAPIGateway.Utilities.SerializeFromBinary<TValue>(contents);
            OnValueChanged?.Invoke(_value, true);
        }

        private void SendUpdate()
        {
            if (!MyAPIGateway.Multiplayer.MultiplayerActive || Component == null)
                return;

            var packet = new SimpleSyncManager.InternalSimpleSyncBothWays
            {
                SyncId = SyncId,
                Contents = MyAPIGateway.Utilities.SerializeToBinary(_value)
            };
            if (MyAPIGateway.Session.IsServer)
            {
                ServerNetwork.SendToEveryoneInSync(packet, Component.Position);
            }
            else
            {
                ClientNetwork.SendToServer(packet);
            }
        }

        public static implicit operator TValue(SimpleSync<TValue, TComponent> sync) => sync._value;
    }
}
