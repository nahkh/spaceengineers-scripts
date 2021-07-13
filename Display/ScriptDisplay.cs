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
    partial class Program
    {
        public class ScriptDisplay
        {
            private readonly StatusDisplay.LogBuffer buffer;
            public ScriptDisplay(IMyProgrammableBlock programmableBlock, IMyGridProgramRuntimeInfo runtimeInfo, string name=null, string version=null)
            {
                if (name == null)
                {
                    name = programmableBlock.CustomName;
                }
                if (version != null)
                {
                    name += " " + version;
                }
                buffer = new StatusDisplay.LogBuffer(8);
                new StatusDisplay(programmableBlock.GetSurface(0), 36, 15)
                    .WithRow("Running script", () => name)
                    .WithHorizontalLine()
                    .WithRow("Update frequency", () => runtimeInfo.UpdateFrequency.ToString())
                    .WithRow("Call chain depth", () => runtimeInfo.CurrentCallChainDepth.ToString())
                    .WithRow("Instruction count", () => runtimeInfo.CurrentInstructionCount.ToString())
                    .WithHorizontalLine()
                    .WithLog(buffer)
                    .WithTime()
                    .Build();
            }

            public void Write(string logMessage)
            {
                buffer.Write(DateTime.Now.ToString("HH:mm:ss") + " " + logMessage);
            }
        }
    }
}
