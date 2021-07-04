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
        public class PartDisplay
        {
            private readonly IMyTextSurface surface;
            private readonly Func<Dictionary<Component.ComponentType, float>> partCountSource;
            private Dictionary<Component.ComponentType, float> currentParts;
            private DateTime lastUpdate;
            private static readonly TimeSpan updateInterval = TimeSpan.FromMinutes(1);
            private List<SubassemblyCounter> subassemblyCounters;
            private bool showTotal;
            private bool alwaysShow;
            
            public PartDisplay(IMyTextSurface surface, Func<Dictionary<Component.ComponentType, float>> partCountSource)
            {
                this.surface = surface;
                this.partCountSource = partCountSource;
                lastUpdate = DateTime.MinValue;
                currentParts = null;
                subassemblyCounters = new List<SubassemblyCounter>();
                showTotal = false;
                alwaysShow = false;
            }

            private Dictionary<Component.ComponentType, float> GetPartCount()
            {
                if (currentParts == null || DateTime.Now - lastUpdate > updateInterval)
                {
                    currentParts = partCountSource.Invoke();
                    lastUpdate = DateTime.Now;
                }
                return currentParts;
            }

            public PartDisplay WithSubassemblyCounter(SubassemblyCounter counter)
            {
                subassemblyCounters.Add(counter);
                return this;
            }

            public PartDisplay ShowTotal()
            {
                showTotal = true;
                return this;
            }

            public PartDisplay AlwaysShow()
            {
                alwaysShow = true;
                return this;
            }

            public Display Build()
            {
                StatusDisplay statusDisplay = new StatusDisplay(surface, 36, 30)
                    .withCenteredLabel("Parts in storage")
                    .withHorizontalLine();
                if (alwaysShow)
                {
                    foreach (Component.ComponentType type in Component.Types())
                    {
                        statusDisplay.withRow(type.ToString(), GetForPart(type));
                    }
                }
                else
                {
                    foreach (Component.ComponentType type in Component.Types())
                    {
                        statusDisplay.withOptionalRow(type.ToString(), GetForPart(type), PartExists(type));
                    }
                }
                if (showTotal)
                {
                    statusDisplay
                        .withHorizontalLine()
                        .withRow("Total", GetTotal);
                }

                if (subassemblyCounters.Count > 0)
                {
                    statusDisplay
                        .withHorizontalLine()
                        .withCenteredLabel("Enough parts for");
                    foreach(SubassemblyCounter counter in subassemblyCounters)
                    {
                        statusDisplay.withRow(counter.Label, () => counter.CountPossibleSubassemblies(GetPartCount()).ToString());
                    }
                }

                return statusDisplay
                    .build();
            }

            private Func<string> GetForPart(Component.ComponentType type)
            {
                return () => ((long)GetPartCount().GetValueOrDefault(type, 0)).ToString();
            }

            private Func<bool> PartExists(Component.ComponentType type)
            {
                return () => GetPartCount().GetValueOrDefault(type, 0) > 0;
            }

            private string GetTotal()
            {
                return GetPartCount().Values.Aggregate((a, b) => a + b).ToString();
            }

            public class SubassemblyCounter
            {
                private readonly string label;
                private readonly Dictionary<Component.ComponentType, long> partsNeeded;

                public SubassemblyCounter(string label, Dictionary<Component.ComponentType, long> partsNeeded)
                {
                    this.label = label;
                    this.partsNeeded = partsNeeded;
                }

                public long CountPossibleSubassemblies(Dictionary<Component.ComponentType, float> availableParts)
                {
                    long possibleAssembliesCount = long.MaxValue;

                    foreach(Component.ComponentType type in partsNeeded.Keys)
                    {
                        long componentsEnoughForAssembly = ((long) availableParts.GetValueOrDefault(type, 0)) / partsNeeded[type];
                        possibleAssembliesCount = Math.Min(possibleAssembliesCount, componentsEnoughForAssembly);
                    }

                    return possibleAssembliesCount;
                }

                public string Label
                {
                    get
                    {
                        return label;
                    }
                }

                public class Builder
                {
                    private readonly string label;
                    Dictionary<Component.ComponentType, long> partsNeeded;

                    public Builder(string label)
                    {
                        this.label = label;
                        partsNeeded = new Dictionary<Component.ComponentType, long>();
                    }

                    public Builder WithPart(Component.ComponentType type, long count)
                    {
                        partsNeeded[type] = partsNeeded.GetValueOrDefault(type, 0) + count;
                        return this;
                    }

                    public SubassemblyCounter Build()
                    {
                        return new SubassemblyCounter(label, partsNeeded);
                    }
                }
            }
        }
    }
}
