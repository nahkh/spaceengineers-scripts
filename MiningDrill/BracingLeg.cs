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
        public class BracingLeg
        {
            private static readonly float epsilon = 0.01f;
            private readonly IMyLandingGear landingGear;
            private readonly IMyPistonBase piston;

            public enum State
            {
                STANDBY,
                CONNECTING,
                CONNECTED,
                DISCONNECTING,
            }

            private State state;

            public BracingLeg(IMyLandingGear landingGear, IMyPistonBase piston)
            {
                this.landingGear = landingGear;
                this.piston = piston;
                if (landingGear.IsLocked)
                {
                    state = State.CONNECTED;
                    this.piston.Velocity = 0.0f;
                } else if (Math.Abs(this.piston.CurrentPosition - this.piston.MinLimit) < epsilon)
                {
                    this.piston.Velocity = 0.0f;
                    state = State.STANDBY;
                } else if (this.piston.Velocity > 0)
                {
                    state = State.CONNECTING;
                } else
                {
                    state = State.DISCONNECTING;
                }
            }

            public void Connect()
            {
                if (state == State.STANDBY)
                {
                    state = State.CONNECTING;
                    piston.Velocity = 0.5f;
                }
            }

            public void Disconnect()
            {
                if (state == State.CONNECTED || state == State.CONNECTING)
                {
                    state = State.DISCONNECTING;
                    landingGear.Unlock();
                    piston.Velocity = -0.5f;
                }
            }

            public void Update()
            {
                switch(state)
                {
                    case State.CONNECTED:
                    case State.STANDBY:
                        // Nothing to do
                        break;
                    case State.CONNECTING:
                        if (landingGear.IsLocked)
                        {
                            piston.Velocity = 0.0f;
                            state = State.CONNECTED;
                        }
                        break;
                    case State.DISCONNECTING:
                        if (Math.Abs(this.piston.CurrentPosition - this.piston.MinLimit) < epsilon)
                        {
                            state = State.STANDBY;
                            piston.Velocity = 0.0f;
                        }
                        break;
                }
            }

            public string Info()
            {
                return state.ToString();
            }
        }
    }
}
