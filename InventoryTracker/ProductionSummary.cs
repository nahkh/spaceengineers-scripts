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
        public class ProductionSummary
        {
            private readonly List<IMyAssembler> assemblers;
            private readonly Action<string> logger;
            private readonly List<MyProductionItem> productionItems;
            private readonly Dictionary<Component.ComponentType, int> summary;

            public ProductionSummary(List<IMyAssembler> assemblers, Action<string> logger) {
                this.assemblers = assemblers;
                this.logger = logger;
                summary = new Dictionary<Component.ComponentType, int>();
                productionItems = new List<MyProductionItem>();
            }

            public Dictionary<Component.ComponentType, int> ProductionQueueSummary()
            {
                productionItems.Clear();
                summary.Clear();
                foreach (IMyAssembler assembler in assemblers)
                {
                    productionItems.Clear();
                    assembler.GetQueue(productionItems);
                    foreach (MyProductionItem item in productionItems)
                    {
                        int amount = item.Amount.ToIntSafe();
                        MyDefinitionId blueprintId = item.BlueprintId;
                        Component.ComponentType type;
                        if(Component.TryFromDefinitionId(blueprintId, out type))
                        {
                            summary[type] = summary.GetValueOrDefault(type, 0) + amount;
                        }
                    }
                }
                return summary;
            }

        }
    }
}
