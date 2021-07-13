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
        public class SquareWalker
        {
            private bool dirty;
            private int x, y;
            private readonly int w;
            private readonly int h;
            private readonly IMyTextSurface infoSurface;
            private readonly IMyTextSurface renderSurface;
            private readonly Renderer renderer;

            public SquareWalker(int w, int h, IMyTextSurface infoSurface, IMyTextSurface renderSurface, Action<string> logger)
            {
                dirty = true;
                if (w <= 0)
                {
                    w = 1;
                }
                if (h <= 0)
                {
                    h = 1;
                }
                this.w = w;
                this.h = h;
                this.infoSurface = infoSurface;
                this.renderSurface = renderSurface;
                renderer = new Renderer(renderSurface, w, h, Color.Black, logger);
                x = 0;
                y = 0;
            }

            public void Left()
            {
                x--;
                if (x < 0)
                {
                    x = w - 1;
                }
                dirty = true;
            }

            public void Right()
            {
                x++;
                if (x >= w)
                {
                    x = 0;
                }
                dirty = true;
            }

            public void Up()
            {
                y--;
                if (y < 0)
                {
                    y = h - 1;
                }
                dirty = true;
            }

            public void Down()
            {
                y++;
                if (y >= h)
                {
                    y = 0;
                }
                dirty = true;
            }

            public void Update()
            {
                if (dirty)
                {
                    dirty = false;
                    infoSurface.WriteText("x = " + x + "\ny = " + y);
                    renderer.StartDraw();
                    renderer.DrawLine(0, 0, w - 1, 0, Color.Blue);
                    renderer.DrawLine(w - 1, 0, w - 1, h - 1, Color.Blue);
                    renderer.DrawLine(w - 1, h - 1, 0, h - 1, Color.Blue);
                    renderer.DrawLine(0, h - 1, 0, 0, Color.Blue);
                    renderer.DrawRect(1, 1, w - 2, h - 2, Color.Red);
                    //renderer.DrawLine(x, y, w / 2, h / 2, Color.Green);
                    renderer.SetPixel(x, y, new Color(0xFFFFFFFF));
                    renderer.Flush();
                }
            }
        }
    }
}
