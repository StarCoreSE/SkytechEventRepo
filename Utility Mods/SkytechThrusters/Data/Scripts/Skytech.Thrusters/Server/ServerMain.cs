using System;
using Skytech.Thrusters.Server.Networking;
using Skytech.Thrusters.Shared.Utils;
using Sandbox.ModAPI;
using Skytech.Thrusters.Shared;
using VRage.Game.Components;

namespace Skytech.Thrusters.Server
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ServerMain : MySessionComponentBase
    {
        public static ServerMain I;

        private bool _doneDelayedInit = false;

        public override void LoadData()
        {
            if (!MyAPIGateway.Session.IsServer || GlobalData.Killswitch)
                return;

            try
            {
                Log.Info("ServerMain", "Start initialize...");
                Log.IncreaseIndent();

                I = this;

                new ServerNetwork().LoadData();

                Log.DecreaseIndent();
                Log.Info("ServerMain", "Initialized.");
            }
            catch (Exception ex)
            {
                Log.Exception("ServerMain", ex, true);
            }
        }

        public void DelayedInit()
        {
            _doneDelayedInit = true;
        }

        protected override void UnloadData()
        {
            if (!MyAPIGateway.Session.IsServer || GlobalData.Killswitch)
                return;

            try
            {
                Log.Info("ServerMain", "Start unload...");
                Log.IncreaseIndent();

                ServerNetwork.I.UnloadData();

                I = null;
                Log.DecreaseIndent();
                Log.Info("ServerMain", "Unloaded.");
            }
            catch (Exception ex)
            {
                Log.Exception("ServerMain", ex, true);
            }
        }


        public override void UpdateAfterSimulation()
        {
            if (!MyAPIGateway.Session.IsServer || GlobalData.Killswitch)
                return;

            try
            {
                if (!_doneDelayedInit)
                    DelayedInit();

                ServerNetwork.I.Update();
            }
            catch (Exception ex)
            {
                Log.Exception("ServerMain", ex);
            }
        }
    }
}
