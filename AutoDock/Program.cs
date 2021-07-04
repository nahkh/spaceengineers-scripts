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
        private static readonly string callbackArgument = "UNICAST";
        Vector3D position;
        DockController dockController;
        ScriptDisplay scriptDisplay;
        Settings settings;
        public Program()
        {
            settings = new Settings(Me);
            scriptDisplay = new ScriptDisplay(Me, Runtime);
            dockController = new DockController(
                new BlockFinder<IMyShipConnector>(this).withCustomData(settings.AutodockTag).get(), 
                new BlockFinder<IMyPistonBase>(this).withCustomData(settings.PerpendicularTag).getAll(),
                new BlockFinder<IMyPistonBase>(this).withCustomData(settings.ExtendingTag).getAll(),
                new BlockFinder<IMyMotorAdvancedStator>(this).withCustomData(settings.AutodockTag).getAll());

            new StatusDisplay(new BlockFinder<IMyTextPanel>(this).withCustomData("LCD:AUTODOCK").get(), 36, 30)
                .withCenteredLabel("Autodock")
                .withHorizontalLine()
                .withRow("State", dockController.StateInfo)
                .withRow("Pistons", dockController.PistonCount)
                .withRow("Address", () => IGC.Me.ToString())
                .build();
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            IGC.UnicastListener.SetMessageCallback(callbackArgument);
        }

        public void Save()
        {
     
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument == callbackArgument)
            {
                ProcessMessages();
            }
            dockController.Update();
            Display.render();
        }

        private void ProcessMessages()
        {
            while (IGC.UnicastListener.HasPendingMessage)
            {
                scriptDisplay.Write("Processing message");
                MyIGCMessage myIGCMessage = IGC.UnicastListener.AcceptMessage();
                if (myIGCMessage.Tag == DockingRequest.tag)
                {
                    string body = myIGCMessage.As<string>();
                    DockingRequest dockingRequest = DockingRequest.Parse(body);
                    if (dockingRequest == null)
                    {
                        scriptDisplay.Write("Received bad docking request: " + body);
                    } else
                    {
                        scriptDisplay.Write("Connecting to " + dockingRequest.ToString());
                        dockController.ConnectTo(dockingRequest.Position, dockingRequest.Orientation);
                    }
                }
            }
            
        }
    }
}
