using System;
using System.Collections.Generic;
using AriUtils;
using VRage.Game.ModAPI;

namespace Skytech.Engines.Shared.Fuel
{
    internal class FuelTankManager : SingletonBase<FuelTankManager>
    {
        private Dictionary<IMyCubeGrid, GridFuelTank> _fuelTanks = new Dictionary<IMyCubeGrid, GridFuelTank>();

        private static readonly string[] FuelTankSubtypes =
        {
            "ST_T_FuelTankCenter",
            "ST_T_FuelTankCorner",
            "ST_T_FuelTankEdge",
        };

        public override void Init()
        {
            GlobalData.RegisterOnBlockAdded(OnBlockAdded);
            GlobalData.RegisterOnBlockRemoved(OnBlockRemoved);
        }

        public override void Update()
        {
            
        }

        public override void Unload()
        {
            base.Unload();
            GlobalData.UnregisterOnBlockAdded(OnBlockAdded);
            GlobalData.UnregisterOnBlockAdded(OnBlockRemoved);
        }

        public bool TryGetTank(IMyCubeGrid grid, out GridFuelTank tank) => _fuelTanks.TryGetValue(grid, out tank);

        private void OnBlockAdded(IMyCubeBlock block)
        {
            if (!FuelTankSubtypes.Contains(block.BlockDefinition.SubtypeName))
                return;

            GridFuelTank tank;
            if (!_fuelTanks.TryGetValue(block.CubeGrid, out tank))
            {
                tank = new GridFuelTank(block.CubeGrid);
                _fuelTanks.Add(block.CubeGrid, tank);
            }

            tank.AddBlock(block);
        }

        private void OnBlockRemoved(IMyCubeBlock block)
        {
            if (!FuelTankSubtypes.Contains(block.BlockDefinition.SubtypeName))
                return;

            GridFuelTank tank;
            if (!_fuelTanks.TryGetValue(block.CubeGrid, out tank))
                return;

            tank.RemoveBlock(block);
            if (tank.IsClosed)
            {
                _fuelTanks.Remove(block.CubeGrid);
            }
        }
    }
}
