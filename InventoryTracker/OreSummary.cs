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
        public class OreSummary
        {
            private readonly List<IMyCargoContainer> cargoContainers;

            public OreSummary(List<IMyCargoContainer> cargoContainers)
            {
                this.cargoContainers = cargoContainers;
            }

            public Dictionary<Ore.OreType, float> OreAmounts()
            {
                Dictionary<Ore.OreType, float> amounts = new Dictionary<Ore.OreType, float>();
                foreach(IMyCargoContainer container in cargoContainers)
                {
                    CollectOres(amounts, container);
                }
                return amounts;
            }

            private void CollectOres(Dictionary<Ore.OreType, float> collection, IMyCargoContainer container)
            {
                IMyInventory inventory = container.GetInventory(0);
                foreach (Ore.OreType type in Ore.Types())
                {
                    MyFixedPoint point = inventory.GetItemAmount(Ore.ItemType(type));
                    float itemInKg = (float)point.RawValue / 1000000000f;
                    collection[type] = collection.GetValueOrDefault(type, 0) + itemInKg;
                }
            }
        }
    }
}
