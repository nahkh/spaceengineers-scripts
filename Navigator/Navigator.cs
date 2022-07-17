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
        public class Navigator
        {
            private static Vector3D NORTH = Vector3D.UnitZ;
            private readonly IMyCockpit orientingCockpit;
            
            private Vector3D navigationTarget;
            private bool navigatingToTarget;
            
            public Navigator(IMyCockpit orientingCockpit)
            {
                this.orientingCockpit = orientingCockpit;
                navigatingToTarget = false;
            }

            public void NavigateTo(Vector3D target)
            {
                navigationTarget = target;
                navigatingToTarget = true;
            }

            public bool Navigating
            {
                get
                {
                    return navigatingToTarget;
                }
            }

            public void ClearTarget()
            {
                navigatingToTarget = false;
                navigationTarget = Vector3D.Zero;
            }

            // Current compass heading
            public double CompassHeading()
            {
                Vector3D planetPosition;
                if (!orientingCockpit.TryGetPlanetPosition(out planetPosition))
                {
                    return 0;
                }
                Vector3D gravity = orientingCockpit.GetNaturalGravity();
                Vector3D position = orientingCockpit.WorldMatrix.Translation;
                Vector3D facing = orientingCockpit.WorldMatrix.Forward;
                Vector3D right = orientingCockpit.WorldMatrix.Right;
                Vector3D facingAlongSurfaceNormal = Vector3D.ProjectOnPlane(ref facing, ref gravity);
                Vector3D rightAlongSurfaceNormal = Vector3D.ProjectOnPlane(ref right, ref gravity);
                Vector3D localNorth = Vector3D.ProjectOnPlane(ref NORTH, ref gravity);
                facingAlongSurfaceNormal.Normalize();
                localNorth.Normalize();
                rightAlongSurfaceNormal.Normalize();
                double angle = Math.Acos(localNorth.Dot(facingAlongSurfaceNormal)) * 180 / Math.PI;
                if (localNorth.Dot(rightAlongSurfaceNormal) > 0)
                {
                    return 360 - angle;
                } else
                {
                    return angle;
                }
            }

            // Bearing to target
            public double TargetBearing()
            {
                if (!navigatingToTarget)
                {
                    return 0;
                }
                Vector3D planetPosition;
                if (!orientingCockpit.TryGetPlanetPosition(out planetPosition))
                {
                    return 0;
                }
                Vector3D fromPlanetToMe = orientingCockpit.WorldMatrix.Translation - planetPosition;
                Vector3D fromPlanetToTarget = navigationTarget - planetPosition;
                Vector3D normalOfGreatCircle = Vector3D.Cross(fromPlanetToTarget, fromPlanetToMe);
                normalOfGreatCircle.Normalize();
                Vector3D gravity = orientingCockpit.GetNaturalGravity();
                gravity.Normalize();
                Vector3D directionAlongSurfaceNormal = -Vector3D.Cross(gravity, normalOfGreatCircle);
                Vector3D localNorth = Vector3D.ProjectOnPlane(ref NORTH, ref gravity);
                localNorth.Normalize();
                double angle = Math.Acos(localNorth.Dot(directionAlongSurfaceNormal)) * 180 / Math.PI;
                if (localNorth.Dot(normalOfGreatCircle) > 0)
                {
                    return 360 - angle;
                }
                else
                {
                    return angle;
                }
            }

            // Distance to target
            public double TargetDistance()
            {
                if (!navigatingToTarget)
                {
                    return 0;
                }
                Vector3D planetPosition;
                if (!orientingCockpit.TryGetPlanetPosition(out planetPosition))
                {
                    return 0;
                }
                Vector3D fromPlanetToMe = orientingCockpit.WorldMatrix.Translation - planetPosition;
                Vector3D fromPlanetToTarget = navigationTarget - planetPosition;
                double averageAltitude = (fromPlanetToMe.Length() + fromPlanetToTarget.Length()) / 2f;
                fromPlanetToMe.Normalize();
                fromPlanetToTarget.Normalize();
                double angle = Math.Acos(fromPlanetToMe.Dot(fromPlanetToTarget));
                double distanceAlongGreatArc = angle * averageAltitude;
                double directDistance = (navigationTarget - orientingCockpit.WorldMatrix.Translation).Length();
                return Math.Max(distanceAlongGreatArc, directDistance);
            }

            public static double RelativeBearing(double heading, double bearing)
            {
                double relativeDistance = bearing - heading;
                if (relativeDistance < -180)
                {
                    relativeDistance = relativeDistance + 360;
                }
                if (relativeDistance > 180)
                {
                    relativeDistance = relativeDistance - 360;
                }
                return relativeDistance;
            }
           
        }
    }
}
