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
        public class PidController
        {
            private readonly double kp;
            private readonly double ki;
            private readonly double kd;
            private readonly double maxI;

            private DateTime lastUpdate;
            private double p;
            private double i;
            private double d;
            private double lastOutput;

            public PidController(double Kp, double Ki, double Kd, double maxI)
            {
                kp = Kp;
                ki = Ki;
                kd = Kd;
                this.maxI = maxI;
                lastUpdate = DateTime.Now;
            }

            public double Signal(double input)
            {
                DateTime currentTime = DateTime.Now;
                double dT = (currentTime - lastUpdate).TotalSeconds;
                i += input * dT;
                d = (input - p) / dT;
                if (i > maxI)
                {
                    i = maxI;
                }
                if (i < -maxI)
                {
                    i = -maxI;
                }
                p = input;
                lastOutput = kp * p + ki * i + kd * d;
                return lastOutput;
            }

            public void Reset()
            {
                p = 0;
                i = 0;
                d = 0;
                lastUpdate = DateTime.Now;
            }

            public string DebugInfo()
            {
                return p.ToString("n2") + " " + i.ToString("n2") + " " + d.ToString("n2") + " -> " + lastOutput.ToString("n2");
            }
        }
    }
}
