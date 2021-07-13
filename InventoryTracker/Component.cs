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
        public class Component
        {
            public enum ComponentType
            {
                BulletproofGlass,
                Computer,
                Construction,
                Detector,
                Display,
                Explosives,
                Girder,
                GravityGenerator,
                InteriorPlate,
                LargeTube,
                Medical,
                MetalGrid,
                Motor,
                PowerCell,
                RadioCommunication,
                Reactor,
                SmallTube,
                SolarCell,
                SteelPlate,
                Superconductor,
                Thrust,
                ZoneChip,
            }

            public static MyItemType ItemType(ComponentType partType)
            {
                return MyItemType.MakeComponent(partType.ToString());
            }
            public static IEnumerable<ComponentType> Types()
            {
                return Enum.GetValues(typeof(ComponentType)).Cast<ComponentType>();
            }

            public static List<MyItemType> ItemTypeList()
            {
                List<MyItemType> types = new List<MyItemType>();
                foreach (ComponentType type in Types())
                {
                    types.Add(ItemType(type));
                }
                return types;
            }

            public static readonly Dictionary<string, ComponentType> NameToType;

            static Component()
            {
                NameToType = new Dictionary<string, ComponentType>();
                foreach (ComponentType type in Types())
                {
                    NameToType.Add(type.ToString(), type);
                }
            }
        }
    }
}
