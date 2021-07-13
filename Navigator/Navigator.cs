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
            private readonly IMyTextSurface textSurface;
            private readonly IMyCockpit orientingCockpit;
            private readonly Vector2 textureSize;
            private readonly double maxDeviation;
            private MySpriteDrawFrame drawFrame;
            private readonly MySprite middleIndicator;
            private List<IndicatorSprite> indicatorSprites;
            private MySprite targetSprite;
            private Vector3D navigationTarget;
            private bool navigatingToTarget;
            private MySprite leftIndicator;
            private MySprite rightIndicator;
            
            public Navigator(IMyTextSurface textSurface, IMyCockpit orientingCockpit, double maxDeviation = 45)
            {
                this.textSurface = textSurface;
                this.orientingCockpit = orientingCockpit;
                this.maxDeviation = maxDeviation;
                textSurface.ContentType = ContentType.SCRIPT;
                textureSize = textSurface.TextureSize;
                middleIndicator = MySprite.CreateSprite("Triangle", new Vector2(textureSize.X / 2, textureSize.Y * 0.5f), new Vector2(textureSize.X / 10, textureSize.Y / 10));
                indicatorSprites = new List<IndicatorSprite>();
                string[] compassPoints = new string[] {"N" , "NE", "E", "SE", "S", "SW", "W", "NW"};
                for (int i = 0; i < 16; ++i)
                {
                    float height = textureSize.Y / 30f;
                    if (i % 4 == 0)
                    {
                        height *= 2f;
                    } else if (i % 2 == 0)
                    {
                        height *= 1.5f;
                    }
                    if (i % 2 == 0)
                    {
                        MySprite labelSprite = MySprite.CreateText(compassPoints[i / 2], "Monospace", textSurface.FontColor, scale: 0.5f);
                        labelSprite.Position = new Vector2(0, textureSize.Y * 0.4f);
                        indicatorSprites.Add(new IndicatorSprite(labelSprite, i * (360f / 16f), maxDeviation));
                    }
                    MySprite sprite = MySprite.CreateSprite("SquareSimple", new Vector2(0, textureSize.Y * 0.35f), new Vector2(textureSize.X / 50, height));
                    indicatorSprites.Add(new IndicatorSprite(sprite, i * (360f/16f), maxDeviation));
                }
                rightIndicator = MySprite.CreateSprite("AH_BoreSight", new Vector2(textureSize.X * 0.75f, textureSize.Y * 0.5f), new Vector2(textureSize.X * 0.1f, textureSize.Y * 0.1f));
                leftIndicator = MySprite.CreateSprite("AH_BoreSight", new Vector2(textureSize.X * 0.25f, textureSize.Y * 0.5f), new Vector2(textureSize.X * 0.1f, textureSize.Y * 0.1f));
                leftIndicator.RotationOrScale = (float)Math.PI;
                targetSprite = MySprite.CreateSprite("AH_BoreSight", new Vector2(textureSize.X * 0.25f, textureSize.Y * 0.4f), new Vector2(textureSize.X * 0.1f, textureSize.Y * 0.1f));
                targetSprite.RotationOrScale = (float)Math.PI * 1.5f;
                navigatingToTarget = false;
            }

            public void Update()
            {
                drawFrame = textSurface.DrawFrame();
                double heading = CompassHeadingInDegrees();
                MySprite headingNumericSprite = MySprite.CreateText(((int)heading).ToString(), "Monospace", textSurface.FontColor, scale: 0.5f);
                headingNumericSprite.Position = new Vector2(textureSize.X / 2, textureSize.Y / 5);
                drawFrame.Add(middleIndicator);
                drawFrame.Add(headingNumericSprite);
                foreach (IndicatorSprite sprite in indicatorSprites)
                {
                    if (sprite.IsVisibleWhenFacing(heading))
                    {
                        drawFrame.Add(sprite.GetForFacing(heading, textureSize.X));
                    }
                }
                if (navigatingToTarget)
                {
                    double headingToTarget = HeadingToTarget();
                    double distanceToTarget = DistanceToTarget();
                    MySprite targetDistanceSprite = MySprite.CreateText(FormatDistance(distanceToTarget), "Monospace", textSurface.FontColor, scale: 0.5f);
                    targetDistanceSprite.Position = new Vector2(textureSize.X / 2, textureSize.Y * 0.7f);
                    drawFrame.Add(targetDistanceSprite);
                    
                    IndicatorSprite targetIndicator = new IndicatorSprite(targetSprite, headingToTarget, maxDeviation);
                    if (targetIndicator.IsVisibleWhenFacing(heading))
                    {
                        drawFrame.Add(targetIndicator.GetForFacing(heading, textureSize.X));
                    }
                    else
                    {
                        if (targetIndicator.IsLeftWhenFacing(heading))
                        {
                            drawFrame.Add(leftIndicator);
                        }
                        if (targetIndicator.IsRightWhenFacing(heading))
                        {
                            drawFrame.Add(rightIndicator);
                        }
                    }
                }

                drawFrame.Dispose();
            }

            public void NavigateTo(Vector3D target)
            {
                navigationTarget = target;
                navigatingToTarget = true;
            }

            private double CompassHeadingInDegrees()
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

            private string FormatDistance(double distance)
            {
                if (distance < 1000)
                {
                    return distance.ToString("n2") + " m";
                }
                distance /= 1000;
                return distance.ToString("n2") + " km";
            }

            private double HeadingToTarget()
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

            private double DistanceToTarget()
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


            public class IndicatorSprite
            {
                private MySprite sprite;
                private readonly double compassPosition;
                private readonly double maxDeviation;
                private readonly Vector2D[] ranges;

                public IndicatorSprite(MySprite sprite, double compassPosition, double maxDeviation)
                {
                    this.sprite = sprite;
                    this.compassPosition = compassPosition;
                    this.maxDeviation = maxDeviation;
                    ranges = ComputeRange(compassPosition, maxDeviation);
                    double unseen = 360 - (maxDeviation * 2);
                }

                private static Vector2D[] ComputeRange(double point, double maxDeviation)
                {
                    List<Vector2D> ranges = new List<Vector2D>();
                    double leftEdge = point - maxDeviation;
                    double rightEdge = point + maxDeviation;
                    if (leftEdge < 0)
                    {
                        ranges.Add(new Vector2D(leftEdge, 360));
                        leftEdge = 0;
                    }
                    if (rightEdge > 360)
                    {
                        ranges.Add(new Vector2D(0, rightEdge - 360));
                        rightEdge = 360;
                    }
                    ranges.Add(new Vector2D(leftEdge, rightEdge));
                    return ranges.ToArray();
                }

                private static double Constrain(double number)
                {
                    while (number < 0)
                    {
                        number += 360;
                    }
                    while (number >= 360)
                    {
                        number -= 360;
                    }
                    return number;
                }

                public bool IsVisibleWhenFacing(double facing)
                {
                    return Math.Abs(RelativeFacing(facing)) < maxDeviation;
                }

                public double RelativeFacing(double facing)
                {
                    double relativeDistance = compassPosition - facing;
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

                public bool IsLeftWhenFacing(double facing)
                {
                    return RelativeFacing(facing) < 0;
                }
                public bool IsRightWhenFacing(double facing)
                {
                    return RelativeFacing(facing) > 0;
                }

                public MySprite GetForFacing(double facing, double textureWidth)
                {
                    double leftEdge = facing - maxDeviation;
                    double rightEdge = facing + maxDeviation;
                    double effectivePosition = compassPosition;
                    if (effectivePosition > leftEdge + 360)
                    {
                        effectivePosition -= 360;
                    }
                    if (effectivePosition < rightEdge - 360)
                    {
                        effectivePosition += 360;
                    }
                    double fractionOfScreen = (effectivePosition - leftEdge) / (rightEdge - leftEdge);
                    sprite.Position = new Vector2((float)(textureWidth * fractionOfScreen), sprite.Position.Value.Y);
                    return sprite;
                }
            }
            
        }
    }
}
