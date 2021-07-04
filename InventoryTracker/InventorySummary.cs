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
        public abstract class InventorySummary<T>
        {
            private readonly List<IMyTerminalBlock> entities;

            public InventorySummary(List<IMyTerminalBlock> entities)
            {
                this.entities = entities;
            }

            public Dictionary<T, float> Amounts()
            {
                Dictionary<T, float> amounts = new Dictionary<T, float>();
                foreach(IMyEntity entity in entities)
                {
                    for(int i = 0; i < entity.InventoryCount; ++i)
                    {
                        CollectOres(amounts, entity.GetInventory(i));
                    }
                    
                }
                return amounts;
            }

            protected abstract MyItemType ItemType(T t);
            protected abstract IEnumerable<T> Types();


            private void CollectOres(Dictionary<T, float> collection, IMyInventory inventory)
            {
                foreach (T type in Types())
                {
                    MyFixedPoint point = inventory.GetItemAmount(ItemType(type));
                    float itemCount = ItemValue(point);
                    collection[type] = collection.GetValueOrDefault(type, 0) + itemCount;
                }
            }

            protected virtual float ItemValue(MyFixedPoint point)
            {
                return point.RawValue / 1000000f;
            }
        }
    }
}
