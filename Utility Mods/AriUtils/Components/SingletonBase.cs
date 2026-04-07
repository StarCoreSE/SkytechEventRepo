using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace AriUtils
{
    /// <summary>
    /// Generic singleton class
    /// </summary>
    /// <typeparam name="TThis">This class</typeparam>
    public abstract class SingletonBase<TThis> : ISingleton
        where TThis : SingletonBase<TThis>, new()
    {
        public static TThis I { get; private set; }

        public IMyModContext ModContext { get; set; }
        public virtual int InitPriority => 0;

        private readonly int _hashCode;

        public static TThis Create<TOwner>() where TOwner : SessionInstance
        {
            if (I != null)
                throw new Exception($"Singleton {typeof(TThis).PrettyName()} instantiated multiple times!");
            I = new TThis();

            SessionInstance.RegisterSingleton<TOwner>(I);
            return I;
        }

        protected SingletonBase()
        {
            _hashCode = typeof(TThis).GetHashCode();
        }

        public abstract void Init();
        public abstract void Update();
        public virtual void Unload()
        {
            I = null;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }

    public interface ISingleton
    {
        int InitPriority { get; }

        void Init();
        void Update();
        void Unload();

        IMyModContext ModContext { get; set; }
    }

    public abstract class SessionInstance : MySessionComponentBase
    {
        public static int Ticks = 0;

        private static Dictionary<Type, SessionInstance> _instances = new Dictionary<Type, SessionInstance>();
        private static Dictionary<Type, List<ISingleton>> _singletonsAwaitingInit = new Dictionary<Type, List<ISingleton>>();

        /// <summary>
        /// If true, boots on dedicated
        /// </summary>
        protected abstract bool LoadOnServer { get; }
        /// <summary>
        /// If true, boots on client.
        /// </summary>
        protected abstract bool LoadOnClient { get; }

        protected bool _skipRun = false;
        private bool _thisSessionLoaded;
        private string _instanceName = "AWAITING INIT";
        private HashSet<ISingleton> _singletons = new HashSet<ISingleton>();

        public static void RegisterSingleton<TOwner>(ISingleton singleton) where TOwner : SessionInstance
        {
            SessionInstance owner;
            if (_instances.TryGetValue(typeof(TOwner), out owner))
            {
                owner._RegisterSingleton(singleton);
            }
            else
            {
                List<ISingleton> singletons;
                if (!_singletonsAwaitingInit.TryGetValue(typeof(TOwner), out singletons))
                {
                    singletons = new List<ISingleton>();
                    _singletonsAwaitingInit.Add(typeof(TOwner), singletons);
                }
                singletons.Add(singleton);
            }
        }

        public override void LoadData()
        {
            _skipRun = !((LoadOnServer && MyAPIGateway.Session.IsServer) || (LoadOnClient && !MyAPIGateway.Utilities.IsDedicated));
            _instanceName = this.GetType().Name;

            _instances.Add(GetType(), this);
            List<ISingleton> singletons;
            if (_singletonsAwaitingInit.TryGetValue(GetType(), out singletons))
            {
                foreach (var singleton in singletons)
                {
                    _RegisterSingleton(singleton);
                }
            }
            _singletonsAwaitingInit.Remove(GetType());

            _thisSessionLoaded = true;

            // always check first
            if (!GlobalData.CheckShouldLoad(ModContext))
                return;
            GlobalData.Init(ModContext);

            if (_skipRun)
                return;

            Log.Info(_instanceName, "Start LoadData...");
            Log.IncreaseIndent();
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Existing singletons init
            try
            {
                foreach (var singleton in _singletons)
                {
                    _LoadSingleton(singleton);
                }
            }
            catch (Exception ex)
            {
                Log.Exception(_instanceName, ex, true);
            }

            sw.Stop();
            Log.DecreaseIndent();
            Log.Info(_instanceName, $"Finished LoadData ({(double) sw.ElapsedTicks/TimeSpan.TicksPerMillisecond:N}ms)");
        }

        public override void UpdateAfterSimulation()
        {
            if (GlobalData.Killswitch || _skipRun)
                return;

            try
            {
                foreach (var singleton in _singletons)
                {
                    singleton.Update();
                }
            }
            catch (Exception ex)
            {
                Log.Exception(_instanceName, ex, false);
            }

            Log.Update();
            Ticks++;
        }

        protected override void UnloadData()
        {
            if (!GlobalData.Killswitch && !_skipRun)
            {
                Log.Info(_instanceName, "Start UnloadData...");
                Log.IncreaseIndent();
                Stopwatch sw = new Stopwatch();
                sw.Start();

                try
                {
                    foreach (var singleton in _singletons)
                    {
                        Log.Info(_instanceName, $"Unloading singleton {singleton.GetType().PrettyName()}:");
                        Log.IncreaseIndent();
                        singleton.Unload();
                        Log.DecreaseIndent();
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(_instanceName, ex, true);
                }

                sw.Stop();
                Log.DecreaseIndent();
                Log.Info(_instanceName,
                    $"Finished UnloadData ({(double)sw.ElapsedTicks / TimeSpan.TicksPerMillisecond:N}ms)");
            }

            // always unload
            _instances.Remove(GetType());
            _thisSessionLoaded = false;
            GlobalData.Unload();
        }

        private void _RegisterSingleton(ISingleton singleton)
        {
            if (!_singletons.Add(singleton))
            {
                Log.Info(_instanceName, "Existing instance hashcodes:");
                Log.IncreaseIndent();
                foreach (var singletonn in _singletons)
                    Log.Info(_instanceName, $"{singletonn.GetType().PrettyName()}: {singletonn.GetHashCode()}");
                Log.DecreaseIndent();
                Log.Exception(_instanceName, new Exception($"Failed to register singleton {singleton.GetType().PrettyName()} (already exists! {singleton.GetHashCode()})"), true);
                return;
            }

            singleton.ModContext = ModContext;

            Log.Info(_instanceName, $"Registered singleton {singleton.GetType().PrettyName()}.");

            if (_thisSessionLoaded)
            {
                try
                {
                    _LoadSingleton(singleton);
                }
                catch (Exception ex)
                {
                    Log.Exception(_instanceName, new Exception($"Failed to register singleton {singleton.GetType().PrettyName()} (load failed)", ex), true);
                }
            }
        }

        private void _LoadSingleton(ISingleton singleton)
        {
            Log.Info(_instanceName, $"Loading singleton {singleton.GetType().PrettyName()}:");
            Log.IncreaseIndent();
            singleton.Init();
            Log.DecreaseIndent();
        }
    }
}
