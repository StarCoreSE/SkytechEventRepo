using System;

namespace Skytech.Engines.Shared.Exhaust
{
    internal interface IExhaustProducer
    {
        FuelEngineExhaust.Exhaust GetExhaustProduced(int id);
        Action<IExhaustProducer> OnClose { get; set; }
    }
}
