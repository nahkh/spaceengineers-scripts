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
            IMyCockpit cockpit = new BlockFinder<IMyCockpit>(this).WithCustomData(settings.CockpitTag).InSameConstructAs(Me).Get();
            List<Vector3I> positions = GridScanner.GetBlocks(cockpit);
            ScriptDisplay scriptDisplay = new ScriptDisplay(Me, Runtime);
            int logDisplayNumber = settings.LogSurface;
            Action<string> logger = str => { };
            if (logDisplayNumber >= 0)
            {
                LogDisplay log = new LogDisplay(cockpit.GetSurface(logDisplayNumber), 8, 17);
                logger = log.Log;
            }
            
            logger.Invoke("Starting");
            logger.Invoke("BC " + positions.Count);
            positionRenderer = new PositionRenderer(cockpit.CubeGrid, settings, logger, cockpit.GetSurface(settings.MainSurface), positions, 10);
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
