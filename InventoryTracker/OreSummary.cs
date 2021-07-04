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
        public class OreSummary : InventorySummary<Ore.OreType>
        {

            public OreSummary(List<IMyTerminalBlock> cargoContainers) : base(cargoContainers)
            {
            }

            protected override MyItemType ItemType(Ore.OreType t)
            {
                return Ore.ItemType(t);
            }
            protected override IEnumerable<Ore.OreType> Types()
            {
                return Ore.Types();
            }

            protected override float ItemValue(MyFixedPoint point)
            {
                return point.RawValue / 1000000000f;
            }
        }
    }
}
