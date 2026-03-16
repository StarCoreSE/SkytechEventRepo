using Skytech.Engines.Shared.Utils;
using System;
using System.Collections.Generic;
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

                if (!GlobalData.CheckShouldLoad())
                {
                    I = null;
                    return;
                }

                Log.Init(ModContext);
                Log.Info("SharedMain", "Start initialize...");
                Log.IncreaseIndent();

                GlobalData.Init();
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
                if (Ticks % 10 == 0)
                {
                    GlobalData.UpdatePlayers();
                }

                foreach (var asm in AssemblyManagers)
                {
                    asm.Update();
                }

                GlobalObjectPools.Update();
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
