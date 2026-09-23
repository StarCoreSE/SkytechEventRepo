using AriUtils.Components.Networking;
using VRage.Game.Components;

namespace AriUtils.Components
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once UnusedType.Global
    public partial class ServerMain : SessionInstance
    {
        protected override bool LoadOnServer => true;
        protected override bool LoadOnClient => false;

        protected readonly ServerNetwork Network = ServerNetwork.CreateWithOwner<ServerMain>();
    }
}
