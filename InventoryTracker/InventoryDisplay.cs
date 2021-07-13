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
        public abstract class InventoryDisplay<T>
        {
            private readonly string name;
            private readonly IMyTextSurface surface;
            private readonly Func<Dictionary<T, float>> valueSource;
            private Dictionary<T, float> currentValues;
            private DateTime lastUpdate;
            private static readonly TimeSpan updateInterval = TimeSpan.FromMinutes(1);

            public InventoryDisplay(string name, IMyTextSurface surface, Func<Dictionary<T, float>> valueSource)
            {
                this.name = name;
                this.surface = surface;
                this.valueSource = valueSource;
                lastUpdate = DateTime.MinValue;
                currentValues = null;
            }

            private Dictionary<T, float> GetCounts()
            {
                if (currentValues == null || DateTime.Now - lastUpdate > updateInterval)
                {
                    currentValues = valueSource.Invoke();
                    lastUpdate = DateTime.Now;
                }
                return currentValues;
            }

            protected abstract IEnumerable<T> Types();

            public Display Build()
            {
                StatusDisplay statusDisplay = new StatusDisplay(surface, 36, 26)
                    .WithCenteredLabel(name)
                    .WithHorizontalLine();
                foreach (T type in Types())
                {
                    statusDisplay.WithRow(type.ToString(), GetForType(type));
                }
                return statusDisplay
                    .WithHorizontalLine()
                    .WithRow("Total", GetTotal)
                    .WithTime()
                    .Build();
            }

            private Func<string> GetForType(T type)
            {
                return () => GetCounts().GetValueOrDefault(type, 0f).ToString("n2");
            }

            private string GetTotal()
            {
                return GetCounts().Values.Aggregate((a, b) => a + b).ToString("n2");
            }
        }
    }
}
