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
        public class BatteryWatcher
        {
            private readonly List<IMyBatteryBlock> batteries;
            private readonly string name;

            public BatteryWatcher(List<IMyBatteryBlock> batteries)
            {
                this.batteries = batteries;
                if (this.batteries.Count == 1)
                {
                    name = "1 battery";
                } else
                {
                    name = this.batteries.Count.ToString() + " batteries";
                }
            }

            public string Name
            {
                get
                {
                    return name;
                }
            }

            public bool HasAuto()
            {
                return batteries.Any(battery => battery.ChargeMode == ChargeMode.Auto);
            }

            public bool HasRecharging()
            {
                return batteries.Any(battery => battery.ChargeMode == ChargeMode.Recharge);
            }

            public bool HasDischarging()
            {
                return batteries.Any(battery => battery.ChargeMode == ChargeMode.Discharge);
            }

            public float ChargeAsFraction()
            {
                return ChargeAsFraction(battery => true);
            }

            public float ChargeAsFraction(ChargeMode chargeMode)
            {
                return ChargeAsFraction(battery => battery.ChargeMode == chargeMode);
            }

            public int BatteryCount(ChargeMode chargeMode)
            {
                return batteries.Count(battery => battery.ChargeMode == chargeMode);
            }

            private float ChargeAsFraction(Func<IMyBatteryBlock, bool> predicate)
            {
                return Apply(predicate, battery => battery.CurrentStoredPower) / Apply(predicate, battery => battery.MaxStoredPower);
            }

            public float CurrentInput()
            {
                return CurrentInput(battery => true);
            }

            public float CurrentInput(ChargeMode chargeMode)
            {
                return CurrentInput(battery => battery.ChargeMode == chargeMode);
            }

            private float CurrentInput(Func<IMyBatteryBlock, bool> predicate)
            {
                return Apply(predicate, battery => battery.CurrentInput);
            }

            public float CurrentOutput()
            {
                return CurrentOutput(battery => true);
            }

            public float CurrentOutput(ChargeMode chargeMode)
            {
                return CurrentOutput(battery => battery.ChargeMode == chargeMode);
            }

            private float CurrentOutput(Func<IMyBatteryBlock, bool> predicate)
            {
                return Apply(predicate, battery => battery.CurrentOutput);
            }

            private float Apply(Func<IMyBatteryBlock, bool> predicate, Func<IMyBatteryBlock, float> map)
            {
                float value = 0.0f;
                foreach (IMyBatteryBlock battery in batteries)
                {
                    if (predicate.Invoke(battery))
                    {
                        value += map.Invoke(battery);
                    }
                }
                return value;
            }
        }
    }
}
