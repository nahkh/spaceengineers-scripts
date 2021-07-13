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
        public class ProjectorExtractor
        {

            public static Dictionary<string, int> ExtractFromProjector<T>(Dictionary<T, int> amounts) {
                Dictionary<string, int> unboxedValues = new Dictionary<string, int>();
                foreach(T key in amounts.Keys)
                {
                    unboxedValues[key.ToString()] = amounts[key];
                }
                return unboxedValues;
            }

        }
    }
}
