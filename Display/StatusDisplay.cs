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
            private readonly List<Row> visibleRows;
            private readonly IMyTextSurface textSurface;
            private readonly int width;
            private readonly int height;
            private bool showTime = false;
            private bool showRowLabel = true;

            public StatusDisplay(IMyTextSurface textSurface, int width, int height)
            {
                rows = new List<Row>();
                visibleRows = new List<Row>();
                this.textSurface = textSurface;
                this.width = width;
                this.height = height;
                output = new List<string>();
            }

            public StatusDisplay WithCenteredLabel(string label)
            {
                rows.Add(new CenteredLabelRow(width, label));
                return this;
            }

            public StatusDisplay WithRow(string label, Func<string> contentSource)
            {
                rows.Add(new TextRow(width, label, contentSource));
                return this;
            }

            public StatusDisplay WithOptionalRow(string label, Func<string> contentSource, Func<bool> predicate)
            {
                rows.Add(new OptionalRow(new TextRow(width, label, contentSource), predicate));
                return this;
            }

            public StatusDisplay WithHorizontalLine()
            {
                rows.Add(new HorizontalLineRow(width));
                return this;
            }

            public StatusDisplay WithLog(LogBuffer logBuffer)
            {
                rows.AddList(logBuffer.GetRows());
                return this;
            }

            public StatusDisplay WithTime()
            {
                showTime = true;
                return this;
            }

            public List<string> Render()
            {
                visibleRows.Clear();
                foreach (Row row in rows)
                {
                    if (row.IsVisible())
                    {
                        visibleRows.Add(row);
                    }
                }
                output.Clear();
                for (int i = 0; i < height; ++i)
                {
                    if (i < visibleRows.Count)
                    {
                        output.Add(visibleRows[i].Render());
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

            public Display Build()
            {
                return new Display(textSurface, Render);
            }

            public interface Row
            {
                bool IsVisible();
                string Render();
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

                public string Render() {
                    string content = contentSource.Invoke();
                    int remainingLength = width - content.Length - label.Length;
                    if (remainingLength <= 0) {
                        remainingLength = 1;
                    }
                    return label + new string(' ', remainingLength) + content;
                }

                public bool IsVisible()
                {
                    return true;
                }
            }

            public class HorizontalLineRow : Row
            {
                private readonly string content;
                public HorizontalLineRow(int width)
                {
                    this.content = new string('-', width);
                }

                public string Render()
                {
                    return content;
                }
                public bool IsVisible()
                {
                    return true;
                }
            }

            public class CenteredLabelRow : Row
            {
                private readonly string content;
                
                public CenteredLabelRow(int width, string label)
                {
                    content = new string(' ', (width - label.Length) / 2) + label;
                }

                public string Render()
                {
                    return content;
                }
                public bool IsVisible()
                {
                    return true;
                }
            }

            public class OptionalRow : Row
            {
                private readonly Row delegateRow;
                private readonly Func<bool> predicate;

                public OptionalRow(Row row, Func<bool> predicate) {
                    delegateRow = row;
                    this.predicate = predicate;
                }

                public string Render()
                {
                    return delegateRow.Render();
                }

                public bool IsVisible()
                {
                    return predicate();
                }
            }

            public class FreeRow : Row
            {
                private readonly Func<string> provider;

                public FreeRow(Func<string> provider)
                {
                    this.provider = provider;
                }

                public string Render()
                {
                    return provider.Invoke();
                }

                public bool IsVisible()
                {
                    return true;
                }
            }

            public class LogBuffer
            {
                private readonly int rowCount;
                private readonly List<string> rows;

                public LogBuffer(int rowCount)
                {
                    this.rowCount = rowCount;
                    rows = new List<string>();
                }

                public void Write(string logMessage)
                {
                    rows.Add(logMessage);
                    while(rows.Count > rowCount)
                    {
                        rows.RemoveAt(0);
                    }
                }
                
                private Func<string> ForRow(int row)
                {
                    return () =>
                    {
                        if (row >= 0 && row < rows.Count)
                        {
                            return rows[row];
                        }
                        else
                        {
                            return "";
                        }
                    };
                }

                public List<Row> GetRows()
                {
                    List<Row> rows = new List<Row>();
                    for (int i = 0; i < rowCount; ++i)
                    {
                        rows.Add(new FreeRow(ForRow(i)));
                    }
                    return rows;
                }
            }
        }
    }
}
