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
        public class TableDisplay<ROW, COL>
        {
            Dictionary<COL, int> columnWidth;
            List<COL> columns;
            List<ROW> rows;
            Func<ROW, COL, string> dataSource;
            Dictionary<COL, Alignment> columnAlignment;
            Dictionary<ROW, Func<bool>> rowVisibility;
            private readonly IMyTextSurface textSurface;
            private readonly int width;
            private readonly int height;
            private int rowIdWidth;
            private int dynamicColumnWidth;
            private bool showRowLabel;

            private string label = null;

            public enum Alignment
            {
                LEFT,
                RIGHT,
                MIDDLE,
            }

            public TableDisplay(IMyTextSurface textSurface, int width, int height)
            {
                columnWidth = new Dictionary<COL, int>();
                columns = new List<COL>();
                columnAlignment = new Dictionary<COL, Alignment>();
                rowVisibility = new Dictionary<ROW, Func<bool>>();
                rows = new List<ROW>();
                this.textSurface = textSurface;
                this.width = width;
                this.height = height;
                rowIdWidth = 0;
                dynamicColumnWidth = 0;
                showRowLabel = true;
            }


            public TableDisplay<ROW, COL> WithoutRowLabel()
            {
                showRowLabel = false;
                return this;
            }


            public TableDisplay<ROW, COL> Column(COL col, int columnWidth = -1, Alignment alignment = Alignment.LEFT)
            {
                columns.Add(col);
                if (columnWidth > 0) {
                    this.columnWidth[col] = columnWidth;
                }
                columnAlignment[col] = alignment;
                return this;
            }

            public TableDisplay<ROW, COL> Row(ROW row)
            {
                return Row(row, () => true);
            }

            public TableDisplay<ROW, COL> Row(ROW row, Func<bool> visibility)
            {
                if (row.ToString().Length > rowIdWidth)
                {
                    rowIdWidth = row.ToString().Length;
                }
                rows.Add(row);
                rowVisibility[row] = visibility;
                return this;
            }

            public TableDisplay<ROW, COL> Rows(IEnumerable<ROW> rows)
            {
                foreach (ROW row in rows)
                {
                    Row(row);
                }
                return this;
            }

            public TableDisplay<ROW, COL> WithData(Func<ROW, COL, string> dataSource)
            {
                this.dataSource = dataSource;   
                return this;
            }
            public TableDisplay<ROW, COL> WithLabel(string label)
            {
                this.label = Pad(label, Alignment.MIDDLE, width);
                return this;
            }

            private List<string> Render()
            {
                List<string> output = new List<string>();
                if (label != null)
                {
                    output.Add(label);
                }
                string columnHeaderRow = "";
                if (showRowLabel)
                {
                    columnHeaderRow = new string(' ', rowIdWidth);
                }
                foreach(COL col in columns)
                {
                    columnHeaderRow += Pad(col.ToString(), columnAlignment[col], columnWidth.GetValueOrDefault(col, dynamicColumnWidth));
                }
                output.Add(columnHeaderRow);
                output.Add(new string('-', width));
                foreach(ROW row in rows)
                {
                    if (rowVisibility[row].Invoke())
                    {
                        string normalRow = "";
                        if (showRowLabel)
                        {
                            normalRow += Pad(row.ToString(), Alignment.LEFT, rowIdWidth);
                        }
                        foreach (COL col in columns)
                        {
                            normalRow += Pad(dataSource.Invoke(row, col), columnAlignment[col], columnWidth.GetValueOrDefault(col, dynamicColumnWidth));
                        }
                        output.Add(normalRow);
                    }
                }

                return output;
            }

            static private string Pad(string input, Alignment alignment, int desiredWidth)
            {
                if (input.Length > desiredWidth)
                {
                    return input.Substring(0, desiredWidth);
                }
                int spareWidth = desiredWidth - input.Length;
                switch(alignment)
                {
                    case Alignment.LEFT:
                        return input + new string(' ', spareWidth);
                    case Alignment.RIGHT:
                        return new string(' ', spareWidth) + input;
                    case Alignment.MIDDLE:
                        int left = spareWidth / 2;
                        int right = spareWidth - left;
                        return new string(' ', left) + input + new string(' ', right);
                    default:
                        throw new ArgumentException("Unknown alignment: " + alignment);
                }
            }

            public Display Build()
            {
                int totalTakenWidth = 0;
                if (showRowLabel)
                {
                    totalTakenWidth += rowIdWidth;
                }
                foreach(COL col in columnWidth.Keys)
                {
                    totalTakenWidth += columnWidth[col];
                }
                int remainingWidth = width - totalTakenWidth;
                if (remainingWidth < 0)
                {
                    throw new ArgumentException("Not enough space for all columns");
                }
                int columnsWithoutExplicitWidth = columns.Count - columnWidth.Count;
                if (columnsWithoutExplicitWidth > 0)
                {
                    dynamicColumnWidth = remainingWidth / columnsWithoutExplicitWidth;
                }
                return new Display(textSurface, Render);
            }

        }
    }
}
