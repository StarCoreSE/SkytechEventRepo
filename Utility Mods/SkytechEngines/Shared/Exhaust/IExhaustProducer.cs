using System.Collections.Generic;
using Collections;
using VRage.Game.ModAPI;

namespace Skytech.Engines.Shared.Exhaust
{
    internal interface IExhaustProducer : IClosable
    {
        // TODO isconnected of some kind
        List<FuelEngineExhaust> OutletAssembly { get; set; }
        FuelEngineExhaust.Exhaust ExhaustProduced { get; }
        IMyCubeBlock Block { get; }

        bool IsOutlet(IMyCubeBlock block);
    }
}
