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
            IMyPistonBase pistonA = new BlockFinder<IMyPistonBase>(this).withCustomData(tagA).get();
            IMyShipMergeBlock mergeA = new BlockFinder<IMyShipMergeBlock>(this).withCustomData(tagA).get();
            IMyShipConnector connectorA = new BlockFinder<IMyShipConnector>(this).withCustomData(tagA).get();
            IMyPistonBase pistonB = new BlockFinder<IMyPistonBase>(this).withCustomData(tagB).get();
            IMyShipMergeBlock mergeB = new BlockFinder<IMyShipMergeBlock>(this).withCustomData(tagB).get();
            IMyShipConnector connectorB = new BlockFinder<IMyShipConnector>(this).withCustomData(tagB).get();
            IMyPistonBase pistonExtender = new BlockFinder<IMyPistonBase>(this).withCustomData(extenderTag).get();
            return new Walker(0.5f, new PistonAssembly(pistonA, connectorA, mergeA), new PistonAssembly(pistonB, connectorB, mergeB), pistonExtender);
        }

        public void setupStatusDisplay(Walker walker, string tag)
        {
            IMyTextPanel textPanel = new BlockFinder<IMyTextPanel>(this).withCustomData(tag).get();
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
            Display.render();
        }
    }
}
