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
        public class Ingot
        {
            public enum IngotType
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
            }

            public static MyItemType ItemType(IngotType type)
            {
                return MyItemType.MakeIngot(type.ToString());
            }

            public static string ShortName(IngotType type)
            {
                switch (type)
                {
                    case IngotType.Stone:
                        return "St";
                    case IngotType.Iron:
                        return "Fe";
                    case IngotType.Silicon:
                        return "Si";
                    case IngotType.Nickel:
                        return "Ni";
                    case IngotType.Cobalt:
                        return "Co";
                    case IngotType.Silver:
                        return "Ag";
                    case IngotType.Gold:
                        return "Au";
                    case IngotType.Magnesium:
                        return "Mg";
                    case IngotType.Uranium:
                        return "U ";
                    case IngotType.Platinum:
                        return "Pl";
                    default:
                        return "XX";
                }
            }

            public static IEnumerable<IngotType> Types()
            {
                return Enum.GetValues(typeof(IngotType)).Cast<IngotType>();
            }

            public static List<MyItemType> ItemTypeList()
            {
                List<MyItemType> types = new List<MyItemType>();
                foreach (IngotType type in Types())
                {
                    types.Add(ItemType(type));
                }
                return types;
            }

            public static readonly Dictionary<string, IngotType> NameToType;

            static Ingot() {
                NameToType = new Dictionary<string, IngotType>();
                foreach (IngotType type in Types())
                {
                    NameToType.Add(type.ToString(), type);
                }
            }
        }
    }
}
