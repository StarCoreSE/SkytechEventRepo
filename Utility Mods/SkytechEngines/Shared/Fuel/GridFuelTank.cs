using System.Collections.Generic;
using System.Text;
using AriUtils.HUD;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace Skytech.Engines.Shared.Fuel
{
    internal class GridFuelTank
    {
        public const float FuelPerTank = 2500;


        public readonly IMyCubeGrid Grid;
        public double FuelLevel { get; private set; } = 1;
        public bool IsClosed { get; private set; }

        public double FuelAmount => FuelLevel * FuelPerTank * _tankBlocks.Count;
        public double FullFuelAmount => FuelPerTank * _tankBlocks.Count;

        private double _fuelRate = 0;
        private int _lastFuelUpdate = -1;

        private HashSet<long> _tankBlocks = new HashSet<long>();

        public GridFuelTank(IMyCubeGrid grid)
        {
            Grid = grid;
        }

        public void AddBlock(IMyCubeBlock block)
        {
            if (!_tankBlocks.Add(block.EntityId))
                return;

            BlockInfo.Register(block, GetBlockInfo);
            FuelLevel = (_tankBlocks.Count - 1) * FuelPerTank * FuelLevel;
        }

        private void GetBlockInfo(IMyCubeBlock block, StringBuilder sb)
        {
            sb.AppendLine($"Fuel: {FuelAmount:N1}/{FullFuelAmount:N0}L ({FuelLevel*100:N0}%)");
            double rate = CalcFuelRate();
            sb.AppendLine($"Fuel Rate: {(rate > 0 ? "+" : "")}{rate:N}L/s");
        }

        public void RemoveBlock(IMyCubeBlock block)
        {
            if (!_tankBlocks.Remove(block.EntityId))
                return;

            BlockInfo.Unregister(block, GetBlockInfo);

            if (_tankBlocks.Count == 0)
                IsClosed = true;
        }

        public bool UpdateFuel(double fuelDelta)
        {
            fuelDelta /= 60;

            double amt = FuelAmount;
            if (amt < -fuelDelta)
            {
                FuelLevel = 0;

                _fuelRate = 0;
                _lastFuelUpdate = MyAPIGateway.Session.GameplayFrameCounter;

                return false;
            }

            FuelLevel = (amt + fuelDelta) / FullFuelAmount;

            if (_lastFuelUpdate != MyAPIGateway.Session.GameplayFrameCounter)
                _fuelRate = 0;

            _fuelRate += fuelDelta;
            _lastFuelUpdate = MyAPIGateway.Session.GameplayFrameCounter;
            return true;
        }

        public void Refill() => FuelLevel = 1;

        public double CalcFuelRate()
        {
            double rate = _fuelRate * 60;

            if (_lastFuelUpdate != MyAPIGateway.Session.GameplayFrameCounter)
                _fuelRate = 0;

            return rate;
        }
    }
}
