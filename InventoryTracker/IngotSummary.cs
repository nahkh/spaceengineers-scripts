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
        public class IngotSummary : InventorySummary<Ingot.IngotType>
        {

            public IngotSummary(List<IMyTerminalBlock> cargoContainers) : base(cargoContainers)
            {
            }

            protected override MyItemType ItemType(Ingot.IngotType t)
            {
                return Ingot.ItemType(t);
            }
            protected override IEnumerable<Ingot.IngotType> Types()
            {
                return Ingot.Types();
            }

            protected override float ItemValue(MyFixedPoint point)
            {
                return point.RawValue / 1000000000f;
            }
        }
    }
}
