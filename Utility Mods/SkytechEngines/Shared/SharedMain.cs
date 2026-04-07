using System;
using Skytech.Engines.Shared.Exhaust;
using Skytech.Engines.Shared.Fuel;

namespace AriUtils.Components
{
    public partial class SharedMain
    {
        private TurboManager _tm = TurboManager.Create<SharedMain>();
        private FuelTankManager _ftm = FuelTankManager.Create<SharedMain>();
    }
}
