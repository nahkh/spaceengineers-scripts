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
        public class TradeableItem
        {
            private readonly ItemType type;
            private readonly string subtypeId;

            public enum ItemType
            {
                Component,
                Ingot,
                Ore,
            }

            public TradeableItem(ItemType type, string subtypeId)
            {
                this.type = type;
                this.subtypeId = subtypeId;
            }

            public static TradeableItem FromOre(Ore.OreType oreType)
            {
                return new TradeableItem(ItemType.Ore, oreType.ToString());
            }

            public static TradeableItem FromIngot(Ingot.IngotType ingotType)
            {
                return new TradeableItem(ItemType.Ingot, ingotType.ToString());
            }
            public static TradeableItem FromComponent(Component.ComponentType componentType)
            {
                return new TradeableItem(ItemType.Component, componentType.ToString());
            }

            public MyItemType ToItemType()
            {
                switch(type)
                {
                    case ItemType.Component:
                        return MyItemType.MakeComponent(subtypeId);
                    case ItemType.Ingot:
                        return MyItemType.MakeIngot(subtypeId);
                    case ItemType.Ore:
                        return MyItemType.MakeOre(subtypeId);
                    default:
                        throw new Exception("Invalid type: " + type);
                }
            }

            public override string ToString()
            {
                return type.ToString() + ":" + subtypeId;
            }

            public bool Equals(TradeableItem tradeableItem)
            {
                return type == tradeableItem.type && subtypeId == tradeableItem.subtypeId;
            }

            public static TradeableItem Parse(string input)
            {
                string[] elements = input.Split(':');
                if (elements.Length != 2)
                {
                    throw new ArgumentException("Cannot parse TradeableItem " + input);
                }
                ItemType type = ParseItemType(elements[0]);
                return new TradeableItem(type, elements[1]);
            }

            public static ItemType ParseItemType(string input)
            {
                switch (input)
                {
                    case "Ore":
                        return ItemType.Ore;
                    case "Ingot":
                        return ItemType.Ingot;
                    case "Component":
                        return ItemType.Component;
                    default:
                        throw new ArgumentException("Cannot parse TradeableItem.ItemType " + input);
                }
            }

            public static List<TradeableItem> AllTradeableItems()
            {
                List<TradeableItem> items = new List<TradeableItem>();
                foreach(Ore.OreType type in Ore.Types())
                {
                    items.Add(FromOre(type));
                }
                foreach (Ingot.IngotType type in Ingot.Types())
                {
                    items.Add(FromIngot(type));
                }
                foreach (Component.ComponentType type in Component.Types())
                {
                    items.Add(FromComponent(type));
                }
                return items;
            }
        }
    }
}
