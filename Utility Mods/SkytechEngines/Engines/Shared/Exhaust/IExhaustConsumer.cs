using Collections;
using VRage.Game.ModAPI;

namespace Skytech.Engines.Shared.Exhaust
{
    internal interface IExhaustConsumer : IClosable
    {
        // TODO isconnected of some kind
        void UpdateExhaust(FuelEngineExhaust.Exhaust available);
        IMyCubeBlock Block { get; }

        bool IsInlet(IMyCubeBlock block);
    }
}
