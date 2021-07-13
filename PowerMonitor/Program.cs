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
    partial class Program : MyGridProgram
    {
        
        public Program()
        {
            List<IMyBatteryBlock> batteries = new BlockFinder<IMyBatteryBlock>(this).InSameConstructAs(Me).GetAll();
            IMyTextPanel textPanel = new BlockFinder<IMyTextPanel>(this).WithCustomData("PowerMonitor-Display").Get();
            BatteryWatcher batteryWatcher = new BatteryWatcher(batteries);
            new PowerDisplay(textPanel, batteryWatcher).Build();
            new ScriptDisplay(Me, Runtime);
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Save()
        {
            
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Display.Render();
        }
    }
}
