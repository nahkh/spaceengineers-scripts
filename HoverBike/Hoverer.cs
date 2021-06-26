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
        public class Hoverer
        {
            private readonly IMyCockpit cockpit;
            private readonly List<IMyThrust> thrusters;
            private readonly IMyCameraBlock camera;
            private static readonly double desiredAltitude = 10;
            private double currentDesiredAltitude = 10;
            private readonly PidController pid;
            private DateTime landingStarted;
            private static readonly double landingSeconds = 10;
            private static readonly double maxVelocity = 100;
            private static readonly double scanRange = 100;
            private double lastProjectedAltitude;

            private DateTime lastScan;
            private Vector3D lastTarget;
            private bool lastScanHit;

            enum State
            {
                ACTIVE,
                DEACTIVATING,
                INACTIVE,
            }

            State state;

            public Hoverer(IMyCockpit cockpit, List<IMyThrust> thrusters, IMyCameraBlock camera) {
                this.cockpit = cockpit;
                this.thrusters = thrusters;
                this.camera = camera;
                state = State.INACTIVE;
                pid = new PidController(5, 0.1, 0.05, 10);
            }

            public void Update()
            {
                if (state == State.DEACTIVATING)
                {
                    double secondsSinceLandingStarted = (DateTime.Now - landingStarted).TotalSeconds;
                    if (secondsSinceLandingStarted > landingSeconds)
                    {
                        state = State.INACTIVE;
                        SetThrust(0f);
                    }
                    else
                    {
                        currentDesiredAltitude = desiredAltitude * (1 - (secondsSinceLandingStarted / landingSeconds));
                    }
                } else
                {
                    currentDesiredAltitude = desiredAltitude;
                }
                if (state != State.INACTIVE)
                {
                    if (IsUpsideDown())
                    {
                        SetThrust(0);
                    } else {
                        double currentAltitude = CurrentAltitude();
                        if (currentAltitude < currentDesiredAltitude / 2 || (GetVerticalVelocity() < -10 && currentAltitude < 100))
                        {
                            SetThrust(1);
                            pid.Reset();
                        } else
                        {
                            double pidInput = GetDesiredVelocity(currentAltitude) - GetVerticalVelocity();
                            double output = pid.Signal(pidInput);
                            output = Constrain(output, 0, 1);
                            SetThrust((float)output);
                        }
                    }
                }
            }

            public void Activate()
            {
                if (state == State.ACTIVE)
                {
                    return;
                }
                currentDesiredAltitude = desiredAltitude;
                camera.EnableRaycast = true;
                pid.Reset();
                state = State.ACTIVE;
            }

            public void Deactivate()
            {
                if (state == State.ACTIVE)
                {
                    state = State.DEACTIVATING;
                    landingStarted = DateTime.Now;
                    camera.EnableRaycast = false;
                }
            }

            public string Info()
            {
                return state.ToString();
            }

            public string AltitudeInfo()
            {
                return CurrentAltitude().ToString("n2");
            }

            public string DesiredAltitude()
            {
                return currentDesiredAltitude.ToString("n2");
            }

            public string ProjectedAltitudeInfo()
            {
                return lastProjectedAltitude.ToString("n2");
            }

            public string ScanInfo()
            {
                return lastScan.ToString("HH:mm:ss");
            }

            public string DebugInfo()
            {
                return pid.DebugInfo();
            }

            public string LastTargetInfo()
            {
                return "GPS:HIT:" + Math.Floor(lastTarget.X) + ":" + Math.Floor(lastTarget.Y) + ":" + Math.Floor(lastTarget.Z) + ":#FF75C9F1:  " + lastScanHit;
            }

            private double Constrain(double input, double min, double max)
            {
                if (input < min)
                {
                    return min;
                }
                if (input > max)
                {
                    return max;
                }
                return input;
            }

            private double CurrentAltitude()
            {
                double elevation;
                cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out elevation);
                double projectedAltitude = ProjectedAltitude();
                lastProjectedAltitude = projectedAltitude;
                return Math.Min(projectedAltitude, elevation);
            }

            private double ProjectedAltitude()
            {
                Vector3D gravity = cockpit.GetNaturalGravity();
                gravity.Normalize();
                float maxAngle = camera.RaycastConeLimit;
                
                lastScanHit = false;
                if (camera.CanScan(scanRange))
                {

                    float angle = -maxAngle * (1 - (float)(GetForwardVelocity() / maxVelocity));
                    if (angle > -0.1)
                    {
                        angle = 0.1f;
                    }
                    MyDetectedEntityInfo info = camera.Raycast(scanRange, angle);
                    if (!info.IsEmpty() && (info.Type == MyDetectedEntityType.LargeGrid || info.Type == MyDetectedEntityType.Planet))
                    {
                        lastScan = DateTime.Now;
                        lastScanHit = true;
                        lastTarget = info.HitPosition.Value;
                        Vector3D hitPosition = info.HitPosition.Value - cockpit.WorldMatrix.Translation;
                        
                        return Vector3D.Dot(hitPosition, gravity);
                    }
                }
                if (DateTime.Now - lastScan < TimeSpan.FromSeconds(1)) {
                    Vector3D hitPosition = lastTarget - cockpit.WorldMatrix.Translation;

                    return Vector3D.Dot(hitPosition, gravity);
                } else
                {
                    return 999;
                }
            }

            private void SetThrust(float thrust)
            {
                thrusters.ForEach(thruster => thruster.ThrustOverridePercentage = thrust);
            }

            private double GetDesiredVelocity(double currentAltitude)
            {
                return currentDesiredAltitude - currentAltitude;
            }

            private double GetVerticalVelocity()
            {
                Vector3D velocity = cockpit.GetShipVelocities().LinearVelocity;
                Vector3D gravity = cockpit.GetNaturalGravity();
                gravity.Normalize();
                Vector3D projection = Vector3D.ProjectOnVector(ref velocity, ref gravity);
                double sign = (Vector3D.Dot(gravity, velocity) > 0) ? -1 : 1;
                return sign * projection.Length();
            }

            private bool IsUpsideDown()
            {
                Vector3D gravity = cockpit.GetNaturalGravity();
                return Vector3D.Dot(gravity, cockpit.WorldMatrix.Up) > 0;
            }

            private double GetForwardVelocity()
            {
                Vector3D velocity = cockpit.GetShipVelocities().LinearVelocity;
                return Vector3D.Dot(velocity, cockpit.WorldMatrix.Forward);
            }


            public bool IsInactive()
            {
                return state == State.INACTIVE;
            }
        }
    }
}
