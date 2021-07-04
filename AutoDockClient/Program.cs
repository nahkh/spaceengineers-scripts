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
        IMyShipConnector connector;
        ScriptDisplay display;
        Settings settings;

        public Program()
        {
            settings = new Settings(Me);
            display = new ScriptDisplay(Me, Runtime);
            connector = new BlockFinder<IMyShipConnector>(this).withCustomData(settings.ConnectorTag).get();
            Display.render();
        }

        public void Save()
        {
        
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument == "SEND")
            {
                display.Write("Sending docking request to:");
                display.Write(settings.DockId.ToString());
                DockingRequest request = BuildRequest();
                IGC.SendUnicastMessage(settings.DockId, DockingRequest.tag, request.ToString());
            }
            Display.render();
        }

        private DockingRequest BuildRequest()
        {
            Vector3D spot = connector.WorldMatrix.Translation + (connector.WorldMatrix.Forward * 2.5);
            Vector3D orientation = connector.WorldMatrix.Backward;
            return new DockingRequest(spot, orientation);
        }
    }
}
