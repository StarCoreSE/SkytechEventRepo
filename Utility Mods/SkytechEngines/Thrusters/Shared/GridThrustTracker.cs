using System;
using System.Collections.Generic;
using AriUtils;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace Skytech.Thrusters.Shared
{
    internal class GridThrustTracker : SingletonBase<GridThrustTracker>
    {
        private readonly Dictionary<IMyCubeGrid, List<MyThrust>> _gridThrusters = new Dictionary<IMyCubeGrid, List<MyThrust>>();
        private int _lastCThrustUpdate = -1;
        private readonly Dictionary<IMyCubeGrid, float> _gridCounterThrust = new Dictionary<IMyCubeGrid, float>();

        public override void Init()
        {
            GlobalData.RegisterOnBlockAdded(OnBlockAdded);
            GlobalData.RegisterOnBlockRemoved(OnBlockRemoved);
        }

        public override void Update()
        {
            _gridCounterThrust.Clear();
        }

        public override void Unload()
        {
            base.Unload();
            GlobalData.UnregisterOnBlockAdded(OnBlockAdded);
            GlobalData.UnregisterOnBlockRemoved(OnBlockRemoved);
        }

        public float GetCounterGravityThrust(IMyCubeGrid grid)
        {
            List<MyThrust> thrustList;
            if (!_gridThrusters.TryGetValue(grid, out thrustList))
                return 0;

            // vain attempt at caching
            float counterThrust = 0;
            if (_lastCThrustUpdate < MyAPIGateway.Session.GameplayFrameCounter)
            {
                _gridCounterThrust.Clear();
            }
            else if (_gridCounterThrust.TryGetValue(grid, out counterThrust))
            {
                return counterThrust;
            }
            Vector3 invGravityDir = -grid.NaturalGravity.Normalized();

            foreach (var thrust in thrustList)
            {
                float thrustForce = thrust.CurrentStrength * thrust.BlockDefinition.ForceMagnitude * ((IMyThrust) thrust).ThrustMultiplier;
                counterThrust += Math.Max(0, Vector3.Dot(thrust.WorldMatrix.Backward, invGravityDir) * thrustForce); // assumes dampeners are on
            }

            _lastCThrustUpdate = MyAPIGateway.Session.GameplayFrameCounter;
            _gridCounterThrust[grid] = counterThrust; // probably not thread safe lol lmao
            return counterThrust;
        }

        private void OnBlockAdded(IMyCubeBlock block)
        {
            MyThrust thrust = block as MyThrust;
            if (thrust == null)
                return;

            List<MyThrust> thrustList;
            if (!_gridThrusters.TryGetValue(thrust.CubeGrid, out thrustList))
            {
                thrustList = new List<MyThrust>();
                _gridThrusters.Add(thrust.CubeGrid, thrustList);
            }
            thrustList.Add(thrust);
        }

        private void OnBlockRemoved(IMyCubeBlock block)
        {
            MyThrust thrust = block as MyThrust;
            if (thrust == null)
                return;

            List<MyThrust> thrustList;
            if (!_gridThrusters.TryGetValue(thrust.CubeGrid, out thrustList))
                return;
            if (thrustList.Count <= 1)
            {
                _gridThrusters.Remove(thrust.CubeGrid);
            }
            else
            {
                thrustList.Remove(thrust);
            }
        }
    }
}
