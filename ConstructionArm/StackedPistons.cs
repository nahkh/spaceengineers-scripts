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
        public class StackedPistons : ArmComponent
        {
            private static readonly float RETRACTED_LENGTH = 5f;
            private static readonly float EPSILON = 0.00001f;
            private float desiredLength;
            private float desiredMaxVelocity;
            private float maxAcceleration = 0.5f;
            private readonly List<IMyPistonBase> pistons;
            private readonly Action<string> logger;
            private readonly float minLength;
            private readonly float maxLength;
                

            public StackedPistons(string label, List<IMyPistonBase> pistons, Action<string> logger)
            {
                if (string.IsNullOrEmpty(label))
                {
                    throw new ArgumentException($"'{nameof(label)}' cannot be null or empty.", nameof(label));
                }

                if (pistons == null)
                {
                    throw new ArgumentNullException(nameof(pistons));
                }

                if (logger == null)
                {
                    throw new ArgumentNullException(nameof(logger));
                }

                this.pistons = pistons;
                this.logger = logger;
                desiredLength = 0;
                desiredMaxVelocity = 0;
                maxAcceleration = 1f;
                float minLength = 0;
                float maxLength = 0;
                int index = 1;
                foreach(IMyPistonBase piston in pistons)
                {
                    piston.Velocity = 0;
                    desiredLength += piston.CurrentPosition + RETRACTED_LENGTH;
                    minLength += RETRACTED_LENGTH + piston.MinLimit;
                    maxLength += RETRACTED_LENGTH + piston.MaxLimit;
                    piston.CustomName = label + " " + index++ + " / " + pistons.Count;
                }
                this.minLength = minLength;
                this.maxLength = maxLength;
            }

            public float Length
            {
                get
                {
                    float length = 0;
                    foreach(IMyPistonBase piston in pistons)
                    {
                        length += RETRACTED_LENGTH + piston.CurrentPosition;
                    }
                    return length;
                }
            }
            public float Velocity
            {
                get
                {
                    float velocity = 0;
                    foreach (IMyPistonBase piston in pistons)
                    {
                        velocity += piston.Velocity;
                    }
                    return velocity;
                }

                set
                {
                    if (Math.Abs(value) < EPSILON)
                    {
                        foreach (IMyPistonBase piston in pistons)
                        {
                            piston.Velocity = 0;
                        }
                        return;
                    }
                    List<IMyPistonBase> availablePistons = new List<IMyPistonBase>();
                    if (value < 0)
                    {
                        foreach (IMyPistonBase piston in pistons)
                        {
                            if (Math.Abs(piston.CurrentPosition - piston.MinLimit) > EPSILON)
                            {
                                availablePistons.Add(piston);
                            }
                            else
                            {
                                piston.Velocity = 0;
                            }
                        }
                    }
                    else 
                    {
                        foreach (IMyPistonBase piston in pistons)
                        {
                            if (Math.Abs(piston.CurrentPosition - piston.MaxLimit) > EPSILON)
                            {
                                availablePistons.Add(piston);
                            } else
                            {
                                piston.Velocity = 0;
                            }
                        }
                    }
                    if (availablePistons.Count > 0)
                    {
                        foreach (IMyPistonBase piston in availablePistons)
                        {
                            piston.Velocity = value / availablePistons.Count;
                        }
                    }
                }
            }

            public void setDesiredState(float desiredLength, float desiredMaxVelocity)
            {
                logger.Invoke("DL " + desiredLength.ToString("n2"));
                if (desiredLength > maxLength)
                {
                    desiredLength = maxLength;
                }
                if (desiredLength < minLength)
                {
                    desiredLength = minLength;
                }
                this.desiredLength = desiredLength;
                this.desiredMaxVelocity = desiredMaxVelocity;
            }

            public void tick()
            {
                float desiredLengthChange = desiredLength - Length;
                if (Math.Abs(desiredLengthChange) < EPSILON)
                {
                    Velocity = 0f;
                    return;
                }

                float targetVelocity = Velocity;
                if (desiredLengthChange < 0)
                {
                    targetVelocity -= maxAcceleration;
                } else
                {
                    targetVelocity += maxAcceleration;
                }
                
                // If we're faster than max desired, slow down
                if (Math.Abs(targetVelocity) > desiredMaxVelocity)
                {
                    logger.Invoke("Faster than max, limiting");
                    if (targetVelocity > 0)
                    {
                        targetVelocity = desiredMaxVelocity;
                    } else
                    {
                        targetVelocity = -desiredMaxVelocity;
                    }
                }

                // If we're approaching the desired position, slow down
                if (Math.Abs(desiredLengthChange) < targetVelocity)
                {
                    logger.Invoke("Approaching desired position");
                    targetVelocity = desiredLengthChange;
                }
                Velocity = targetVelocity;
            }

            public void emergencyStop()
            {
                Velocity = 0;
            }

            public void returnToIdle()
            {
                logger.Invoke("RTI");
                desiredLength = minLength;
            }

            public bool isIdle()
            {
                return Math.Abs(Length - minLength) < EPSILON;
            }

            public void stow()
            {
                logger.Invoke("STOW");
                desiredLength = minLength;
            }

            public bool isStowed()
            {
                return Math.Abs(Length - minLength) < EPSILON;
            }

            public string getStatus()
            {
                return Length.ToString("n2") + " " + desiredLength.ToString("n2") + " " + Velocity.ToString("n2");
            }

            public ErrorReport getError()
            {
                return ErrorReport.NONE;
            }
        }
    }
}
