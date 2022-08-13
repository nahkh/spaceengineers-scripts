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
        public class UserInput
        {
            public enum XAxis
            {
                FORWARD,
                STILL,
                BACKWARD
            }

            public enum YAxis
            {
                LEFT,
                STILL,
                RIGHT
            }

            public enum ZAxis
            {
                DOWN,
                STILL,
                UP
            }

            public readonly XAxis xAxis;
            public readonly YAxis yAxis;
            public readonly ZAxis zAxis;

            public UserInput(XAxis xAxis, YAxis yAxis, ZAxis zAxis)
            {
                this.xAxis = xAxis;
                this.yAxis = yAxis;
                this.zAxis = zAxis;
            }

            public override string ToString()
            {
                string output = "";
                if (xAxis != XAxis.STILL)
                {
                    output = xAxis.ToString();
                }
                if (yAxis != YAxis.STILL)
                {
                    if (output.Length > 0)
                    {
                        output += " and ";
                    }
                    output += yAxis.ToString();
                }
                if (zAxis != ZAxis.STILL)
                {
                    if (output.Length > 0)
                    {
                        output += " and ";
                    }
                    output += zAxis.ToString();
                }
                if (output.Length == 0)
                {
                    return "STILL";
                } else
                {
                    return output;
                }                
            }

            public bool IsIdle
            {
                get
                {
                    return xAxis == XAxis.STILL && yAxis == YAxis.STILL && zAxis == ZAxis.STILL;
                }
            }
        }
    }
}
