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
        public class RotorController : ArmComponent
        {
            private readonly IMyMotorAdvancedStator stator;
            private readonly float idlePosition;
            private readonly float stowedPosition;
            private readonly float maxRadians;
            private readonly float maxAcceleration;
            private readonly float idleDelta;
            private readonly float maxRadiansPerTick;
            private bool desiredAngleSet;
            private float desiredAngle;
            private bool desiredVelocitySet;
            private float desiredVelocity;

            private float previousAngle;
            private DateTime previousTickTime;

            public RotorController(IMyMotorAdvancedStator stator, float idlePosition, float stowedPosition, float maxRadians, float maxAcceleration, float idleDelta)
            {
                if (stator == null)
                {
                    throw new ArgumentNullException(nameof(stator));
                }

                this.stator = stator;
                this.idlePosition = idlePosition;
                this.stowedPosition = stowedPosition;
                this.maxRadians = maxRadians;
                this.maxAcceleration = maxAcceleration;
                this.idleDelta = idleDelta;
                maxRadiansPerTick = maxRadians / 20f;
                desiredAngleSet = false;
                desiredAngle = 0f;
                previousAngle = stator.Angle;
                previousTickTime = DateTime.Now;
            }

            public void setDesiredAngle(float desiredAngle)
            {
                this.desiredAngle = desiredAngle;
                desiredAngleSet = true;
                desiredVelocitySet = false;
            }
            public void setDesiredVelocity(float desiredVelocity)
            {
                this.desiredVelocity = desiredVelocity;
                desiredVelocitySet = true;
                desiredAngleSet = false;
            }

            void ArmComponent.emergencyStop()
            {
                stator.TargetVelocityRad = 0;
            }

            ErrorReport ErrorReporter.getError()
            {
                if (!stator.IsAttached)
                {
                    return ErrorReport.critical(stator.CustomName + " is not attached");
                }

                if (Math.Abs(stator.TargetVelocityRad) > maxRadians)
                {
                    return ErrorReport.critical(stator.CustomName + " is going too fast");
                }

                if (Math.Abs(stator.TargetVelocityRad) > 0.9 * maxRadians)
                {
                    return ErrorReport.warning(stator.CustomName + " is near speed limit");
                }

                return ErrorReport.NONE;
            }

            private string formatAngle(float radians)
            {
                return (180 * radians / Math.PI).ToString("n2");
            }

            private float getAngularDistance(float target)
            {
                float distance = target - stator.Angle;
                if (distance < -Math.PI)
                {
                    distance += (float)Math.PI * 2;
                }
                if (distance > Math.PI)
                {
                    distance -= (float)Math.PI * 2;
                }
                return distance;
            }

            string ArmComponent.getStatus()
            {
                return formatAngle(stator.Angle) + (desiredAngleSet ? " S " : " I ") + formatAngle(desiredAngle) + " v " + formatAngle(stator.TargetVelocityRad);
            }

            bool ArmComponent.isIdle()
            {
                return Math.Abs(getAngularDistance(idlePosition)) < idleDelta && stator.TargetVelocityRad < idleDelta;
            }

            void ArmComponent.returnToIdle()
            {
                setDesiredAngle(idlePosition);
            }

            float getTargetVelocity()
            {
                if (desiredAngleSet)
                {
                    float distance = getAngularDistance(desiredAngle);

                    if (Math.Abs(distance) < idleDelta)
                    {
                        stator.TargetVelocityRad = 0;
                    }
                    return distance;
                } else if (desiredVelocitySet)
                {
                    return desiredVelocity;
                } else
                {
                    return 0;
                }
            }

            void ArmComponent.tick()
            {
                float targetVelocity = getTargetVelocity();

                if (Math.Abs(targetVelocity) > maxRadiansPerTick)
                {
                    targetVelocity = maxRadians * 0.8f * (targetVelocity < 0 ? -1 : 1);
                }

                float currentVelocity = stator.TargetVelocityRad;
                AngleDelta delta = GetAngleDelta();
                if (delta.reliable)
                {
                    currentVelocity = (float)(delta.radians / delta.timeSpan.TotalSeconds);
                }

                if (Math.Abs(targetVelocity - currentVelocity) > maxAcceleration)
                {
                    targetVelocity = (targetVelocity > 0) ? maxAcceleration : -maxAcceleration;
                }

                stator.TargetVelocityRad = targetVelocity;
            }

            void ArmComponent.stow()
            {
                setDesiredAngle(stowedPosition);
            }

            bool ArmComponent.isStowed()
            {
                return Math.Abs(getAngularDistance(stowedPosition)) < idleDelta && stator.TargetVelocityRad < idleDelta;
            }

            public float Angle
            {
                get {
                    return stator.Angle;
                }
            }

            public Vector3D Position
            {
                get
                {
                    return stator.GetPosition();
                }
            }

            public Vector3D Up
            {
                get
                {
                    return stator.WorldMatrix.Up;
                }
            }

            public Vector3D Forward
            {
                get
                {
                    return stator.WorldMatrix.Forward;
                }
            }

            public Vector3D Left
            {
                get
                {
                    return stator.WorldMatrix.Left;
                }
            }

            class AngleDelta
            {
                public readonly TimeSpan timeSpan;
                public readonly float radians;
                public bool reliable;

                public AngleDelta(TimeSpan timeSpan, float radians, bool reliable)
                {
                    this.timeSpan = timeSpan;
                    this.radians = radians;
                    this.reliable = reliable;
                }
            }

            private AngleDelta GetAngleDelta()
            {
                DateTime now = DateTime.Now;
                TimeSpan diff = now - previousTickTime;
                bool reliable = diff.TotalMilliseconds > 0.1 && diff.TotalSeconds < 1;
                AngleDelta delta = new AngleDelta(diff, -getAngularDistance(previousAngle), reliable);
                previousTickTime = now;
                previousAngle = Angle;
                return delta;                
            }

        }
    }
}
