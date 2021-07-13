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
        BatteryController batteryController;
        IMyShipConnector connector;

        public Program()
        {
            List<IMyBatteryBlock> batteries = new BlockFinder<IMyBatteryBlock>(this)
                .InSameConstructAs(Me)
                .GetAll();

            float maxCharge = batteries.ConvertAll(battery => battery.MaxStoredPower).Max();
            int maxIndex = batteries.FindIndex(battery => battery.MaxStoredPower == maxCharge);
            IMyBatteryBlock mainBattery = batteries[maxIndex];
            IMyBatteryBlock backupBattery = batteries[1 - maxIndex];
            batteryController = new BatteryController(mainBattery, backupBattery);
            connector = new BlockFinder<IMyShipConnector>(this).InSameConstructAs(Me).Get();
            
            new StatusDisplay(new BlockFinder<IMyCockpit>(this).InSameConstructAs(Me).Get().GetSurface(1), 35, 15)
                .WithCenteredLabel("Battery status")
                .WithHorizontalLine()
                .WithRow("Status", batteryController.StateInfo)
                .WithRow("Main", batteryController.MainInfo)
                .WithRow("Backup", batteryController.BackupInfo)
                .WithRow("Charge", batteryController.ChargeInfo)
                .Build();

            new ScriptDisplay(Me, Runtime);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Main(string argument, UpdateType updateSource)
        {
           if (connector.Status == MyShipConnectorStatus.Connected)
           {
                batteryController.BackupPower();
           } else
           {
               batteryController.MainPower();
            }
            batteryController.Update();
            Display.Render();
        }
    }
}
