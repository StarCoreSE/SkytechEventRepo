using System.Collections.Generic;
using Collections;
using VRage.Game.ModAPI;

namespace Skytech.Engines.Shared.Exhaust
{
    internal interface IExhaustProducer : IClosable
    {
        // TODO isconnected of some kind
        ICollection<FuelEngineExhaust> OutletAssembly { get; }
        FuelEngineExhaust.Exhaust ExhaustProduced { get; }
        IMyCubeBlock Block { get; }

        bool IsOutlet(IMyCubeBlock block);
    }
}
