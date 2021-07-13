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
        public class AnalyzedPartDisplay
        {
            private readonly IMyTextSurface textSurface;
            private readonly Func<Dictionary<Component.ComponentType, int>> neededPartsProvider;
            private readonly Func<Dictionary<Component.ComponentType, int>> currentInventoryProvider;
            private readonly Func<Dictionary<Component.ComponentType, int>> productionCount;

            enum ColumnType
            {
                Total,
                Available,
                Missing,
                Building,
            }

            public AnalyzedPartDisplay(IMyTextSurface textSurface, Func<Dictionary<Component.ComponentType, int>> neededPartsProvider, Func<Dictionary<Component.ComponentType, int>> currentInventoryProvider, Func<Dictionary<Component.ComponentType, int>> productionCount)
            {
                this.textSurface = textSurface;
                this.neededPartsProvider = neededPartsProvider;
                this.currentInventoryProvider = currentInventoryProvider;
                this.productionCount = productionCount;
            }

            public Display Build()
            {
                TableDisplay<Component.ComponentType, ColumnType> table = new TableDisplay<Component.ComponentType, ColumnType>(textSurface, 72, 30);

                foreach(Component.ComponentType type in Component.Types())
                {
                    table.Row(type, NeedAnyOf(type));
                }
                return table.WithLabel("Parts needed for project")
                    .Column(ColumnType.Total, alignment:TableDisplay<Component.ComponentType, ColumnType>.Alignment.RIGHT)
                    .Column(ColumnType.Available, alignment: TableDisplay<Component.ComponentType, ColumnType>.Alignment.RIGHT, columnWidth:10)
                    .Column(ColumnType.Missing, alignment: TableDisplay<Component.ComponentType, ColumnType>.Alignment.RIGHT, columnWidth: 10)
                    .Column(ColumnType.Building, alignment: TableDisplay<Component.ComponentType, ColumnType>.Alignment.RIGHT, columnWidth: 10)
                    .WithData(Render)
                    .Build();
            }

            private string Render(Component.ComponentType component, ColumnType column)
            {
                switch(column)
                {
                    case ColumnType.Total:
                        return NeededCount(component).ToString();
                    case ColumnType.Available:
                        return AvailableCount(component).ToString();
                    case ColumnType.Missing:
                        int missingCount = MissingCount(component);
                        if (missingCount == 0)
                        {
                            return "";
                        }
                        return missingCount.ToString();
                    case ColumnType.Building:
                        int productionCount = ProductionCount(component);
                        if (productionCount == 0)
                        {
                            return "";
                        }
                        return productionCount.ToString();
                }
                return "";
            }

            private int NeededCount(Component.ComponentType component)
            {
                return neededPartsProvider.Invoke().GetValueOrDefault(component, 0);
            }
            private int AvailableCount(Component.ComponentType component)
            {
                return currentInventoryProvider.Invoke().GetValueOrDefault(component, 0);
            }

            private int MissingCount(Component.ComponentType component)
            {
                return Math.Max(0, NeededCount(component) - AvailableCount(component));
            }

            private int ProductionCount(Component.ComponentType component)
            {
                return productionCount.Invoke().GetValueOrDefault(component, 0); ;
            }

            private Func<bool> NeedAnyOf(Component.ComponentType type)
            {
                return () => neededPartsProvider.Invoke().GetValueOrDefault(type, 0) > 0;
            }
        }
    }
}
