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
    partial class Program : MyGridProgram
    {
        ProductionSummary productionSummary;
        ComponentSummary componentSummary;
        Dictionary<Component.ComponentType, int> neededComponents;
        Dictionary<string, int> rawData;
        IMyProjector projector;

        public Program()
        {
            projector = new BlockFinder<IMyProjector>(this).WithCustomData("Projector-Test").Get();
            rawData = ProjectorExtractor.ExtractFromProjector(projector.RemainingBlocksPerType);
            componentSummary = new ComponentSummary(new BlockFinder<IMyTerminalBlock>(this)
                .InSameConstructAs(Me)
                .WithCustomPredicate(block => block.HasInventory)
                .GetAll());
            new ScriptDisplay(Me, Runtime, name: "ProjectAnalyzer");
            productionSummary = new ProductionSummary(new BlockFinder<IMyAssembler>(this).InSameConstructAs(Me).GetAll(), Echo);
            neededComponents = CalculateNeededAmounts();
            BuildAnalyzerDisplay();
            
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        private Dictionary<Component.ComponentType, int> CalculateNeededAmounts()
        {
            Dictionary<Component.ComponentType, int> neededComponents = new Dictionary<Component.ComponentType, int>();
            foreach (string id in rawData.Keys)
            {
                BlockType blockType = BlockIndex.GetBlockType(id, projector.CubeGrid.GridSizeEnum);
                if (blockType == null)
                {
                    Echo(id);
                }
                else
                {
                    foreach (Component.ComponentType type in blockType.Components.Keys)
                    {
                        neededComponents[type] = neededComponents.GetValueOrDefault(type, 0) + blockType.Components[type] * rawData[id];
                    }
                }
            }
            return neededComponents;
        }

        private Display BuildAnalyzerDisplay()
        {
            return new AnalyzedPartDisplay(
                        new BlockFinder<IMyTextPanel>(this)
                            .InSameConstructAs(Me)
                            .WithCustomData("Projector-Need-LCD")
                            .Get(),
                        CachingProvider.Of(neededComponents),
                        CachingProvider.Of(componentSummary.IntAmounts),
                        CachingProvider.Of(productionSummary.ProductionQueueSummary, TimeSpan.FromSeconds(10)))
                    .Build();
        }

        public void Save()
        {
           
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Display.Render();
        }
    }
}
