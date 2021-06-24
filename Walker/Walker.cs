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
        public class Walker
        {
            private readonly float velocity;
            private readonly PistonAssembly a;
            private readonly PistonAssembly b;
            private readonly IMyPistonBase extender;
            private static readonly float epsilon = 0.01f;

            public enum State
            {
                ERROR,
                STANDBY,
                MOVING_FORWARD,
                MOVING_BACKWARD,
            }

            State state;

            Func<bool>[] moveForwardAnimation;
            Func<bool>[] moveBackwardAnimation;
            int animationSequence;

            public Walker(float velocity, PistonAssembly a, PistonAssembly b, IMyPistonBase extender)
            {
                this.velocity = velocity;
                this.a = a;
                this.b = b;
                this.extender = extender;
                if (a.State == PistonAssembly.PistonAssemblyState.ERROR || b.State == PistonAssembly.PistonAssemblyState.ERROR || Math.Abs(extender.CurrentPosition - extender.MinLimit) > epsilon)
                {
                    state = State.ERROR;
                } else
                {
                    state = State.STANDBY;
                }

                extender.Velocity = 0.0f;
                moveForwardAnimation = new Func<bool>[] {
                    () => {
                        a.Disconnect();
                        return true;
                    },
                    () => a.State == PistonAssembly.PistonAssemblyState.DISCONNECTED,
                    () => {
                        extender.Velocity = velocity;
                        return true;
                    },
                    () => Math.Abs(extender.CurrentPosition - extender.MaxLimit) < epsilon,
                    () => {
                        a.Connect();
                        return true;
                    },
                    () => a.State == PistonAssembly.PistonAssemblyState.CONNECTED,
                    () => {
                        b.Disconnect();
                        return true;
                    },
                    () => b.State == PistonAssembly.PistonAssemblyState.DISCONNECTED,
                    () => {
                        extender.Velocity = -velocity;
                        return true;
                    },
                    () => Math.Abs(extender.CurrentPosition - extender.MinLimit) < epsilon,
                    () =>
                    {
                        b.Connect();
                        return true;
                    },
                    () => b.State == PistonAssembly.PistonAssemblyState.CONNECTED,
                    () =>
                    {
                        state = State.STANDBY;
                        return true;
                    }
                };
                moveBackwardAnimation = new Func<bool>[] {
                    () => {
                        b.Disconnect();
                        return true;
                    },
                    () => b.State == PistonAssembly.PistonAssemblyState.DISCONNECTED,
                    () => {
                        extender.Velocity = velocity;
                        return true;
                    },
                    () => Math.Abs(extender.CurrentPosition - extender.MaxLimit) < epsilon,
                    () => {
                        b.Connect();
                        return true;
                    },
                    () => b.State == PistonAssembly.PistonAssemblyState.CONNECTED,
                    () => {
                        a.Disconnect();
                        return true;
                    },
                    () => a.State == PistonAssembly.PistonAssemblyState.DISCONNECTED,
                    () => {
                        extender.Velocity = -velocity;
                        return true;
                    },
                    () => Math.Abs(extender.CurrentPosition - extender.MinLimit) < epsilon,
                    () =>
                    {
                        a.Connect();
                        return true;
                    },
                    () => a.State == PistonAssembly.PistonAssemblyState.CONNECTED,
                    () =>
                    {
                        state = State.STANDBY;
                        return true;
                    }
                };
            }

            public void Update()
            {
                a.Update();
                b.Update();
                switch (state)
                {
                    case State.STANDBY:
                    case State.ERROR:
                        // Do nothing
                        break;
                    case State.MOVING_BACKWARD:
                        if (moveBackwardAnimation[animationSequence]())
                        {
                            ++animationSequence;
                        }
                        break;
                    case State.MOVING_FORWARD:
                        if (moveForwardAnimation[animationSequence]())
                        {
                            ++animationSequence;
                        }
                        break;
                }
            }

            public void StepForward()
            {
                if (state == State.STANDBY)
                {
                    state = State.MOVING_FORWARD;
                    animationSequence = 0;
                }
            }

            public void StepBackward()
            {
                if (state == State.STANDBY) {
                    state = State.MOVING_BACKWARD;
                    animationSequence = 0;
                }
            }

            public void EmergencyShutdown()
            {
                a.EmergencyShutdown();
                b.EmergencyShutdown();
                extender.Velocity = 0.0f;
                state = State.ERROR;
            }

            public List<string> StatusInfo()
            {
                List<string> output = new List<string>();

                output.Add("A " + a.State.ToString());
                output.Add("B " + b.State.ToString());
                output.Add("E " + extender.CurrentPosition.ToString());
                output.Add("----");
                output.Add("S " + state.ToString());
                return output;
            }

            public State GetState()
            {
                return state;
            }
        }
    }
}
