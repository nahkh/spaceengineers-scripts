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
        public class NavigatorDisplay
        {
            private readonly IMyTextSurface textSurface;
            private readonly Navigator navigator;
            private readonly Vector2 textureSize;
            private readonly double maxDeviation;
            private MySpriteDrawFrame drawFrame;
            private readonly MySprite middleIndicator;
            private List<IndicatorSprite> indicatorSprites;
            private MySprite targetSprite;
            private MySprite leftIndicator;
            private MySprite rightIndicator;

            public NavigatorDisplay(IMyTextSurface textSurface, Navigator navigator, double maxDeviation = 45)
            {
                this.textSurface = textSurface;
                this.navigator = navigator;
                this.maxDeviation = maxDeviation;
                textSurface.ContentType = ContentType.SCRIPT;
                textureSize = textSurface.TextureSize;
                middleIndicator = MySprite.CreateSprite("Triangle", new Vector2(textureSize.X / 2, textureSize.Y * 0.5f), new Vector2(textureSize.X / 10, textureSize.Y / 10));
                indicatorSprites = new List<IndicatorSprite>();
                string[] compassPoints = new string[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
                for (int i = 0; i < 16; ++i)
                {
                    float height = textureSize.Y / 30f;
                    if (i % 4 == 0)
                    {
                        height *= 2f;
                    }
                    else if (i % 2 == 0)
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
                    indicatorSprites.Add(new IndicatorSprite(sprite, i * (360f / 16f), maxDeviation));
                }
                rightIndicator = MySprite.CreateSprite("AH_BoreSight", new Vector2(textureSize.X * 0.75f, textureSize.Y * 0.5f), new Vector2(textureSize.X * 0.1f, textureSize.Y * 0.1f));
                leftIndicator = MySprite.CreateSprite("AH_BoreSight", new Vector2(textureSize.X * 0.25f, textureSize.Y * 0.5f), new Vector2(textureSize.X * 0.1f, textureSize.Y * 0.1f));
                leftIndicator.RotationOrScale = (float)Math.PI;
                targetSprite = MySprite.CreateSprite("AH_BoreSight", new Vector2(textureSize.X * 0.25f, textureSize.Y * 0.4f), new Vector2(textureSize.X * 0.1f, textureSize.Y * 0.1f));
                targetSprite.RotationOrScale = (float)Math.PI * 1.5f;
            }

            public void Update()
            {
                drawFrame = textSurface.DrawFrame();
                double heading = navigator.CompassHeading();
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
                if (navigator.Navigating)
                {
                    double headingToTarget = navigator.TargetBearing();
                    double distanceToTarget = navigator.TargetDistance();
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
                navigator.NavigateTo(target);
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

            public class IndicatorSprite
            {
                private MySprite sprite;
                private readonly double compassPosition;
                private readonly double maxDeviation;

                public IndicatorSprite(MySprite sprite, double compassPosition, double maxDeviation)
                {
                    this.sprite = sprite;
                    this.compassPosition = compassPosition;
                    this.maxDeviation = maxDeviation;
                    double unseen = 360 - (maxDeviation * 2);
                }

                public bool IsVisibleWhenFacing(double facing)
                {
                    return Math.Abs(Navigator.RelativeBearing(compassPosition, facing)) < maxDeviation;
                }

                public bool IsLeftWhenFacing(double facing)
                {
                    return Navigator.RelativeBearing(compassPosition, facing) < 0;
                }
                public bool IsRightWhenFacing(double facing)
                {
                    return Navigator.RelativeBearing(compassPosition, facing) > 0;
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
