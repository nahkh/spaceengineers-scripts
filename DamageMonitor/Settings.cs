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
                    _ini.Set("Cockpit", "Tag", "");
                    _ini.SetComment("Cockpit", "Tag", "The string which should appear in the cockpit's CustomData.");
                    _ini.Set("Cockpit", "MainSurface", "0");
                    _ini.SetComment("Cockpit", "MainSurface", "The number of the surface where the damage should be displayed");
                    _ini.Set("Cockpit", "LogSurface", "2");
                    _ini.SetComment("Cockpit", "LogSurface", "The number of the surface where the log should be displayed");
                    _ini.Set("IndependentDisplay", "Tag", "");
                    _ini.SetComment("IndependentDisplay", "Tag", "The string which should appear in an independent surfaces CustomData. If this value is set, the script will ignore any cockpits and only update the independent display.");
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

            public string CockpitTag
            {
                get
                {
                    return _ini.Get("Cockpit", "Tag").ToString();
                }
            }
            public int MainSurface
            {
                get
                {
                    return _ini.Get("Cockpit", "MainSurface").ToInt32();
                }
            }

            public string IndepentDisplay
            {
                get
                {
                    return _ini.Get("IndependentDisplay", "Tag").ToString();
                }
            }

            public int LogSurface
            {
                get
                {
                    return _ini.Get("Cockpit", "LogSurface").ToInt32(-1);
                }
            }
        }
    }
}
