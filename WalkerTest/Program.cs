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
        Walker walker;

        public Program()
        {
            new ScriptDisplay(Me, Runtime);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            walker = buildWalker("A", "B", "E");
            setupStatusDisplay(walker, "WalkerLCD");
        }

        public Walker buildWalker(string tagA, string tagB, string extenderTag)
        {
            IMyPistonBase pistonA = new BlockFinder<IMyPistonBase>(this).WithCustomData(tagA).Get();
            IMyShipMergeBlock mergeA = new BlockFinder<IMyShipMergeBlock>(this).WithCustomData(tagA).Get();
            IMyShipConnector connectorA = new BlockFinder<IMyShipConnector>(this).WithCustomData(tagA).Get();
            IMyPistonBase pistonB = new BlockFinder<IMyPistonBase>(this).WithCustomData(tagB).Get();
            IMyShipMergeBlock mergeB = new BlockFinder<IMyShipMergeBlock>(this).WithCustomData(tagB).Get();
            IMyShipConnector connectorB = new BlockFinder<IMyShipConnector>(this).WithCustomData(tagB).Get();
            IMyPistonBase pistonExtender = new BlockFinder<IMyPistonBase>(this).WithCustomData(extenderTag).Get();
            return new Walker(0.5f, new PistonAssembly(pistonA, connectorA, mergeA), new PistonAssembly(pistonB, connectorB, mergeB), pistonExtender);
        }

        public void setupStatusDisplay(Walker walker, string tag)
        {
            IMyTextPanel textPanel = new BlockFinder<IMyTextPanel>(this).WithCustomData(tag).Get();
            new Display(textPanel, walker.StatusInfo);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument == "FORWARD")
            {
                walker.StepForward();
            }
            if (argument == "BACKWARD")
            {
                walker.StepBackward();
            }
            if (argument == "EMERGENCYSTOP")
            {
                walker.EmergencyShutdown();
            }
            walker.Update();
            Display.Render();
        }
    }
}
