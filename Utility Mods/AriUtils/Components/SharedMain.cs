using VRage.Game.Components;

namespace AriUtils.Components
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class SharedMain : SessionInstance
    {
        protected override bool LoadOnServer => true;
        protected override bool LoadOnClient => true;

        // GlobalData is inited and unloaded by every SingletonBase, but SharedMain drives updates. hacky and cringe but i'm the only one that'll look at this.
        private GlobalData _globalData = GlobalData.CreateWithOwner<SharedMain>();
    }
}
