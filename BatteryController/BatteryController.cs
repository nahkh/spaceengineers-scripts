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
        public class BatteryController
        {
            
            private readonly IMyBatteryBlock mainBattery;
            private readonly IMyBatteryBlock backupBattery;

            enum State
            {
                ON_MAIN,
                TO_BACKUP,
                ON_BACKUP,
                TO_MAIN,
            }

            private State state;

            public BatteryController(IMyBatteryBlock mainBattery, IMyBatteryBlock backupBattery)
            {
                this.mainBattery = mainBattery;
                this.backupBattery = backupBattery;
                if (mainBattery.ChargeMode == ChargeMode.Recharge || mainBattery.CurrentStoredPower <= float.Epsilon)
                {
                    backupBattery.ChargeMode = ChargeMode.Auto;
                    state = State.ON_BACKUP;
                } else
                {
                    mainBattery.ChargeMode = ChargeMode.Auto;
                    state = State.TO_MAIN;
                }
            }


            public void MainPower()
            {
                if (state == State.ON_BACKUP)
                {
                    state = State.TO_MAIN;
                    mainBattery.ChargeMode = ChargeMode.Auto;
                }
            }

            public void BackupPower()
            {
                if (state == State.ON_MAIN)
                {
                    state = State.TO_BACKUP;
                    backupBattery.ChargeMode = ChargeMode.Auto;
                }
            }

            public void Update()
            {
                switch(state)
                {
                    case State.TO_MAIN:
                        if (mainBattery.ChargeMode != ChargeMode.Recharge)
                        {
                            backupBattery.ChargeMode = ChargeMode.Recharge;
                            state = State.ON_MAIN;
                        }
                        break;
                    case State.TO_BACKUP:
                        if (backupBattery.ChargeMode != ChargeMode.Recharge)
                        {
                            mainBattery.ChargeMode = ChargeMode.Recharge;
                            state = State.ON_BACKUP;
                        }
                        break;
                    case State.ON_MAIN:
                        if (mainBattery.CurrentStoredPower <= float.Epsilon)
                        {
                            state = State.TO_BACKUP;
                            backupBattery.ChargeMode = ChargeMode.Auto;
                        }
                        break;
                    case State.ON_BACKUP:
                        break;
                }
            }

            public string StateInfo()
            {
                return state.ToString();
            }

            public string MainInfo()
            {
                float chargePercent = 100 * mainBattery.CurrentStoredPower / mainBattery.MaxStoredPower;
                return chargePercent.ToString("n2") + " %";
            }

            public string BackupInfo()
            {
                float chargePercent = 100 * backupBattery.CurrentStoredPower / backupBattery.MaxStoredPower;
                return chargePercent.ToString("n2") + " %";
            }

            public string ChargeInfo()
            {
                if (Math.Abs(mainBattery.CurrentStoredPower - mainBattery.MaxStoredPower) < 0.001f)
                {
                    return "Full";
                }

                float changeInCharge = Math.Abs(mainBattery.CurrentInput - mainBattery.CurrentOutput);
                string direction;
                float capacity;
                if (mainBattery.CurrentInput > mainBattery.CurrentOutput)
                {
                    // charging
                    direction = "Full in ";
                    capacity = mainBattery.MaxStoredPower - mainBattery.CurrentStoredPower;
                } else
                {
                    direction = "Empty in ";
                    capacity = mainBattery.CurrentStoredPower;
                }

                float seconds = capacity / changeInCharge;

                return direction + TimeSpan.FromSeconds(seconds).ToString();
            }
        }
    }
}
