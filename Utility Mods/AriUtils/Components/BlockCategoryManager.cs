using System.Collections.Generic;
using AriUtils;
using Sandbox.Definitions;
using VRage.Game;

namespace Skytech.Engines.Client.Interface
{
    public class BlockCategoryManager : SingletonBase<BlockCategoryManager>
    {
        private GuiBlockCategoryHelper _blockCategory;
        private HashSet<string> _bufferBlockSubtypes = new HashSet<string>
        {
            // Everything relevant should be automatically added, but if not, put its subtype here.
        }; // DefinitionManager can load before the BlockCategoryManager on client and cause an exception.

        private Dictionary<string, string> _subtypeToTypePairing;

        public override void Init()
        {
            _subtypeToTypePairing = new Dictionary<string, string>();
            foreach (var def in MyDefinitionManager.Static.GetAllDefinitions())
            {
                if (string.IsNullOrEmpty(def.Id.SubtypeName)) continue;
                _subtypeToTypePairing[def.Id.SubtypeName] = def.Id.TypeId.ToString().Replace("MyObjectBuilder_", "");
                if (def.Context?.ModPath == GlobalData.ModContext.ModPath && (def is MyCubeBlockDefinition || def is MyPhysicalItemDefinition)) // Adds all blocks from this mod automatically
                    RegisterFromSubtype(def.Id.SubtypeName);
            }

            _blockCategory = new GuiBlockCategoryHelper("[SkyTech Engines]", "SkytechEnginesBlockCategory");
            foreach (var item in _bufferBlockSubtypes)
                _blockCategory.AddBlock(item);

            Log.Info("BlockCategoryManager", "Initialized.");
        }

        public static void RegisterFromSubtype(string subtypeId)
        {
            if (I._bufferBlockSubtypes.Add(subtypeId) && I._blockCategory != null)
                I._blockCategory.AddBlock(subtypeId);
        }

        public override void Update()
        {
            
        }

        public override void Unload()
        {
            _subtypeToTypePairing = null;
            _blockCategory = null;
            _bufferBlockSubtypes = null;
            Log.Info("BlockCategoryManager", "Unloaded.");
        }

        private class GuiBlockCategoryHelper
        {
            private readonly MyGuiBlockCategoryDefinition _category;

            public GuiBlockCategoryHelper(string name, string id)
            {
                _category = new MyGuiBlockCategoryDefinition
                {
                    Id = new MyDefinitionId(typeof(MyObjectBuilder_GuiBlockCategoryDefinition), id),
                    Name = name,
                    DisplayNameString = name,
                    ItemIds = new HashSet<string>(),
                    IsBlockCategory = true,
                };
                MyDefinitionManager.Static.GetCategories().Add(name, _category);
            }

            public void AddBlock(string subtypeId)
            {
                Log.IncreaseIndent();
                string typeId;
                if (I._subtypeToTypePairing.TryGetValue(subtypeId, out typeId)) // keen broke block category items with just subtypeid
                {
                    _category.ItemIds.Add(typeId + "/" + subtypeId);
                    Log.Info("GuiBlockCategoryHelper", $"Added {typeId + "/" + subtypeId}");
                }
                else
                {
                    _category.ItemIds.Add(subtypeId + "/(null)");
                    Log.Info("GuiBlockCategoryHelper", $"Added {subtypeId + "/(null)"}");
                }
                Log.DecreaseIndent();

                //foreach (var _cat in MyDefinitionManager.Static.GetCategories().Values)
                //{
                //    HeartData.I.Log.Log("Category " + _cat.Name);
                //    foreach (var _id in _cat.ItemIds)
                //        HeartData.I.Log.Log($"   \"{_id}\"");
                //}
            }
        }
    }
}
