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
        public class OreDisplay
        {
            private readonly IMyTextSurface surface;
            private readonly Func<Dictionary<Ore.OreType, float>> oreCountSource;
            private Dictionary<Ore.OreType, float> currentOres;
            private DateTime lastUpdate;
            private static readonly TimeSpan updateInterval = TimeSpan.FromMinutes(1);

            public OreDisplay(IMyTextSurface surface, Func<Dictionary<Ore.OreType, float>> oreCountSource)
            {
                this.surface = surface;
                this.oreCountSource = oreCountSource;
                lastUpdate = DateTime.MinValue;
                currentOres = null;
            }

            private Dictionary<Ore.OreType, float> GetOreCount()
            {
                if (currentOres == null || DateTime.Now - lastUpdate > updateInterval)
                {
                    currentOres = oreCountSource.Invoke();
                }
                return currentOres;
            }

            public Display Build()
            {
                StatusDisplay statusDisplay = new StatusDisplay(surface, 36, 30)
                    .withCenteredLabel("Ore in storage")
                    .withHorizontalLine();
                foreach (Ore.OreType type in Ore.Types())
                {
                    statusDisplay.withRow(type.ToString(), GetForOre(type));
                }
                return statusDisplay
                    .withHorizontalLine()
                    .withRow("Total", GetTotal)
                    .build();
            }

            private Func<string> GetForOre(Ore.OreType type)
            {
                return () => GetOreCount().GetValueOrDefault(type, 0f).ToString("n2");
            }

            private string GetTotal()
            {
                return GetOreCount().Values.Aggregate((a, b) => a + b).ToString("n2");
            }
        }
    }
}
