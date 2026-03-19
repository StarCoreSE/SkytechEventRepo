using Skytech.Engines.Shared.Utils;
using System;
using System.Collections.Generic;
using AriUtils;
using AriUtils.HUD;
using VRage.Game.Components;

namespace Skytech.Engines.Shared
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation, Priority = int.MinValue)]
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SharedMain : MySessionComponentBase
    {
        public static SharedMain I;
        public HashSet<IAssemblyManager> AssemblyManagers = new HashSet<IAssemblyManager>();

        public override void LoadData()
        {
            try
            {
                I = this;

                if (!GlobalData.CheckShouldLoad(ModContext, modIdFormatted => modIdFormatted.Contains("skytech") && modIdFormatted.Contains("engines")))
                {
                    I = null;
                    return;
                }

                Log.Init(ModContext);
                Log.Info("SharedMain", "Start initialize...");
                Log.IncreaseIndent();

                GlobalData.Init(ModContext);
                GlobalObjectPools.Init();

                Log.DecreaseIndent();
                Log.Info("SharedMain", "Initialized.");
            }
            catch (Exception ex)
            {
                Log.Exception("SharedMain", ex, true);
            }
        }

        public int Ticks = 0;

        public override void UpdateAfterSimulation()
        {
            if (GlobalData.Killswitch)
                return;

            try
            {
                GlobalData.Update();

                foreach (var asm in AssemblyManagers)
                {
                    asm.Update();
                }

                GlobalObjectPools.Update();
                BlockInfo.Update();
                Log.Update();
                Ticks++;
            }
            catch (Exception ex)
            {
                Log.Exception("SharedMain", ex);
            }
        }

        protected override void UnloadData()
        {
            if (GlobalData.Killswitch)
                return;

            try
            {
                Log.Info("SharedMain", "Start unload...");
                Log.IncreaseIndent();

                AssemblyManagers = null;
                BlockInfo.Close();
                GlobalObjectPools.Unload();
                GlobalData.Unload();

                I = null;

                Log.DecreaseIndent();
                Log.Info("SharedMain", "Unloaded.");
            }
            catch (Exception ex)
            {
                Log.Exception("SharedMain", ex, true);
            }
        }
    }
}
