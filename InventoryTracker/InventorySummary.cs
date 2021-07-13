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

            public Dictionary<T, N> Amounts<N>(Func<MyFixedPoint, N> value, Func<N, N, N> combiner, Func<N> defaultValue)
            {
                Dictionary<T, N> amounts = new Dictionary<T, N>();
                foreach (IMyEntity entity in entities)
                {
                    for (int i = 0; i < entity.InventoryCount; ++i)
                    {
                        Collect(amounts, value, combiner, defaultValue, entity.GetInventory(i));
                    }

                }
                return amounts;
            }

            public Dictionary<T, float> FloatAmounts()
            {
                Dictionary<T, float> amounts = new Dictionary<T, float>();
                foreach(IMyEntity entity in entities)
                {
                    for(int i = 0; i < entity.InventoryCount; ++i)
                    {
                        Collect(amounts, ItemValue, (a, b) => a + b, () => 0, entity.GetInventory(i));
                    }
                    
                }
                return amounts;
            }

            public Dictionary<T, int> IntAmounts()
            {
                Dictionary<T, int> amounts = new Dictionary<T, int>();
                foreach (IMyEntity entity in entities)
                {
                    for (int i = 0; i < entity.InventoryCount; ++i)
                    {
                        Collect(amounts, point => point.ToIntSafe(), (a, b) => a + b, () => 0, entity.GetInventory(i));
                    }

                }
                return amounts;
            }

            protected abstract MyItemType ItemType(T t);
            protected abstract IEnumerable<T> Types();


            private void Collect<N>(Dictionary<T, N> collection, Func<MyFixedPoint, N> value, Func<N, N, N> combiner, Func<N> defaultValue, IMyInventory inventory)
            {
                foreach (T type in Types())
                {
                    MyFixedPoint point = inventory.GetItemAmount(ItemType(type));
                    N itemValue = value.Invoke(point);
                    collection[type] = combiner.Invoke(collection.GetValueOrDefault(type, defaultValue.Invoke()), itemValue);
                }
            }

            protected virtual float ItemValue(MyFixedPoint point)
            {
                return ((float)point);
            }
        }
    }
}
