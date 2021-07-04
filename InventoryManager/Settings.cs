using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class Settings
        {
            MyIni _ini = new MyIni();

            public Settings(IMyProgrammableBlock programmableBlock)
            {
                MyIniParseResult result;
                if (!_ini.TryParse(programmableBlock.CustomData, out result))
                    throw new Exception(result.ToString());
            }

            public string IngotStorageTag
            {
                get
                {
                    return _ini.Get("InventoryManager", "IngotStorage").ToString("InventoryManager-Ingots");
                }
            }
            public string IngotDisplayTag
            {
                get
                {
                    return _ini.Get("InventoryManager", "IngotDisplay").ToString("InventoryManager-Display-Ingots");
                }
            }
            public string ComponentDisplayTag
            {
                get
                {
                    return _ini.Get("InventoryManager", "ComponentDisplay").ToString("InventoryManager-Display-Components");
                }
            }

            public string ComponentStorageTag
            {
                get
                {
                    return _ini.Get("InventoryManager", "ComponentStorage").ToString("InventoryManager-Components");
                }
            }

            public string ToolsStorageTag
            {
                get
                {
                    return _ini.Get("InventoryManager", "ToolStorage").ToString("InventoryManager-Tools");
                }
            }
            public string AmmoStorageTag
            {
                get
                {
                    return _ini.Get("InventoryManager", "AmmoStorage").ToString("InventoryManager-Ammo");
                }
            }

            public string IgnoreTag
            {
                get
                {
                    return _ini.Get("InventoryManager", "Ignore").ToString("InventoryManager-Ignore");
                }
            }

            public bool MoveItems
            {
                get
                {
                    return _ini.Get("InventoryManager", "MoveItems").ToBoolean(true);
                }
            }
        }
    }
}
