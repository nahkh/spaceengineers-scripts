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
            }


            public string ForwardLegTag
            {
                get
                {
                    return _ini.Get("constructionSettings", "forwardLegTag").ToString("WALKER-A");
                }
            }
            public string RearLegTag
            {
                get
                {
                    return _ini.Get("constructionSettings", "rearLegTag").ToString("WALKER-B");
                }
            }

            public string ExtenderLegTag
            {
                get
                {
                    return _ini.Get("constructionSettings", "extenderTag").ToString("WALKER-E");
                }
            }

            public string ConstructionTag
            { 
                get
                {
                    return _ini.Get("constructionSettings", "constructionPistonTag").ToString("CONSTRUCTION-PISTON");
                }
            }

            public string BuildFilterTag
            {
                get
                {
                    return _ini.Get("constructionSettings", "buildFilterTag").ToString("CONSTRUCTION-BUILDFILTER");
                }

            }
            public string DestructionFilterTag
            {
                get
                {
                    return _ini.Get("constructionSettings", "destructionFilterTag").ToString("CONSTRUCTION-DESTRUCTIONFILTER");
                }
            }

            public string StatusDisplayTag
            {
                get
                {
                    return _ini.Get("constructionSettings", "statusDisplayTag").ToString("DISPLAY-STATUS");
                }
            }
            public string OreDisplaytag
            {
                get
                {
                    return _ini.Get("constructionSettings", "oreDisplayTag").ToString("DISPLAY-ORE");
                }
            }


            public string BraceLegTag
            {
                get
                {
                    return _ini.Get("constructionSettings", "braceTag").ToString("BRACE");
                }
            }

            public float DesiredDepth
            {
                get
                {
                    return (float)_ini.Get("drillSettings", "desiredDepth").ToDouble(100f);
                }
            }
        }
    }
}
