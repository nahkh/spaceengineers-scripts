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
        
        public static class Ore
        {
            public enum OreType
            {
                Stone,
                Iron,
                Silicon,
                Nickel,
                Cobalt,
                Silver,
                Gold,
                Magnesium,
                Uranium,
                Platinum,
                Ice,
            }

            public static MyItemType ItemType(OreType type)
            {
                return MyItemType.MakeOre(type.ToString());
            }

            public static string ShortName(OreType type)
            {
                switch(type)
                {
                    case OreType.Stone:
                        return "St";
                    case OreType.Iron:
                        return "Fe";
                    case OreType.Silicon:
                        return "Si";
                    case OreType.Nickel:
                        return "Ni";
                    case OreType.Cobalt:
                        return "Co";
                    case OreType.Silver:
                        return "Ag";
                    case OreType.Gold:
                        return "Au";
                    case OreType.Magnesium:
                        return "Mg";
                    case OreType.Uranium:
                        return "U ";
                    case OreType.Platinum:
                        return "Pl";
                    case OreType.Ice:
                        return "Ic";
                    default:
                        return "XX";
                }
            }

            public static IEnumerable<OreType> Types()
            {
                return Enum.GetValues(typeof(OreType)).Cast<OreType>();
            }

            public static List<MyItemType> ItemTypeList()
            {
                List<MyItemType> types = new List<MyItemType>();
                foreach (OreType type in Types())
                {
                    types.Add(ItemType(type));
                }
                return types;
            }

            public static readonly Dictionary<string, OreType> NameToType;

            static Ore()
            {
                NameToType = new Dictionary<string, OreType>();
                foreach (OreType type in Types())
                {
                    NameToType.Add(type.ToString(), type);
                }
            }
        }
    }
}
