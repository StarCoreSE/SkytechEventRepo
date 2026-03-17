using VRage.Game.Components;
using Skytech.Engines.Shared.Utils;
using Skytech.Engines.Client.Networking;
using System;
using AriUtils;
using Sandbox.ModAPI;
using Skytech.Engines.Client.Interface;
using Skytech.Engines.Shared;

namespace Skytech.Engines.Client
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    // ReSharper disable once UnusedType.Global
    internal class ClientMain : MySessionComponentBase
    {
        public override void LoadData()
        {
            if (MyAPIGateway.Utilities.IsDedicated || GlobalData.Killswitch)
                return;

            try
            {
                Log.Info("ClientMain", "Start initialize...");
                Log.IncreaseIndent();

                new ClientNetwork().LoadData();
                BlockCategoryManager.Init();

                Log.DecreaseIndent();
                Log.Info("ClientMain", "Initialized.");
            }
            catch (Exception ex)
            {
                Log.Exception("ClientMain", ex, true);
            }
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Utilities.IsDedicated || GlobalData.Killswitch)
                return;

            try
            {
                ClientNetwork.I.Update();
            }
            catch (Exception ex)
            {
                Log.Exception("ClientMain", ex);
            }
        }

        public override void Draw()
        {
            if (MyAPIGateway.Utilities.IsDedicated || GlobalData.Killswitch)
                return;

            try
            {
                
            }
            catch (Exception ex)
            {
                Log.Exception("ClientMain", ex);
            }
        }

        protected override void UnloadData()
        {
            if (MyAPIGateway.Utilities.IsDedicated || GlobalData.Killswitch)
                return;

            try
            {
                Log.Info("ClientMain", "Start unload...");
                Log.IncreaseIndent();

                BlockCategoryManager.Close();
                ClientNetwork.I.UnloadData();

                Log.DecreaseIndent();
                Log.Info("ClientMain", "Unloaded.");
                Log.Close();
            }
            catch (Exception ex)
            {
                Log.Exception("ClientMain", ex, true);
            }
        }
    }
}
