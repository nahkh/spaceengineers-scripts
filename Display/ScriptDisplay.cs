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
            public ScriptDisplay(IMyProgrammableBlock programmableBlock, IMyGridProgramRuntimeInfo runtimeInfo)
            {
                new StatusDisplay(programmableBlock.GetSurface(0), 36, 15)
                    .withRow("Running script", () => programmableBlock.CustomName)
                    .withHorizontalLine()
                    .withRow("Update frequency", () => runtimeInfo.UpdateFrequency.ToString())
                    .withRow("Call chain depth", () => runtimeInfo.CurrentCallChainDepth.ToString())
                    .withRow("Instruction count", () => runtimeInfo.CurrentInstructionCount.ToString())
                    .withTime()
                    .build();
            }
        }
    }
}
