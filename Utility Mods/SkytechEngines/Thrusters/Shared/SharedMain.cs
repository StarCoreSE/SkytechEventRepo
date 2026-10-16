// ReSharper disable once CheckNamespace

using Skytech.Thrusters.Shared;

namespace AriUtils.Components
{
    public partial class SharedMain
    {
        internal GridThrustTracker GridThrustTracker = GridThrustTracker.CreateWithOwner<SharedMain>();
    }
}
