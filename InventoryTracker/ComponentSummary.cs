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
        public class PartSummary : InventorySummary<Component.ComponentType>
        {

            public PartSummary(List<IMyTerminalBlock> cargoContainers) : base(cargoContainers)
            {
            }

            protected override MyItemType ItemType(Component.ComponentType t)
            {
                return Component.ItemType(t);
            }
            protected override IEnumerable<Component.ComponentType> Types()
            {
                return Component.Types();
            }
        }
    }
}
