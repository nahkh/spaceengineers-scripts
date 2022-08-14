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
        public class PositionRenderer
        {
            private enum BlockState
            {
                OK,
                DAMAGED,
                DESTROYED,
            }

            private readonly Dictionary<Vector3I, Vector2I> positions;
            private readonly Dictionary<Vector3I, BlockState> blockStates;
            private readonly Dictionary<Vector2I, BlockState> renderedState;
            private readonly IMyCubeGrid cubeGrid;
            private readonly Action<string> logger;
            private readonly List<Vector3I> positions3;
            private readonly Renderer renderer;
            private readonly int xOffset;
            private readonly int yOffset;
            private bool dirty;
            private int damagedBlockCount;
            private int destroyedBlockCount;
            private int totalBlockCount;

            public PositionRenderer(IMyCubeGrid cubeGrid, Settings settings, Action<string> logger, IMyTextSurface surface, List<Vector3I> positions3, int margin)
            {
                blockStates = new Dictionary<Vector3I, BlockState>();
                renderedState = new Dictionary<Vector2I, BlockState>();
                this.cubeGrid = cubeGrid;
                this.logger = logger;
                this.positions3 = positions3;
                positions = Flatten(positions3, settings.XAxis, settings.YAxis);
                int minX = int.MaxValue;
                int minY = int.MaxValue;
                int maxX = int.MinValue;
                int maxY = int.MinValue;
                
                foreach (Vector2I pos in positions.Values)
                {
                    if (pos.X < minX)
                    {
                        minX = pos.X;
                    }
                    if (pos.X > maxX)
                    {
                        maxX = pos.X;
                    }
                    if (pos.Y < minY)
                    {
                        minY = pos.Y;
                    }
                    if (pos.Y > maxY)
                    {
                        maxY = pos.Y;
                    }
                }
                foreach (Vector3I position in positions3)
                {
                    blockStates[position] = BlockState.OK;
                }
                int width = maxX - minX + 1 + margin * 2;
                int height = maxY - minY + 1 + margin * 2;
                logger.Invoke("Width " + width + " height " + height);
                renderer = new Renderer(surface, width, height, Color.Black);
                xOffset = -minX + margin;
                yOffset = -minY + margin;
                dirty = true;
                totalBlockCount = positions3.Count;
            }

            private static Dictionary<Vector3I, Vector2I> Flatten(List<Vector3I> threeDimPositions, Func<Vector3I, int> x, Func<Vector3I, int> y)
            {
                Dictionary<Vector3I, Vector2I> vectors = new Dictionary<Vector3I, Vector2I>();

                foreach (Vector3I vec in threeDimPositions)
                {
                    Vector2I newVector = new Vector2I(x.Invoke(vec), y.Invoke(vec));
                    vectors.Add(vec, newVector);
                }

                return vectors;
            }

            private void CheckChangesInDamage()
            {
                renderedState.Clear();
                damagedBlockCount = 0;
                destroyedBlockCount = 0;
                foreach(Vector3I position in positions3)
                {
                    BlockState blockState = DetermineCurrentBlockState(position);
                    if (blockState != blockStates[position])
                    {
                        dirty = true;
                    }
                    blockStates[position] = blockState;
                    switch(blockState)
                    {
                        case BlockState.DAMAGED:
                            ++damagedBlockCount;
                            break;
                        case BlockState.DESTROYED:
                            ++destroyedBlockCount;
                            break;
                    }
                    AssignBlockStateForRendering(positions[position], blockState);
                }
            }

            private void AssignBlockStateForRendering(Vector2I position, BlockState blockState)
            {
                if (!renderedState.ContainsKey(position))
                {
                    renderedState[position] = blockState;
                }
                if (renderedState[position] < blockState)
                {
                    renderedState[position] = blockState;
                }
            }

            private BlockState DetermineCurrentBlockState(Vector3I position)
            {
                if (!cubeGrid.CubeExists(position))
                {
                    return BlockState.DESTROYED;
                }
                IMySlimBlock slimBlock = cubeGrid.GetCubeBlock(position);
                if (slimBlock != null && (slimBlock.CurrentDamage > 0 || slimBlock.HasDeformation))
                {
                    return BlockState.DAMAGED;
                } else
                {
                    return BlockState.OK;
                }
            }

            public void Update()
            {
                CheckChangesInDamage();
                if (dirty)
                {
                    logger.Invoke("Detected changes");
                    logger.Invoke("Dam " + damagedBlockCount + ", des " + destroyedBlockCount);
                    Render();
                    dirty = false;
                }
            }

            private void Render()
            {
                renderer.StartDraw();
                foreach (Vector2I pos in renderedState.Keys)
                {
                    renderer.SetPixel(pos.X + xOffset, pos.Y + yOffset, ColorForState(renderedState[pos]));
                }
                renderer.Flush();
            }

            
            private Color ColorForState(BlockState state)
            {
                switch(state)
                {
                    case BlockState.OK:
                        return Color.DarkBlue;
                    case BlockState.DAMAGED:
                        return Color.OrangeRed;
                    case BlockState.DESTROYED:
                        return Color.DarkRed;
                    default:
                        return Color.Yellow;
                }
            }

        }
    }
}
