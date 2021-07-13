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
        public class Renderer
        {
            private bool readyToDraw;
            private MySpriteDrawFrame drawFrame;
            private readonly Action<string> logger;
            private readonly IMyTextSurface surface;
            private readonly int width;
            private readonly int height;
            private readonly double xScale;
            private readonly double yScale;

            public Renderer(IMyTextSurface surface, int width, int height, Color backgroundColor, Action<string> logger = null)
            {
                this.logger = logger;
                this.surface = surface;
                this.width = width;
                this.height = height;
                Vector2 textureSize = surface.TextureSize;
                xScale = textureSize.X / width;
                yScale = textureSize.Y / height;
                surface.ContentType = ContentType.SCRIPT;
                surface.ScriptBackgroundColor = backgroundColor;
                readyToDraw = false;
            }

            public void StartDraw()
            {
                drawFrame = surface.DrawFrame();
                readyToDraw = true;
            }

            public void SetPixel(int x, int y, Color color)
            {
                if (!readyToDraw)
                {
                    logger?.Invoke("no frame");
                    return;
                }
                logger?.Invoke("Setting " + x + " " + y + " " + color);
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    logger?.Invoke("out of bounds");
                    return;
                }
                MySprite sprite = MySprite.CreateSprite("SquareSimple", new Vector2((float)(x * xScale + xScale/2), (float)(y * yScale + yScale / 2)), new Vector2((float)(xScale), (float)(yScale)));
                
                sprite.Color = color;
                drawFrame.Add(sprite);
            }

            public void DrawLine(int x1, int y1, int x2, int y2, Color color)
            {
                if (x1 == x2 && y1 == y2)
                {
                    SetPixel(x1, x2, color);
                    return;
                }
                int dX = Math.Abs(x2 - x1);
                int dY = Math.Abs(y2 - y1);
                if (dX > dY)
                {
                    if (x1 > x2)
                    {
                        int temp = x1;
                        x1 = x2;
                        x2 = temp;
                        temp = y1;
                        y1 = y2;
                        y2 = temp;
                    }
                    float slope = (y2 - y1) / (float)(x2 - x1);
                    float yPos = y1;
                    if (slope > 0)
                    {
                        yPos += slope / 2;
                    }
                    else
                    {
                        yPos -= slope / 2;
                    }
                    for (int i = x1; i <= x2; ++i)
                    {
                        SetPixel(i, (int)yPos, color);
                        yPos += slope;
                    }
                } else
                {
                    if (y1 > y2)
                    {
                        int temp = x1;
                        x1 = x2;
                        x2 = temp;
                        temp = y1;
                        y1 = y2;
                        y2 = temp;
                    }
                    float slope = (x2 - x1) / (float)(y2 - y1);
                    float xPos = x1;
                    if (slope > 0)
                    {
                        xPos += slope / 2;
                    } else
                    {
                        xPos -= slope / 2;
                    }
                    for (int i = y1; i <= y2; ++i)
                    {
                        SetPixel((int)xPos, i, color);
                        xPos += slope;
                    }
                }
            }

            public void DrawRect(int x, int y, int w, int h, Color color)
            {
                if (!readyToDraw)
                {
                    logger?.Invoke("no frame");
                    return;
                }
                logger?.Invoke("Setting " + x + " " + y + " " + color);
                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    logger?.Invoke("out of bounds");
                    return;
                }
                MySprite sprite = MySprite.CreateSprite("SquareSimple", new Vector2((float)((x + w/2f) * xScale), (float)((y + h/2f) * yScale)), new Vector2((float)(xScale * w), (float)(yScale * h)));
                sprite.Color = color;
                drawFrame.Add(sprite);
            }

            public void Flush()
            {
                drawFrame.Dispose();
                readyToDraw = false;
            }
        }
    }
}
