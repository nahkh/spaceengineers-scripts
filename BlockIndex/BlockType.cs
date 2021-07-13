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
        public class BlockType
        {
            private readonly string label;
            private readonly MyCubeSize cubeSize;
            private readonly Dictionary<Component.ComponentType, int> count;

            public BlockType(string label, MyCubeSize cubeSize, Dictionary<Component.ComponentType, int> count)
            {
                this.label = label;
                this.cubeSize = cubeSize;
                this.count = count;
            }

            public MyCubeSize Size
            {
                get
                {
                    return cubeSize;
                }
            }

            public string Label
            {
                get
                {
                    return label;
                }
            }

            public Dictionary<Component.ComponentType, int> Components
            {
                get
                {
                    return count;
                }
            }

        }
    }
}
