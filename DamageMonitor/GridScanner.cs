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
        public class GridScanner
        {
            public static List<Vector3I> GetBlocks(IMyCubeBlock startingPoint)
            {
                List<Vector3I> foundBlocks = new List<Vector3I>();
                IMyCubeGrid grid = startingPoint.CubeGrid;
                HashSet<Vector3I> visited = new HashSet<Vector3I>();
                List<Vector3I> frontier = new List<Vector3I>();
                frontier.Add(startingPoint.Position);
                while (frontier.Count > 0)
                {
                    Vector3I node = frontier.Pop();
                    if (visited.Contains(node))
                    {
                        continue;
                    }
                    foundBlocks.Add(node);
                    visited.Add(node);
                    foreach(Vector3I neighbor in Neighbors(node))
                    {
                        if (grid.CubeExists(neighbor))
                        {
                            frontier.Add(neighbor);
                        }
                    }
                }

                return foundBlocks;
            }

            private static Vector3I[] Neighbors(Vector3I position)
            {
                Vector3I[] neighbors = new Vector3I[6];
                neighbors[0] = position + Vector3I.Right;
                neighbors[1] = position + Vector3I.Left;
                neighbors[2] = position + Vector3I.Up;
                neighbors[3] = position + Vector3I.Down;
                neighbors[4] = position + Vector3I.Forward;
                neighbors[5] = position + Vector3I.Backward;
                return neighbors;
            }
        }
    }
}
