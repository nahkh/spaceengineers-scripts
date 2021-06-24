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
        public class StatusDisplay
        {
            private readonly List<string> output;
            private readonly List<Row> rows;
            private readonly IMyTextSurface textSurface;
            private readonly int width;
            private readonly int height;
            private bool showTime = false;

            public StatusDisplay(IMyTextSurface textSurface, int width, int height)
            {
                rows = new List<Row>();
                this.textSurface = textSurface;
                this.width = width;
                this.height = height;
                output = new List<string>();
            }

            public StatusDisplay withCenteredLabel(string label)
            {
                rows.Add(new CenteredLabelRow(width, label));
                return this;
            }

            public StatusDisplay withRow(string label, Func<string> contentSource)
            {
                rows.Add(new TextRow(width, label, contentSource));
                return this;
            }

            public StatusDisplay withHorizontalLine()
            {
                rows.Add(new HorizontalLineRow(width));
                return this;
            }

            public StatusDisplay withTime()
            {
                showTime = true;
                return this;
            }

            public List<string> render()
            {
                output.Clear();
                for (int i = 0; i < height; ++i)
                {
                    if (i < rows.Count)
                    {
                        output.Add(rows[i].render());
                    } else
                    {
                        if (i == height - 1 && showTime)
                        {
                            output.Add(DateTime.Now.ToString("T"));
                        } else
                        {
                            output.Add("");
                        }
                    }
                }
                return output;
            }

            public Display build()
            {
                return new Display(textSurface, render);
            }

            public interface Row
            {
                string render();
            }

            public class TextRow : Row
            {
                private readonly int width;
                private readonly string label;
                private readonly Func<string> contentSource;

                public TextRow(int width, string label, Func<string> contentSource)
                {
                    this.width = width;
                    this.label = label;
                    this.contentSource = contentSource;
                }

                public string render() {
                    string content = contentSource.Invoke();
                    int remainingLength = width - content.Length - label.Length;
                    if (remainingLength <= 0) {
                        remainingLength = 1;
                    }
                    return label + new string(' ', remainingLength) + content;
                }
            }

            public class HorizontalLineRow : Row
            {
                private readonly string content;
                public HorizontalLineRow(int width)
                {
                    this.content = new string('-', width);
                }

                public string render()
                {
                    return content;
                }
            }

            public class CenteredLabelRow : Row
            {
                private readonly string content;
                
                public CenteredLabelRow(int width, string label)
                {
                    content = new string(' ', (width - label.Length) / 2) + label;
                }

                public string render()
                {
                    return content;
                }
            }
        }
    }
}
