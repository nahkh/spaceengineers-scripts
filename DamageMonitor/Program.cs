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
            if (Me.CustomName.StartsWith("Programmable Block"))
            {
                Me.CustomName = "Program: Damage Monitor";
            }
            List<Vector3I> positions = GridScanner.GetBlocks(Me);
            ScriptDisplay scriptDisplay = new ScriptDisplay(Me, Runtime);
            int logDisplayNumber = settings.LogSurface;
            
            IMyCockpit cockpit = new BlockFinder<IMyCockpit>(this).WithCustomData(settings.CockpitTag).InSameConstructAs(Me).TryGet();
            Action<string> logger = getLogger(scriptDisplay, settings, cockpit);
            logger.Invoke("Starting");
            logger.Invoke("BC " + positions.Count);
            IMyTextSurface surface = getSurface(settings, cockpit);
            if (surface != null)
            {
                positionRenderer = new PositionRenderer(Me.CubeGrid, settings, logger, surface, positions, 10);
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
            } else
            {
                logger.Invoke("Could not initialize a rendering surface, exiting early. Please check the settings.");
                Echo("Could not initialize a rendering surface, exiting early. Please check the settings.");
            }
            
        }

        private Action<string> getLogger(ScriptDisplay scriptDisplay, Settings settings, IMyCockpit cockpit)
        {
            if (settings.LogSurface >= 0 && cockpit != null)
            {
                LogDisplay log = new LogDisplay(cockpit.GetSurface(settings.LogSurface), 8, 17);
                return log.Log;
            } else
            {
                return scriptDisplay.Write;
            }
        }

        private IMyTextSurface getSurface(Settings settings, IMyCockpit cockpit)
        {
            if (settings.IndepentDisplay.Length > 0)
            {
                return new BlockFinder<IMyTextPanel>(this).InSameConstructAs(Me).WithCustomData(settings.IndepentDisplay).Get();
            } else if (cockpit != null) {
                return cockpit.GetSurface(settings.MainSurface);
            } else
            {
               return null;
            }
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
