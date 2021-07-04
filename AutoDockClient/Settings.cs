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


            public long DockId
            {
                get
                {
                    return _ini.Get("AutoDock", "DockId").ToInt64();
                }
            }

            public string ConnectorTag
            {
                get
                {
                    return _ini.Get("AutoDock", "Connector").ToString("AutoDock-Connector");
                }
            }
        }
    }
}
