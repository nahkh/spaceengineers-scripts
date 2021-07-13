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
        Settings settings;
        ProductionSummary productionSummary;
        AssemblerManager assemblerManager;
        ComponentSummary componentSummary;
        Dictionary<Component.ComponentType, int> neededComponents;
        Dictionary<string, int> rawData;
        IMyProjector projector;

        public Program()
        {
            settings = new Settings(Me);
            projector = new BlockFinder<IMyProjector>(this).WithCustomData(settings.ProjectorTag).Get();
            rawData = ProjectorExtractor.ExtractFromProjector(projector.RemainingBlocksPerType);
            componentSummary = new ComponentSummary(new BlockFinder<IMyTerminalBlock>(this)
                .InSameConstructAs(Me)
                .WithCustomPredicate(block => block.HasInventory)
                .GetAll());
            new ScriptDisplay(Me, Runtime, name: "ProjectAnalyzer");
            List<IMyAssembler> assemblers = new BlockFinder<IMyAssembler>(this).InSameConstructAs(Me).WithCustomData(settings.AssemblerTag).GetAll();
            productionSummary = new ProductionSummary(assemblers, Echo);
            assemblerManager = new AssemblerManager(assemblers, Echo);
            neededComponents = CalculateNeededAmounts();
            BuildAnalyzerDisplay();
            
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        private Dictionary<Component.ComponentType, int> CalculateNeededAmounts()
        {
            MyCubeSize cubeSize;
            switch(settings.ProjectorSize)
            {
                case "Large":
                    cubeSize = MyCubeSize.Large;
                    break;
                case "Small":
                    cubeSize = MyCubeSize.Small;
                    break;
                case "Auto":
                default:
                    cubeSize = projector.CubeGrid.GridSizeEnum;
                    break;
            }

            Dictionary<Component.ComponentType, int> neededComponents = new Dictionary<Component.ComponentType, int>();
            foreach (string id in rawData.Keys)
            {
                BlockType blockType = BlockIndex.GetBlockType(id, cubeSize);
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
                            .WithCustomData(settings.DisplayTag)
                            .Get(),
                        CachingProvider.Of(() => neededComponents, TimeSpan.FromSeconds(10)),
                        CachingProvider.Of(componentSummary.IntAmounts, TimeSpan.FromSeconds(10)),
                        CachingProvider.Of(productionSummary.ProductionQueueSummary, TimeSpan.FromSeconds(10)))
                    .Build();
        }

        public void Save()
        {
           
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument == "CLEAR")
            {
                Clear();
            }
            if (argument == "ENQUEUE")
            {
                Enqueue();
            }
            if (argument == "RECALCULATE")
            {
                neededComponents = CalculateNeededAmounts();
            }
            Display.Render();
        }

        private void Clear()
        {
            assemblerManager.ClearQueues();
        }

        private void Enqueue()
        {
            Dictionary<Component.ComponentType, int> availableComponents = componentSummary.IntAmounts();
            Dictionary<Component.ComponentType, int> enqueuedComponents = productionSummary.ProductionQueueSummary();
            foreach (Component.ComponentType type in Component.Types())
            {
                int needed = neededComponents.GetValueOrDefault(type, 0);
                int available = availableComponents.GetValueOrDefault(type, 0);
                int enqueued = enqueuedComponents.GetValueOrDefault(type, 0);
                int toEnqueue = needed - available - enqueued;
                assemblerManager.AddToQueue(type, toEnqueue);
            }
        }
    }
}
