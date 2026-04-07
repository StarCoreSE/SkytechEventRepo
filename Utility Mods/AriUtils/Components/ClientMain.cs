using VRage.Game.Components;

namespace AriUtils.Components
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    // ReSharper disable once UnusedType.Global
    public partial class ClientMain : SessionInstance
    {
        protected override bool LoadOnServer => false;
        protected override bool LoadOnClient => true;
    }
}
