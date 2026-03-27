using AriUtils.HUD;
using Skytech.Engines.Shared.ModularAssemblies;
using Skytech.Engines.Shared.ModularAssemblies.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AriUtils;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Render.Particles;
using VRageMath;

namespace Skytech.Engines
{
    internal class FuelEngineExhaust : AssemblyBase
    {
        /// <summary>
        /// Helper for BlockInfo display only.
        /// </summary>
        public HashSet<IMyCubeBlock> PartsExhaustBlocked { get; private set; } = new HashSet<IMyCubeBlock>();

        private DefinitionDefs.ModularPhysicalDefinition _definition = new ModularDefinition().FuelEngineExhaust;
        /// <summary>
        /// block, open directions
        /// </summary>
        private Dictionary<IMyCubeBlock, ExhaustVent[]> _externalVents = new Dictionary<IMyCubeBlock, ExhaustVent[]>();
        /// <summary>
        /// block, (internal amt, pressure)
        /// </summary>
        private Dictionary<IMyCubeBlock, Vector2> _exhaustPressure = new Dictionary<IMyCubeBlock, Vector2>();
        /// <summary>
        /// block, (internal amt, pressure)
        /// </summary>
        private Dictionary<IMyCubeBlock, float> _inlets = new Dictionary<IMyCubeBlock, float>();

        private readonly string[] _ventBlacklist = 
        {
            "ST_T_Cylinder",
            "ST_T_InlineTurboLeft",
            "ST_T_InlineTurboRight",
            "ST_T_TurbochargerLeft",
            "ST_T_TurbochargerRight",
        };

        private bool _needsPressureUpdate = false;
        private Dictionary<IMyCubeBlock, FuelEngineCylinder> _cylinders = new Dictionary<IMyCubeBlock, FuelEngineCylinder>();

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

            _exhaustPressure[block] = Vector2I.Zero;
            PerformVentCheck(block);
            _needsPressureUpdate = true;

            // delay cylinder check by a tick to give everything time to init
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                if (block.BlockDefinition.SubtypeName == "ST_T_Cylinder")
                {
                
                    int cylId = ModularApi.GetContainingAssembly(block, "FuelEngineCylinder");
                    if (cylId != -1)
                    {
                        FuelEngineCylinder cyl = AssemblyManager<FuelEngineCylinder>.Get(cylId);
                        if (cyl == null)
                            throw new Exception("Missing cylinder assembly!");
                        _cylinders[block] = cyl;

                        foreach (var conn in ModularApi.GetConnectedBlocks(block, _definition.Name))
                        {
                            if (conn.BlockDefinition.SubtypeName == "ST_T_Cylinder")
                                continue;

                            cyl.Exhausts[conn] = this;
                        }
                    }
                }
                else
                {
                    foreach (var conn in ModularApi.GetConnectedBlocks(block, _definition.Name))
                    {
                        FuelEngineCylinder cyl;
                        if (!_cylinders.TryGetValue(conn, out cyl))
                            continue;
                        cyl.Exhausts[block] = this;
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

            FuelEngineCylinder cyl;
            if (_cylinders.TryGetValue(block, out cyl))
            {
                List<IMyCubeBlock> thisExhausts = new List<IMyCubeBlock>();
                foreach (var ex in cyl.Exhausts)
                    if (ex.Value == this)
                        thisExhausts.Add(ex.Key);
                foreach (var ex in thisExhausts)
                    cyl.Exhausts.Remove(ex);
            }
            else
            {
                foreach (var conn in ModularApi.GetConnectedBlocks(block, _definition.Name))
                {
                    if (!_cylinders.TryGetValue(conn, out cyl))
                        continue;
                    cyl.Exhausts.Remove(block);
                }
            }

            _externalVents.Remove(block);
            PartsExhaustBlocked.Remove(block);
            _exhaustPressure.Remove(block);
            _needsPressureUpdate = true;
        }

        public override void UpdateTick()
        {
            base.UpdateTick();

            //foreach (var block in _externalVents)
            //{
            //    foreach (var dir in block.Value)
            //    {
            //        DebugDraw.AddLine(block.Key.GetPosition(), Grid.GridIntegerToWorld(block.Key.Position + dir.GridDirection), Color.Red, 1/60f);
            //    }
            //}

            if (_needsPressureUpdate)
            {
                // set all pressure to zero
                foreach (var block in Blocks)
                    _exhaustPressure[block] = Vector2I.Zero;

                if (_cylinders.Count > 0)
                {
                    foreach (var inlet in _inlets)
                        _exhaustPressure[inlet.Key] = new Vector2(inlet.Value);
                    
                    Queue<IMyCubeBlock> scanSet = new Queue<IMyCubeBlock>();
                    HashSet<IMyCubeBlock> visited = new HashSet<IMyCubeBlock>();
                    
                    foreach (var inlet in _inlets)
                    {
                        scanSet.Enqueue(inlet.Key);

                        while (scanSet.Count > 0)
                        {
                            IMyCubeBlock block = scanSet.Dequeue();
                            if (block.BlockDefinition.SubtypeName == "ST_T_Cylinder" || _externalVents.ContainsKey(block))
                                continue;

                            var connected = ModularApi.GetConnectedBlocks(block, _definition.Name);

                            // evenly split amount and pressure
                            int conCount = 0;
                            foreach (var con in connected)
                            {
                                if (con.BlockDefinition.SubtypeName == "ST_T_Cylinder")
                                    continue;
                                conCount++;
                            }
                    
                            foreach (var con in connected)
                            {
                                if (!visited.Add(con) || _cylinders.ContainsKey(con))
                                    continue;
                    
                                _exhaustPressure[con] += inlet.Value / conCount;
                                scanSet.Enqueue(con);
                            }
                        } // TODO update turbos and turbo directionality.
                    
                        visited.Clear();
                    }
                }
                
                _needsPressureUpdate = false;
            }
        }

        /// <summary>
        /// exhaust amount, pressure
        /// </summary>
        /// <param name="block"></param>
        /// <param name="data"></param>
        public void UpdateInlet(IMyCubeBlock block, float exhaust)
        {
            float prev;
            if (_inlets.TryGetValue(block, out prev) && prev == exhaust)
                return;
            
            if (exhaust == 0)
                _inlets.Remove(block);
            else
                _inlets[block] = exhaust;
            _needsPressureUpdate = true;
        }

        private void OnGridBlockAdd(IMySlimBlock block)
        {
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

            sb.AppendLine($"Exhaust Volume: {_exhaustPressure[block].X:N1}");
            sb.AppendLine($"Exhaust Pressure: {_exhaustPressure[block].Y:N1}");

            if (PartsExhaustBlocked.Count > 0)
            {
                sb.AppendLine($"{PartsExhaustBlocked.Count} exhaust{(PartsExhaustBlocked.Count == 1 ? "" : "s")} blocked!");
            }

            if (_externalVents.Count == 0)
            {
                sb.AppendLine("Zero exhaust vents found!");
            }
            // TODO pressure levels
        }

        private void PerformVentCheck(IMyCubeBlock block)
        {
            // turbos don't vent to simplify direction logic
            if (_ventBlacklist.Contains(block.BlockDefinition.SubtypeName))
                return;

            Dictionary<Vector3I, string[]> allowedConnections;
            if (!_definition.AllowedConnections.TryGetValue(block.BlockDefinition.SubtypeName, out allowedConnections))
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
                    if (i == 1 && _definition.AllowedConnections.ContainsKey(intersectBlock.BlockDefinition.Id.SubtypeName))
                        continue; // don't add to exhaust-blocked list if connected normally

                    PartsExhaustBlocked.Add(block);
                    continue;
                }

                validDirs.Add(new ExhaustVent(this, block, dir, blockDir));
            }

            if (validDirs.Count > 0)
            {
                _externalVents[block] = validDirs.ToArray();
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
                MatrixD matrix = MatrixD.CreateWorld(pos, LocalDirection, Vector3D.Up);
                // ExhaustSmokeSmall
                if (MyParticlesManager.TryCreateParticleEffect("ExhaustSmokeSmall", ref matrix, ref Vector3D.Zero, block.Render.GetRenderObjectID(), out Particle))
                {
                    //MyAPIGateway.Utilities.ShowNotification("Spawned particle at " + hitEffect.WorldMatrix.Translation);
                }
                else
                {
                    Log.Exception("FuelEngineExhaust", new Exception("Could not create exhaust particle."));
                    //throw new Exception($"Failed to create new impact particle! RenderId: {uint.MaxValue} Effect: {Definition.VisualDef.ImpactParticle}");
                }

                Update();
            }

            public void Update()
            {
                // velocity would be cool but it's being mean 3:
                Vector2 exhaustStrength = Assembly._exhaustPressure[Block];

                if (exhaustStrength.X == 0 && !Particle.IsEmittingStopped)
                    Particle.StopEmitting();
                if (exhaustStrength.X > 0 && Particle.IsEmittingStopped)
                    Particle.Play();

                Particle.UserBirthMultiplier = exhaustStrength.X;
                Particle.UserLifeMultiplier = exhaustStrength.X;
                Particle.UserVelocityMultiplier = exhaustStrength.Y; // TODO new particle with working velocity
            }

            public void Close()
            {
                MyParticlesManager.RemoveParticleEffect(Particle);
            }

            public bool Equals(ExhaustVent other)
            {
                return Equals(Block, other.Block) && LocalDirection.Equals(other.LocalDirection);
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
                    return ((Block != null ? Block.GetHashCode() : 0) * 397) ^ LocalDirection.GetHashCode();
                }
            }
        }
    }
}
