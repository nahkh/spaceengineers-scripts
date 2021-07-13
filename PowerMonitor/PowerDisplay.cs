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
        public class PowerDisplay
        {
            private readonly IMyTextSurface textSurface;
            private readonly BatteryWatcher batteryWatcher;

            public PowerDisplay(IMyTextSurface textSurface, BatteryWatcher batteryWatcher)
            {
                this.textSurface = textSurface;
                this.batteryWatcher = batteryWatcher;
            }

            public enum RowType
            {
                All,
                Auto,
                Rechg,
                Dischg,
            }

            public enum ColumnType
            {
                LABEL,
                COUNT,
                CHARGE,
                CHG,
            }

            private string Render(RowType rowType, ColumnType columnType)
            {
                switch(columnType)
                {
                    case ColumnType.LABEL:
                        switch (rowType)
                        {
                            case RowType.All:
                                return "All";
                            default:
                                return " " + rowType.ToString();
                        }
                    case ColumnType.COUNT:
                        switch (rowType)
                        {
                            case RowType.All:
                                return "";
                            case RowType.Auto:
                                return batteryWatcher.BatteryCount(ChargeMode.Auto).ToString();
                            case RowType.Rechg:
                                return batteryWatcher.BatteryCount(ChargeMode.Recharge).ToString();
                            case RowType.Dischg:
                                return batteryWatcher.BatteryCount(ChargeMode.Discharge).ToString();
                            default:
                                return "";

                        }
                    case ColumnType.CHARGE:
                        switch(rowType)
                        {
                            case RowType.All:
                                return FormatPercentage(batteryWatcher.ChargeAsFraction());
                            case RowType.Auto:
                                return FormatPercentage(batteryWatcher.ChargeAsFraction(ChargeMode.Auto));
                            case RowType.Rechg:
                                return FormatPercentage(batteryWatcher.ChargeAsFraction(ChargeMode.Recharge));
                            case RowType.Dischg:
                                return FormatPercentage(batteryWatcher.ChargeAsFraction(ChargeMode.Discharge));
                            default:
                                return "";
                        }
                    case ColumnType.CHG:
                        switch(rowType)
                        {
                            case RowType.All:
                                return FormatWatts(batteryWatcher.CurrentInput() - batteryWatcher.CurrentOutput());
                            case RowType.Auto:
                                return FormatWatts(batteryWatcher.CurrentInput(ChargeMode.Auto) - batteryWatcher.CurrentOutput(ChargeMode.Auto));
                            case RowType.Rechg:
                                return FormatWatts(batteryWatcher.CurrentInput(ChargeMode.Recharge) - batteryWatcher.CurrentOutput(ChargeMode.Recharge));
                            case RowType.Dischg:
                                return FormatWatts(batteryWatcher.CurrentInput(ChargeMode.Discharge) - batteryWatcher.CurrentOutput(ChargeMode.Discharge));
                            default:
                                return "";
                        }
                    default:
                        return "";
                }
            }

            private string FormatPercentage(float fraction)
            {
                return (fraction * 100).ToString("n2") + "%";
            }

            private string FormatWatts(float power)
            {
                if (Math.Abs(power) < 0.0001f)
                {
                    return (power * 1000000f).ToString("n2") + " W";
                }
                if (Math.Abs(power) < 1f)
                {
                    return (power * 1000f).ToString("n2") + " kW";
                }
                return power.ToString("n2") + " MW";
            }

            public Display Build()
            {
                return new TableDisplay<RowType, ColumnType>(textSurface, 36, 30)
                    .Column(ColumnType.LABEL)
                    .Column(ColumnType.COUNT, alignment:TableDisplay<RowType, ColumnType>.Alignment.RIGHT, columnWidth:8)
                    .Column(ColumnType.CHARGE, alignment: TableDisplay<RowType, ColumnType>.Alignment.RIGHT, columnWidth: 10)
                    .Column(ColumnType.CHG, alignment: TableDisplay<RowType, ColumnType>.Alignment.RIGHT, columnWidth: 12)
                    .Row(RowType.All)
                    .Row(RowType.Auto, batteryWatcher.HasAuto)
                    .Row(RowType.Rechg, batteryWatcher.HasRecharging)
                    .Row(RowType.Dischg, batteryWatcher.HasDischarging)
                    .WithoutRowLabel()
                    .WithData(Render)
                    .Build();
            }
        }
    }
}
