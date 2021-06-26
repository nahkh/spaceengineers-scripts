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
        public class Balancer
        {
            private readonly IMyGyro gyro;
            private readonly IMyCockpit cockpit;

            enum State
            {
                ACTIVE,
                DEACTIVATING,
                INACTIVE,
            }
            State state;

            public Balancer(IMyGyro gyro, IMyCockpit cockpit)
            {
                this.gyro = gyro;
                this.cockpit = cockpit;
                state = this.gyro.GyroOverride ? State.ACTIVE : State.INACTIVE;
            }

            public void Activate()
            {
                if (state == State.INACTIVE)
                {
                    gyro.GyroOverride = true;
                    gyro.GyroPower = 1f;
                    gyro.Yaw = 0f;
                    gyro.Pitch = 0f;
                    gyro.Roll = 0f;
                    state = State.ACTIVE;
                }
            }

            public void Deactivate()
            {
                if (state == State.ACTIVE)
                {
                    state = State.DEACTIVATING;
                }
            }

            public void Shutdown()
            {
                if (state == State.DEACTIVATING)
                {
                    gyro.GyroOverride = false;
                    gyro.GyroPower = 1f;
                    gyro.Yaw = 0f;
                    gyro.Pitch = 0f;
                    gyro.Roll = 0f;
                    state = State.INACTIVE;
                }
            }

            public void Update()
            {
                if (state != State.INACTIVE)
                {
                    
                    float desiredRoll = DesiredRoll();
                    
                    gyro.Roll = 2 * (desiredRoll - (float)CalculateRollDeviation());

                    float desiredPitch = DesiredPitch();
                    gyro.Pitch = desiredPitch - (float)CalculatePitchDeviation();
                    if (IsTraveling())
                    {
                        double yawDeviation = CalculateYawDeviation();
                        if (yawDeviation > Math.PI / 2)
                        {
                            yawDeviation -= Math.PI;
                        }
                        if (yawDeviation < -Math.PI / 2)
                        {
                            yawDeviation += Math.PI;
                        }
                        gyro.Yaw = -(float)(2 * yawDeviation);
                    }
                }
            }

            private float DesiredRoll()
            {
                float desiredRoll = 0;
                if (state == State.ACTIVE)
                {
                    Vector3 moveIndicator = cockpit.MoveIndicator;
                    if (moveIndicator.X > 0.1 || moveIndicator.X < -0.1)
                    {
                        desiredRoll = (float)(moveIndicator.X > 0 ? (Math.PI / 4) : (-Math.PI / 4));
                    }
                } else if (state == State.DEACTIVATING)
                {
                    double velocity = GetLateralVelocity();
                    if (velocity > 0.1)
                    {
                        desiredRoll = -0.1f;
                    } else if (velocity < -0.1)
                    {
                        desiredRoll = 0.1f;
                    }
                }
                return desiredRoll;
            }

            private float DesiredPitch()
            {
                float desiredPitch = 0;
                if (state == State.ACTIVE)
                {
                    Vector3 moveIndicator = cockpit.MoveIndicator;
                    if (moveIndicator.Z > 0.1)
                    {
                        desiredPitch = (float)(Math.PI / 8);
                    }
                } else if (state == State.DEACTIVATING)
                {
                    double velocity = GetForwardVelocity();
                    if (velocity > 0.1)
                    {
                        desiredPitch = 0.1f;
                    }
                    if (velocity < -0.1)
                    {
                        desiredPitch = -0.1f;
                    }
                }
                
                return desiredPitch;
            }

            public string Info()
            {
                return state.ToString();
            }

            public string RollDeviation()
            {
                return CalculateRollDeviation().ToString("n2");
            }

            public string YawDeviation()
            {
                return CalculateYawDeviation().ToString("n2");
            }

            public string PitchDeviation()
            {
                return CalculatePitchDeviation().ToString("n2");
            }

            private double CalculateRollDeviation() { 
                Vector3D gravity = cockpit.GetNaturalGravity();
                Vector3D forward = cockpit.WorldMatrix.Forward;
                Vector3D projection = Vector3D.ProjectOnPlane(ref gravity, ref forward);
                projection.Normalize();
                double sign = Vector3D.Dot(gravity, cockpit.WorldMatrix.Right) > 0 ? 1 : -1;
                return sign * Math.Acos(Vector3D.Dot(projection, cockpit.WorldMatrix.Down));
            }

            private double CalculatePitchDeviation()
            {
                Vector3D gravity = cockpit.GetNaturalGravity();
                Vector3D right = cockpit.WorldMatrix.Right;
                Vector3D projection = Vector3D.ProjectOnPlane(ref gravity, ref right);
                projection.Normalize();
                double sign = Vector3D.Dot(gravity, cockpit.WorldMatrix.Forward) < 0 ? 1 : -1;
                return sign * Math.Acos(Vector3D.Dot(projection, cockpit.WorldMatrix.Down));
            }

            private double CalculateYawDeviation()
            {
                Vector3D velocity = cockpit.GetShipVelocities().LinearVelocity;
                Vector3D up = cockpit.WorldMatrix.Up;
                Vector3D projection = Vector3D.ProjectOnPlane(ref velocity, ref up);
                projection.Normalize();
                double sign = Vector3D.Dot(velocity, cockpit.WorldMatrix.Right) > 0 ? 1 : -1;
                return sign * Math.Acos(Vector3D.Dot(projection, cockpit.WorldMatrix.Forward));
            }

            private bool IsTraveling()
            {
                Vector3D velocity = cockpit.GetShipVelocities().LinearVelocity;
                return Vector3D.Dot(velocity, cockpit.WorldMatrix.Forward) > 2.0;
            }

            private double GetForwardVelocity()
            {
                Vector3D velocity = cockpit.GetShipVelocities().LinearVelocity;
                return Vector3D.Dot(velocity, cockpit.WorldMatrix.Forward);
            }

            private double GetLateralVelocity()
            {
                Vector3D velocity = cockpit.GetShipVelocities().LinearVelocity;
                return Vector3D.Dot(velocity, cockpit.WorldMatrix.Right);
            }
        }
    }
}
