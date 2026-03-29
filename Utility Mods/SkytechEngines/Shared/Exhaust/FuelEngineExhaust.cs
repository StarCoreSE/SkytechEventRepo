using AriUtils.HUD;
using Skytech.Engines.Shared.ModularAssemblies;
using Skytech.Engines.Shared.ModularAssemblies.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AriUtils;
using Collections;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Render.Particles;
using VRageMath;

namespace Skytech.Engines.Shared.Exhaust
{
    internal class FuelEngineExhaust : AssemblyBase
    {
        /// <summary>
        /// Helper for BlockInfo display only.
        /// </summary>
        public HashSet<IMyCubeBlock> PartsExhaustBlocked { get; private set; } = new HashSet<IMyCubeBlock>();

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<IMyCubeBlock, ExhaustVent[]> _externalVents = new Dictionary<IMyCubeBlock, ExhaustVent[]>();
        
        private Exhaust _exhaust = Exhaust.Zero;
        public int TotalOutlets => _externalVents.Count + _outlets.Count;

        /// <summary>
        /// Inlet PARTS; i.e. cylinders, turbos
        /// </summary>
        private CleanedSet<IExhaustProducer> _inlets = new CleanedSet<IExhaustProducer>(); // TODO inlet/outlet loops are bad!!!
        /// <summary>
        /// Outlet PARTS; i.e. turbos
        /// </summary>
        private CleanedSet<IExhaustConsumer> _outlets = new CleanedSet<IExhaustConsumer>();

        private readonly string[] _ventBlacklist = 
        {
            "ST_T_Cylinder",
            "ST_T_InlineTurboLeft",
            "ST_T_InlineTurboRight",
            "ST_T_TurbochargerLeft",
            "ST_T_TurbochargerRight",
        };

        public bool NeedsPressureUpdate = false;

        protected override void Init()
        {
            base.Init();
            Grid.OnBlockAdded += OnGridBlockAdd;
            Grid.OnBlockRemoved += OnGridBlockRemove;
        }

        public override void Unload()
        {
            base.Unload();
            Grid.OnBlockAdded -= OnGridBlockAdd;
            Grid.OnBlockRemoved -= OnGridBlockRemove;

            // Modular Assemblies doesn't call OnPartRemove on assembly close
            foreach (var ventSet in _externalVents)
            {
                foreach (var vent in ventSet.Value)
                    vent.Close();
            }
        }

        public override void OnPartAdd(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartAdd(block, isBasePart);

            PerformVentCheck(block);

            // delay cylinder check by a tick to give everything time to init
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                // Check for turbo and cylinder connections
                foreach (var pos in ModularApi.GetGridConnectingPositions(block, Definition.Name))
                {
                    IExhaustConsumer c;
                    if (TryGetConsumer(pos, out c) && c.IsInlet(block))
                    {
                        TryRegisterOutlet(c);
                    }

                    IExhaustProducer p;
                    if (TryGetProducer(pos, out p) && p.IsOutlet(block))
                    {
                        TryRegisterInlet(p);
                    }
                }
            });
        }

        public override void OnPartRemove(IMyCubeBlock block, bool isBasePart)
        {
            base.OnPartRemove(block, isBasePart);

            ExhaustVent[] vents;
            if (_externalVents.TryGetValue(block, out vents))
            {
                foreach (var vent in vents)
                {
                    vent.Close();
                }
            }

            // Check for turbo and cylinder connections
            foreach (var pos in ModularApi.GetGridConnectingPositions(block, Definition.Name))
            {
                IExhaustConsumer c;
                if (TryGetConsumer(pos, out c) && c.IsInlet(block))
                {
                    RemoveOutlet(c);
                }

                IExhaustProducer p;
                if (TryGetProducer(pos, out p) && p.IsOutlet(block))
                {
                    RemoveInlet(p);
                }
            }

            _externalVents.Remove(block);
            PartsExhaustBlocked.Remove(block);
        }

        public override void UpdateTick()
        {
            base.UpdateTick();

            NeedsPressureUpdate |= _inlets.RunCleanup();
            NeedsPressureUpdate |= _outlets.RunCleanup();
            
            if (NeedsPressureUpdate)
            {
                NeedsPressureUpdate = false;

                _exhaust = Exhaust.Zero;
                foreach (var inlet in _inlets)
                {
                    _exhaust += inlet.ExhaustProduced * inlet.OutletAssembly.Count(asm => asm == this);
                }

                Exhaust split = _exhaust / TotalOutlets;

                foreach (var outlet in _outlets)
                {
                    outlet.UpdateExhaust(split); // NOTE - this only works if ALL consumers have the same consumption. Which they do.
                }

                foreach (var pair in _externalVents)
                {
                    foreach (var vent in pair.Value)
                    {
                        vent.Update();
                    }
                }
            }
        }

        public bool TryRegisterInlet(IExhaustProducer inlet)
        {
            if (_inlets.Add(inlet))
            {
                NeedsPressureUpdate = true;
                inlet.OutletAssembly.Add(this);
                return true;
            }
            return false;
        }

        public void RemoveInlet(IExhaustProducer inlet)
        {
            if (_inlets.Remove(inlet))
            {
                NeedsPressureUpdate = true;
                inlet.OutletAssembly.Remove(this);
            }
        }

        public bool TryRegisterOutlet(IExhaustConsumer outlet)
        {
            if (_outlets.Add(outlet))
            {
                NeedsPressureUpdate = true;
                return true;
            }
            return false;
        }

        public void RemoveOutlet(IExhaustConsumer outlet)
        {
            if (_outlets.Remove(outlet))
            {
                NeedsPressureUpdate = true;
            }
        }

        private void OnGridBlockAdd(IMySlimBlock block)
        {
            // check for in/outlets
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                IMyCubeBlock fatBlock = block.FatBlock;
                if (fatBlock != null)
                {
                    IExhaustConsumer c;
                    if (TryGetConsumer(fatBlock, out c))
                    {
                        foreach (var part in Blocks) // actually horrible but it'll have to do. TODO replace with get block from position in interface, then check against this blocks hashset
                        {
                            if (c.IsInlet(part))
                            {
                                TryRegisterOutlet(c);
                                PartsExhaustBlocked.Remove(part);
                                break;
                            }
                        }
                    }

                    IExhaustProducer p;
                    if (TryGetProducer(fatBlock, out p))
                    {
                        foreach (var part in Blocks) // actually horrible but it'll have to do. TODO replace with get block from position in interface, then check against this blocks hashset
                        {
                            if (p.IsOutlet(part))
                            {
                                TryRegisterInlet(p);
                                PartsExhaustBlocked.Remove(part);
                                break;
                            }
                        }
                    }
                }
            });

            // check for occluded exhausts

            BoundingBoxI bounds = new BoundingBoxI(block.Min, block.Max);
            Dictionary<IMyCubeBlock, List<ExhaustVent>> updatedExhausts = new Dictionary<IMyCubeBlock, List<ExhaustVent>>();
            foreach (var exhaust in _externalVents)
            {
                List<ExhaustVent> validVents = new List<ExhaustVent>(); // TODO could optimize this func a looot
                foreach (var vent in exhaust.Value)
                {
                    //DebugDraw.AddGridPoint(exhaust.Key.Position, Grid, Color.Red, 10);
                    if (bounds.Intersects(new Ray(exhaust.Key.Position, vent.GridDirection)) == null)
                    {
                        validVents.Add(vent);
                        //DebugDraw.AddGridPoint(exhaust.Key.Position + vent.GridDirection, Grid, Color.Green, 10);
                    }
                    //else
                    //{
                    //    DebugDraw.AddGridPoint(exhaust.Key.Position + vent.GridDirection, Grid, Color.Red, 10);
                    //}
                }

                if (exhaust.Value.Length == validVents.Count)
                    continue;

                updatedExhausts[exhaust.Key] = validVents;
            }

            foreach (var exhaust in updatedExhausts)
            {
                if (exhaust.Value.Count == 0)
                {
                    foreach (var vent in _externalVents[exhaust.Key])
                        vent.Close();
                    _externalVents.Remove(exhaust.Key);
                }
                else
                {
                    foreach (var vent in _externalVents[exhaust.Key])
                        if (!exhaust.Value.Contains(vent))
                            vent.Close();
                    _externalVents[exhaust.Key] = exhaust.Value.ToArray();
                }
            }
        }

        private void OnGridBlockRemove(IMySlimBlock block)
        {
            // check for open exhausts
            // TODO issue with assembly splits not checking occlusion

            // check for in/outlets
            IMyCubeBlock fatBlock = block.FatBlock;
            if (fatBlock != null)
            {
                IExhaustConsumer c;
                if (TryGetConsumer(fatBlock, out c))
                {
                    RemoveOutlet(c);
                }

                IExhaustProducer p;
                if (TryGetProducer(fatBlock, out p))
                {
                    RemoveInlet(p);
                }
            }

            BoundingBoxI bounds = new BoundingBoxI(block.Min, block.Max);
            List<IMyCubeBlock> blocksNeedCheck = new List<IMyCubeBlock>();
            foreach (var exhaust in _externalVents)
            {
                bool needsCheck = false;

                foreach (var vent in exhaust.Value)
                {
                    //DebugDraw.AddGridPoint(exhaust.Key.Position, Grid, Color.Red, 10);
                    if (bounds.Intersects(new Ray(exhaust.Key.Position, vent.GridDirection)) == null)
                    {
                        needsCheck = true;
                        break;
                        //DebugDraw.AddGridPoint(exhaust.Key.Position + vent.GridDirection, Grid, Color.Green, 10);
                    }
                    //else
                    //{
                    //    DebugDraw.AddGridPoint(exhaust.Key.Position + vent.GridDirection, Grid, Color.Red, 10);
                    //}
                }

                if (needsCheck)
                {
                    blocksNeedCheck.Add(exhaust.Key);
                }
            }

            foreach (var exhaust in blocksNeedCheck)
            {
                PerformVentCheck(exhaust);
            }
        }

        protected override void BlockInfoCallback(IMyCubeBlock block, StringBuilder sb)
        {
            if (block.BlockDefinition.SubtypeName == "ST_T_Cylinder")
                return;

            base.BlockInfoCallback(block, sb);

            sb.AppendLine($"Exhaust Volume: {_exhaust.Amount:N1}");
            sb.AppendLine($"Exhaust Pressure: {_exhaust.Pressure:N1}");

            if (PartsExhaustBlocked.Count > 0)
            {
                sb.AppendLine($"{PartsExhaustBlocked.Count} exhaust{(PartsExhaustBlocked.Count == 1 ? "" : "s")} blocked!");
            }

            if (_externalVents.Count == 0 && _outlets.Count == 0)
            {
                sb.AppendLine("Zero exhaust vents found!");
            }
            
            sb.AppendLine($"Out: {_outlets.Count}+{_externalVents.Count} In: {_inlets.Count}");
        }

        private void PerformVentCheck(IMyCubeBlock block)
        {
            //// turbos don't vent to simplify direction logic
            //if (_ventBlacklist.Contains(block.BlockDefinition.SubtypeName))
            //    return;

            Dictionary<Vector3I, string[]> allowedConnections;
            if (!Definition.AllowedConnections.TryGetValue(block.BlockDefinition.SubtypeName, out allowedConnections))
                return;

            ExhaustVent[] existing = _externalVents.GetValueOrDefault(block, Array.Empty<ExhaustVent>());

            List<ExhaustVent> validDirs = new List<ExhaustVent>(6);
            foreach (var dir in allowedConnections.Keys)
            {
                // reuse existing exhausts yippee
                bool didUseExisting = false;
                foreach (var exVent in existing)
                {
                    if (exVent.LocalDirection == dir)
                    {
                        validDirs.Add(exVent);
                        didUseExisting = true;
                        break;
                    }
                }

                if (didUseExisting)
                    continue;

                Vector3I blockDir = (Vector3I) Vector3D.Rotate(dir, block.LocalMatrix);
                Vector3I chkPos = block.Position;
                IMySlimBlock intersectBlock;

                int i = 0;
                do
                {
                    chkPos += blockDir;
                    intersectBlock = Grid.GetCubeBlock(chkPos);
                    i++;
                } while (intersectBlock == null && Grid.PositionComp.LocalAABB.Contains(chkPos * Grid.GridSize) == ContainmentType.Contains);

                //DebugDraw.AddGridPoint(chkPos, Grid, Color.Pink, 10);

                if (intersectBlock != null)
                {
                    if (i == 1 && (Definition.AllowedConnections.ContainsKey(intersectBlock.BlockDefinition.Id.SubtypeName) || _ventBlacklist.Contains(intersectBlock.BlockDefinition.Id.SubtypeName)))
                        continue; // don't add to exhaust-blocked list if connected normally

                    PartsExhaustBlocked.Add(block);
                    continue;
                }

                validDirs.Add(new ExhaustVent(this, block, dir, blockDir));
            }

            if (validDirs.Count > 0)
            {
                _externalVents[block] = validDirs.ToArray();

                // delayed update so everything can init correctly
                foreach (var pair in _externalVents)
                {
                    foreach (var vent in pair.Value)
                    {
                        vent.Update();
                    }
                }
            }
        }

        private struct ExhaustVent : IEquatable<ExhaustVent>
        {
            public readonly FuelEngineExhaust Assembly;
            public readonly IMyCubeBlock Block;
            public readonly Vector3I LocalDirection;
            public readonly Vector3I GridDirection;
            public readonly MyParticleEffect Particle;

            public ExhaustVent(FuelEngineExhaust assembly, IMyCubeBlock block, Vector3I localDirection, Vector3I gridDirection)
            {
                Assembly = assembly;
                Block = block;
                LocalDirection = localDirection;
                GridDirection = gridDirection;

                Vector3D pos = (Vector3D)localDirection * block.CubeGrid.GridSize / 2;
                MatrixD matrix = MatrixD.CreateWorld(pos, Vector3.CalculatePerpendicularVector(LocalDirection), LocalDirection);
                // ExhaustSmokeSmall
                if (MyParticlesManager.TryCreateParticleEffect("EngineExhaustSmoke", ref matrix, ref Vector3D.Zero, block.Render.GetRenderObjectID(), out Particle))
                {
                    //MyAPIGateway.Utilities.ShowNotification("Spawned particle at " + hitEffect.WorldMatrix.Translation);
                }
                else
                {
                    Log.Exception("FuelEngineExhaust", new Exception("Could not create exhaust particle."));
                    //throw new Exception($"Failed to create new impact particle! RenderId: {uint.MaxValue} Effect: {Definition.VisualDef.ImpactParticle}");
                }
            }

            public void Update()
            {
                // velocity would be cool but it's being mean 3:
                float exhaustAmount = Assembly._exhaust.Amount / Assembly.TotalOutlets;
                float exhaustPressure = Assembly._exhaust.Pressure / Assembly.TotalOutlets;

                //MyAPIGateway.Utilities.ShowNotification($"{exhaustAmount:N} {exhaustPressure:N} {(Particle.IsEmittingStopped || Particle.IsStopped)}", 10000);

                if ((Assembly.TotalOutlets == 0 || exhaustAmount <= 0.01) && !Particle.IsEmittingStopped)
                {
                    Particle.StopEmitting();
                    return;
                }
                if (exhaustAmount > 0.01 && (Particle.IsEmittingStopped || Particle.IsStopped))
                    Particle.Play();

                Particle.UserBirthMultiplier = (float) Math.Sqrt(exhaustAmount); // TODO tune exhaust strength; effects way too strong for larger engines.
                //Particle.UserLifeMultiplier = exhaustPressure;
                //Particle.UserColorIntensityMultiplier = 1/exhaustAmount;
                Particle.UserVelocityMultiplier = (float) Math.Sqrt(exhaustPressure);
            }

            public void Close()
            {
                MyParticlesManager.RemoveParticleEffect(Particle);
            }

            public bool Equals(ExhaustVent other)
            {
                return Equals(Block.EntityId, other.Block.EntityId) && LocalDirection.Equals(other.LocalDirection);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ExhaustVent && Equals((ExhaustVent)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Block != null ? Block.GetHashCode() : 0) * 397 ^ LocalDirection.GetHashCode();
                }
            }
        }

        public bool TryGetProducer(Vector3I blockPos, out IExhaustProducer producer)
        {
            var block = Grid.GetCubeBlock(blockPos)?.FatBlock;
            if (block != null)
            {
                Turbo t;
                if (TurboManager.I.TryGetTurbo(block, out t))
                {
                    producer = t;
                    return true;
                }

                FuelEngineCylinder c;
                if (AssemblyManager<FuelEngineCylinder>.TryGet(block, out c))
                {
                    producer = c;
                    return true;
                }
            }

            producer = null;
            return false;
        }

        public bool TryGetProducer(IMyCubeBlock block, out IExhaustProducer producer)
        {
            Turbo t;
            if (TurboManager.I.TryGetTurbo(block, out t))
            {
                producer = t;
                return true;
            }

            FuelEngineCylinder c;
            if (AssemblyManager<FuelEngineCylinder>.TryGet(block, out c))
            {
                producer = c;
                return true;
            }

            producer = null;
            return false;
        }

        public bool TryGetConsumer(Vector3I blockPos, out IExhaustConsumer consumer)
        {
            var block = Grid.GetCubeBlock(blockPos)?.FatBlock;
            if (block != null)
            {
                Turbo t;
                if (TurboManager.I.TryGetTurbo(block, out t))
                {
                    consumer = t;
                    return true;
                }
            }
            
            consumer = null;
            return false;
        }

        public bool TryGetConsumer(IMyCubeBlock block, out IExhaustConsumer consumer)
        {
            Turbo t;
            if (TurboManager.I.TryGetTurbo(block, out t))
            {
                consumer = t;
                return true;
            }
            
            consumer = null;
            return false;
        }

        public struct Exhaust : IEquatable<Exhaust>
        {
            public static readonly Exhaust Zero = new Exhaust(0, 0);

            public float Pressure;
            public float Amount;

            public Exhaust(float amountAndPressure)
            {
                Pressure = amountAndPressure;
                Amount = amountAndPressure;
            }

            public Exhaust(float pressure, float amount)
            {
                Pressure = pressure;
                Amount = amount;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Exhaust))
                    return false;

                Exhaust other = (Exhaust)obj;

                return Math.Abs(Amount - other.Amount) < 0.0001f && Math.Abs(Pressure - other.Pressure) < 0.0001f;
            }

            public bool Equals(Exhaust other)
            {
                return Math.Abs(Amount - other.Amount) < 0.0001f && Math.Abs(Pressure - other.Pressure) < 0.0001f;
            }

            public override int GetHashCode()
            {
                return Amount.GetHashCode() ^ Pressure.GetHashCode();
            }

            public static Exhaust operator +(Exhaust a, Exhaust b)
            {
                return new Exhaust(a.Pressure + b.Pressure, a.Amount + b.Amount);
            }

            public static Exhaust operator -(Exhaust a, Exhaust b)
            {
                return new Exhaust(a.Pressure - b.Pressure, a.Amount - b.Amount);
            }

            public static Exhaust operator +(Exhaust a, float b)
            {
                return new Exhaust(a.Pressure + b, a.Amount + b);
            }

            public static Exhaust operator -(Exhaust a, float b)
            {
                return new Exhaust(a.Pressure - b, a.Amount - b);
            }

            public static Exhaust operator *(Exhaust a, float b)
            {
                return new Exhaust(a.Pressure * b, a.Amount * b);
            }

            public static Exhaust operator /(Exhaust a, float b)
            {
                return new Exhaust(a.Pressure / b, a.Amount / b);
            }

            public static bool operator ==(Exhaust a, Exhaust b)
            {
                return Math.Abs(a.Amount - b.Amount) < 0.0001f && Math.Abs(a.Pressure - b.Pressure) < 0.0001f;
            }

            public static bool operator !=(Exhaust a, Exhaust b)
            {
                return Math.Abs(a.Amount - b.Amount) >= 0.0001f || Math.Abs(a.Pressure - b.Pressure) >= 0.0001f;
            }
        }
    }
}
