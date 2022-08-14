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
        private GridScanner gridScanner;
        private PositionRenderer positionRenderer;
        private IMyCockpit cockpit;
        private Action<string> logger;
        private List<Vector3I> positions;
        public Program()
        {
            settings = new Settings(Me);
            if (Me.CustomName.StartsWith("Programmable Block"))
            {
                Me.CustomName = "Program: Damage Monitor";
            }
            ScriptDisplay scriptDisplay = new ScriptDisplay(Me, Runtime, "Damage monitor", "v3");
            Display.Render();


            cockpit = new BlockFinder<IMyCockpit>(this).WithCustomData(settings.CockpitTag).InSameConstructAs(Me).TryGet();
            logger = getLogger(scriptDisplay, settings, cockpit);
            logger.Invoke("Scanning blocks");
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            gridScanner = new GridScanner(this);
        }

        private PositionRenderer CreateRenderer(List<Vector3I> positions)
        {
            IMyTextSurface surface = getSurface(settings, cockpit);
            if (surface != null)
            {
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
                return new PositionRenderer(Me.CubeGrid, settings, logger, surface, positions, 10);
            }
            else
            {
                logger.Invoke("Could not initialize a rendering surface, exiting early. Please check the settings.");
                Echo("Could not initialize a rendering surface, exiting early. Please check the settings.");
                Runtime.UpdateFrequency = UpdateFrequency.None;
                Display.Render();
                return null;
            }
        }

        private Action<string> getLogger(ScriptDisplay scriptDisplay, Settings settings, IMyCockpit cockpit)
        {
            if (String.IsNullOrEmpty(settings.IndepentDisplay) && settings.LogSurface >= 0 && cockpit != null)
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

        public void Main(string argument, UpdateType updateSource)
        {
            if (positionRenderer == null)
            {
                if (gridScanner == null)
                {
                    logger.Invoke("Creating renderer");
                    positionRenderer = CreateRenderer(positions);
                } else
                {
                    if (gridScanner.Process())
                    {
                        logger.Invoke("Scan complete");
                        positions = gridScanner.GetPositions();
                        logger.Invoke("" + positions.Count + " blocks in grid");
                        gridScanner = null;
                    }
                }
            } else
            {
                positionRenderer.Update();
            }
            
            Display.Render();
        }
    }
}
