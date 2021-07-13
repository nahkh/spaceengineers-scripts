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
        private Settings settings;
        private PositionRenderer positionRenderer;
        public Program()
        {
            settings = new Settings(Me);
            IMyCockpit cockpit = new BlockFinder<IMyCockpit>(this).InSameConstructAs(Me).Get();
            List<Vector3I> positions = GridScanner.GetBlocks(cockpit);
            ScriptDisplay scriptDisplay = new ScriptDisplay(Me, Runtime);
            LogDisplay log = new LogDisplay(cockpit.GetSurface(2), 8, 17);
            log.Log("Starting");
            log.Log("BC " + positions.Count);
            positionRenderer = new PositionRenderer(cockpit.CubeGrid, settings, log.Log, cockpit.GetSurface(0), positions, 10);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            positionRenderer.Update();
            Display.Render();
        }
    }
}
