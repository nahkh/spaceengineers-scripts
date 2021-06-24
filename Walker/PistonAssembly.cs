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
        public class PistonAssembly
        {
            private readonly IMyPistonBase piston;
            private readonly IMyShipConnector connector;
            private readonly IMyShipMergeBlock mergeBlock;
            
            private static readonly float epsilon = 0.01f;
            private static readonly TimeSpan lockingDelay = TimeSpan.FromSeconds(3);

            private DateTime lockingTime;

            public enum PistonAssemblyState
            {
                ERROR,
                DISCONNECTED,
                CONNECTING,
                LOCKING,
                CONNECTED,
                DISCONNECTING,
            }

            private PistonAssemblyState state;

            public PistonAssembly(IMyPistonBase piston, IMyShipConnector connector, IMyShipMergeBlock mergeBlock)
            {
                this.piston = piston;
                this.connector = connector;
                this.mergeBlock = mergeBlock;
                if ((this.connector.Status == MyShipConnectorStatus.Connected && !mergeBlock.IsConnected) || (this.connector.Status != MyShipConnectorStatus.Connected && mergeBlock.IsConnected))
                {
                    // Unexpected starting state, do nothing to avoid making things worse.
                    state = PistonAssemblyState.ERROR;
                    return;
                }

                if (connector.Status == MyShipConnectorStatus.Connected)
                {
                    state = PistonAssemblyState.CONNECTED;
                    mergeBlock.Enabled = true;
                }
                else if (Math.Abs(piston.CurrentPosition - piston.MinLimit) < epsilon)
                {
                    state = PistonAssemblyState.DISCONNECTED;
                    piston.Velocity = 0.0f;
                }
                else if (piston.Velocity > 0)
                {
                    state = PistonAssemblyState.CONNECTING;
                }
                else
                {
                    state = PistonAssemblyState.DISCONNECTING;
                }
            }

            public void Update()
            {
                switch(state)
                {
                    case PistonAssemblyState.DISCONNECTED:
                    case PistonAssemblyState.CONNECTED:
                    case PistonAssemblyState.ERROR:
                        // Nothing to do on update
                        break;
                    case PistonAssemblyState.CONNECTING:
                        if (mergeBlock.IsConnected)
                        {
                            state = PistonAssemblyState.LOCKING;
                            lockingTime = DateTime.Now + lockingDelay;
                        }
                        break;
                    case PistonAssemblyState.LOCKING:
                        if (connector.Status == MyShipConnectorStatus.Connected)
                        {
                            state = PistonAssemblyState.CONNECTED;
                            piston.Velocity = 0f;
                         
                        } else if (connector.Status == MyShipConnectorStatus.Connectable && lockingTime < DateTime.Now) {
                            connector.Connect();
                        }
                        break;
                    case PistonAssemblyState.DISCONNECTING:
                        if (mergeBlock.Enabled)
                        {
                            mergeBlock.Enabled = false;
                        }
                        if (connector.Status == MyShipConnectorStatus.Connected)
                        {
                            connector.Disconnect();
                        }
                        if (Math.Abs(piston.CurrentPosition - piston.MinLimit) < epsilon)
                        {
                            piston.Velocity = 0f;
                            state = PistonAssemblyState.DISCONNECTED;
                        }
                        break;
                }
            }

            public PistonAssemblyState State
            {
                get
                {
                    return state;
                }
            }

            public void Connect()
            {
                if (state == PistonAssemblyState.DISCONNECTED)
                {
                    state = PistonAssemblyState.CONNECTING;
                    piston.Velocity = 0.5f;
                    mergeBlock.Enabled = true;
                }
            }

            public void Disconnect()
            {
                if (state == PistonAssemblyState.CONNECTED)
                {
                    connector.Disconnect();
                    state = PistonAssemblyState.DISCONNECTING;
                    mergeBlock.Enabled = false;
                    piston.Velocity = -0.5f;
                }
            }

            public void EmergencyShutdown()
            {
                piston.Velocity = 0.0f;
                state = PistonAssemblyState.ERROR;
            }
        }
    }
}
