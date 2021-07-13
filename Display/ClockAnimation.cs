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
        public class ClockAnimation
        {
            private static readonly TimeSpan interval = TimeSpan.FromSeconds(1);
            private DateTime lastUpdate = DateTime.Now;
            int position = 0;
            string[] elements = new string[] { "/", "-", "\\", "|" };

            public string Get()
            {
                if (lastUpdate + interval < DateTime.Now)
                {
                    lastUpdate = DateTime.Now;
                    position = (position + 1) % elements.Length;
                }
                return elements[position];
            }
        }
    }
}
