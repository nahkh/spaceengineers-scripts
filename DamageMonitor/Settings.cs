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
        public class Settings
        {
            MyIni _ini = new MyIni();

            public Settings(IMyProgrammableBlock programmableBlock)
            {
                MyIniParseResult result;
                if (!_ini.TryParse(programmableBlock.CustomData, out result))
                    throw new Exception(result.ToString());
                if (String.IsNullOrEmpty(programmableBlock.CustomData))
                {
                    _ini.Set("Axis", "X", "X");
                    _ini.SetComment("Axis", "X", "Which direction is the rendered X-axis on the display. Possible values are X, Y, Z, -X, -Y, -Z");
                    _ini.Set("Axis", "Y", "Z");
                    _ini.SetComment("Axis", "Y", "Which direction is the rendered Y-axis on the display. Possible values are X, Y, Z, -X, -Y, -Z");
                    programmableBlock.CustomData = _ini.ToString();
                }
            }

            public Func<Vector3I, int> XAxis
            {
                get
                {
                    string setting = _ini.Get("Axis", "X").ToString("X");
                    return GetFunc(setting);
                }
            }

            public Func<Vector3I, int> YAxis
            {
                get
                {
                    string setting = _ini.Get("Axis", "Y").ToString("Z");
                    return GetFunc(setting);
                }
                
            }

            private Func<Vector3I, int> GetFunc(string name)
            {
                if (name.StartsWith("-"))
                {
                    return vec => -GetFunc(name.Substring(1)).Invoke(vec);
                }
                switch (name)
                {
                    case "X":
                        return vec => vec.X;
                    case "Y":
                        return vec => vec.Y;
                    case "Z":
                        return vec => vec.Z;
                    default:
                        return vec => 0;
                }
            }
        }
    }
}
