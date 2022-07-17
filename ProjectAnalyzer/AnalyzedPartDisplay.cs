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
            private readonly int wideLcdWidth = 74;
            private readonly IMyTextSurface textSurface;
            private readonly Func<Dictionary<Component.ComponentType, int>> neededPartsProvider;
            private readonly Func<Dictionary<Component.ComponentType, int>> currentInventoryProvider;
            private readonly Func<Dictionary<Component.ComponentType, int>> productionCount;
            private readonly Settings settings;
            

            enum ColumnType
            {
                Total,
                Available,
                Missing,
                Building,
                Cost,
            }

            public AnalyzedPartDisplay(
                IMyTextSurface textSurface, 
                Func<Dictionary<Component.ComponentType, int>> neededPartsProvider,
                Func<Dictionary<Component.ComponentType, int>> currentInventoryProvider,
                Func<Dictionary<Component.ComponentType, int>> productionCount,
                Settings settings)
            {
                this.textSurface = textSurface;
                this.neededPartsProvider = neededPartsProvider;
                this.currentInventoryProvider = currentInventoryProvider;
                this.productionCount = productionCount;
                this.settings = settings;
            }

            public Display Build()
            {
                TableDisplay<Component.ComponentType, ColumnType> table = new TableDisplay<Component.ComponentType, ColumnType>(textSurface, wideLcdWidth, 30);

                foreach(Component.ComponentType type in Component.Types())
                {
                    table.Row(type, NeedAnyOf(type));
                }
                table.Column(ColumnType.Total, alignment: StringUtil.Alignment.RIGHT, columnWidth: 7)
                    .Column(ColumnType.Available, alignment: StringUtil.Alignment.RIGHT, columnWidth: 10)
                    .Column(ColumnType.Missing, alignment: StringUtil.Alignment.RIGHT, columnWidth: 10)
                    .Column(ColumnType.Building, alignment: StringUtil.Alignment.RIGHT, columnWidth: 10)
                    .WithData(Render);

                if (settings.ShowPrices)
                {
                    table
                        .Column(ColumnType.Cost, alignment: StringUtil.Alignment.RIGHT)
                        .AdditionalRow(() => "Parts cost" + StringUtil.Pad(RenderPartCost(), StringUtil.Alignment.RIGHT, wideLcdWidth - 10))
                        .AdditionalRow(() => "Construction fee" + StringUtil.Pad(RenderFee(), StringUtil.Alignment.RIGHT, wideLcdWidth - 16))
                        .AdditionalRow(() => "Total cost" + StringUtil.Pad(RenderTotalCost(), StringUtil.Alignment.RIGHT, wideLcdWidth - 10));
                } else
                {
                    table.WithLabel("Parts needed for project");
                }

                return table.Build();
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
                    case ColumnType.Cost:
                        decimal cost = NeededCount(component) * CostPer(component);
                        if (cost == 0)
                        {
                            return "";
                        }
                        return cost.ToString("n0");
                }
                return "";
            }

            private string RenderTotalCost()
            {
                decimal partCost = CalculatePartCost();
                return "= " + (partCost + CalculateFee(partCost)).ToString("n0") + " SC";
            }

            private string RenderPartCost()
            {
                return CalculatePartCost().ToString("n0") + " SC";
            }
            private string RenderFee()
            {
                return "+ " + CalculateFee(CalculatePartCost()).ToString("n0") + " SC";
            }

            private decimal CalculatePartCost()
            {
                decimal totalCost = 0;
                foreach (Component.ComponentType type in Component.Types())
                {
                    int neededCount = NeededCount(type);
                    totalCost += neededCount * CostPer(type);
                }
                return totalCost;
            }

            private decimal CalculateFee(decimal partCost)
            {
                return Math.Ceiling(Math.Max(settings.MinimalMarkup, settings.Markup * partCost));
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

            private decimal CostPer(Component.ComponentType component)
            {
                return settings.PriceFor(component);
            }

            private Func<bool> NeedAnyOf(Component.ComponentType type)
            {
                return () => neededPartsProvider.Invoke().GetValueOrDefault(type, 0) > 0;
            }
        }
    }
}
