using System;

namespace Skytech.Engines.Shared.Exhaust
{
    internal interface IExhaustConsumer
    {
        /// <summary>
        /// Should be const! And same for all!
        /// </summary>
        float MaxPressureUsage { get; }
        void UpdateExhaust(FuelEngineExhaust.Exhaust available, out float pressureConsumed);
        Action<IExhaustConsumer> OnClose { get; set; }
    }
}
