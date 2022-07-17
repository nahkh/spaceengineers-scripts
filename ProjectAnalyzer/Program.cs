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
        MyCubeSize cubeSize;
        ProductionSummary productionSummary;
        AssemblerManager assemblerManager;
        ComponentSummary componentSummary;
        Dictionary<Component.ComponentType, int> neededComponents;
        Dictionary<string, int> rawData;
        IMyProjector projector;
        CachingProvider.Cache<Dictionary<Component.ComponentType, int>> availableItemCache;
        CachingProvider.Cache<Dictionary<Component.ComponentType, int>> productionQueueCache;

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
            neededComponents = new Dictionary<Component.ComponentType, int>();
            DetectCubeSize();
            CalculateNeededAmounts();
            SetCubeSize();
            SetupCaches();
            BuildAnalyzerDisplay();
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        private void SetCubeSize()
        {
            switch (settings.ProjectorSize)
            {
                case "Large":
                    cubeSize = MyCubeSize.Large;
                    break;
                case "Small":
                    cubeSize = MyCubeSize.Small;
                    break;
                case "Auto":
                default:
                    cubeSize = DetectCubeSize();
                    break;
            }
        }

        private void SetupCaches()
        {
            availableItemCache = new CachingProvider.Cache<Dictionary<Component.ComponentType, int>>(componentSummary.IntAmounts, TimeSpan.FromMinutes(1));
            productionQueueCache = new CachingProvider.Cache<Dictionary<Component.ComponentType, int>>(productionSummary.ProductionQueueSummary, TimeSpan.FromMinutes(1));
        }

        private void InvalidateCaches()
        {
            availableItemCache.Invalidate();
            productionQueueCache.Invalidate();
        }

        private MyCubeSize DetectCubeSize()
        {
            int largeCount = 0;
            int smallCount = 0;
            foreach (string id in rawData.Keys)
            {
                BlockType largeBlockType = BlockIndex.GetBlockType(id, MyCubeSize.Large);
                BlockType smallBlockType = BlockIndex.GetBlockType(id, MyCubeSize.Small);
                if (largeBlockType == null && smallBlockType != null)
                {
                    ++smallCount;
                }
                if (largeBlockType != null && smallBlockType == null)
                {
                    ++largeCount;
                }
            }
            Echo(largeCount.ToString() + " " + smallCount.ToString());
            return largeCount > smallCount ? MyCubeSize.Large : MyCubeSize.Small;
        }

        private void CalculateNeededAmounts()
        {
            neededComponents.Clear();
            foreach (string id in rawData.Keys)
            {
                BlockType blockType = BlockIndex.GetBlockType(id, cubeSize);
                if (blockType == null)
                {
                    Echo(cubeSize.ToString());
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
        }

        private Display BuildAnalyzerDisplay()
        {
            return new AnalyzedPartDisplay(
                        new BlockFinder<IMyTextPanel>(this)
                            .InSameConstructAs(Me)
                            .WithCustomData(settings.DisplayTag)
                            .Get(),
                        () => neededComponents,
                        availableItemCache.Get,
                        productionQueueCache.Get,
                        settings)
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
                productionQueueCache.Invalidate();
            }
            if (argument == "ENQUEUE")
            {
                Enqueue();
                productionQueueCache.Invalidate();
            }
            if (argument == "RECALCULATE")
            {
                rawData = ProjectorExtractor.ExtractFromProjector(projector.RemainingBlocksPerType);
                SetCubeSize();
                CalculateNeededAmounts();
                InvalidateCaches();
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
