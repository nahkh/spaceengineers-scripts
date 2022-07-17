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
            private readonly MyIni _ini = new MyIni();

            public Settings(IMyProgrammableBlock programmableBlock)
            {
                MyIniParseResult result;
                if (!_ini.TryParse(programmableBlock.CustomData, out result))
                    throw new Exception(result.ToString());

                if (String.IsNullOrEmpty(programmableBlock.CustomData))
                {
                    _ini.AddSection("ProjectAnalyzer");
                    _ini.Set("ProjectAnalyzer", "AssemblerTag", "");
                    _ini.SetComment("ProjectAnalyzer", "AssemblerTag", "Which assemblers to monitor. Leave empty for all.");
                    _ini.Set("ProjectAnalyzer", "ProjectorTag", "ProjectAnalyzer-Projector");
                    _ini.SetComment("ProjectAnalyzer", "ProjectorTag", "Which projector to monitor.");
                    _ini.Set("ProjectAnalyzer", "DisplayTag", "ProjectAnalyzer-Display");
                    _ini.SetComment("ProjectAnalyzer", "DisplayTag", "Which Wide LCD display to show results on.");
                    _ini.Set("ProjectAnalyzer", "CubeSize", "Auto");
                    _ini.SetComment("ProjectAnalyzer", "CubeSize", "Which grid size. Acceptable values are: Small, Large, Auto");
                  
                }
                if (!_ini.ContainsSection("Prices"))
                {
                    _ini.AddSection("Prices");
                    foreach (Component.ComponentType item in Component.Types())
                    {
                        _ini.Set("Prices", item.ToString(), 1f);
                    }

                    _ini.Set("Prices", "MarkupPercent", 5f);
                    _ini.SetComment("Prices", "MarkupPercent", "How much of a price markup");
                    _ini.Set("Prices", "MinimumMarkup", 10000);
                    _ini.SetComment("Prices", "MinimumMarkup", "Minimum markup");
                    _ini.Set("Prices", "ShowPrices", true);
                    _ini.SetComment("Prices", "ShowPrices", "Whether to show prices or not");
                }
                programmableBlock.CustomData = _ini.ToString();
            }

            public string AssemblerTag
            {
                get
                {
                    return _ini.Get("ProjectAnalyzer", "AssemblerTag").ToString("");
                }
            }

            public string ProjectorTag
            {
                get
                {
                    return _ini.Get("ProjectAnalyzer", "ProjectorTag").ToString("ProjectAnalyzer-Projector");
                }
            }

            public string DisplayTag
            {
                get
                {
                    return _ini.Get("ProjectAnalyzer", "DisplayTag").ToString("ProjectAnalyzer-Display");
                }
            }

            public string ProjectorSize
            {
                get
                {
                    return _ini.Get("ProjectAnalyzer", "CubeSize").ToString("Large");
                }
            }

            public decimal PriceFor(Component.ComponentType type)
            {
                return _ini.Get("Prices", type.ToString()).ToDecimal();
            }

            public decimal Markup
            {
                get
                {
                    return (_ini.Get("Prices", "MarkupPercent").ToDecimal() / 100);
                }
            }

            public decimal MinimalMarkup
            {
                get
                {
                    return _ini.Get("Prices", "MinimumMarkup").ToDecimal();
                }
            }

            public bool ShowPrices
            {
                get
                {
                    return _ini.Get("Prices", "ShowPrices").ToBoolean();
                }
            }
        }
    }
}
