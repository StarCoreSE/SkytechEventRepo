using VRage.Game.Components;

namespace AriUtils.Components
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class SharedMain : SessionInstance
    {
        protected override bool LoadOnServer => true;
        protected override bool LoadOnClient => true;
    }
}
