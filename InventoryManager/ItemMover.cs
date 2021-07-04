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
        public class ItemMover
        {
            private DateTime lastCheck;
            private static readonly TimeSpan checkInterval = TimeSpan.FromMinutes(1);

            private readonly string label;
            private readonly List<IMyTerminalBlock> inputInventories;
            private readonly List<IMyInventory> outputInventories;
            private readonly List<MyItemType> itemTypes;
            private readonly Func<IMyTerminalBlock, bool> predicate;
            private readonly Action<string> logger;

            public ItemMover(List<IMyTerminalBlock> inputInventories, List<IMyInventory> outputInventories, List<MyItemType> itemTypes, Func<IMyTerminalBlock, bool> predicate, Action<string> logger) : this("items", inputInventories, outputInventories, itemTypes, predicate, logger)
            {
            }

            public ItemMover(string label, List<IMyTerminalBlock> inputInventories, List<IMyInventory> outputInventories, List<MyItemType> itemTypes, Func<IMyTerminalBlock, bool> predicate, Action<string> logger)
            {
                this.label = label;
                this.inputInventories = inputInventories;
                this.outputInventories = outputInventories;
                this.itemTypes = itemTypes;
                this.predicate = predicate;
                this.logger = logger;
                lastCheck = DateTime.MinValue;
            }

            public void Update()
            {
                if (lastCheck + checkInterval < DateTime.Now)
                {
                    logger.Invoke("Checking for " + label);
                    lastCheck = DateTime.Now;
                    float itemCount = 0f;
                    foreach (IMyTerminalBlock block in inputInventories)
                    {
                        if (!predicate.Invoke(block))
                        {
                            for (int i = 0; i < block.InventoryCount; ++i)
                            {
                                itemCount += RemoveEverything(block.GetInventory(i));
                            }
                        }
                    }
                    if (itemCount < 0.01f)
                    {
                        logger.Invoke("Found none");
                    }
                    else
                    {
                        logger.Invoke("Moved " + (long)itemCount + " " + label);
                    }
                }
            }

            private bool ItemIsInteresting(MyInventoryItem item)
            {
                return itemTypes.Contains(item.Type);
            }

            private float RemoveEverything(IMyInventory inputInventory)
            {
                float totalItemCount = 0.0f;
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                inputInventory.GetItems(items, ItemIsInteresting);
                foreach (MyInventoryItem item in items)
                {
                    IMyInventory outputInventory = SelectOutputFor(inputInventory, item.Type);
                    outputInventory?.TransferItemFrom(inputInventory, item);
                    totalItemCount += item.Amount.RawValue / 1000000f;
                }
                return totalItemCount;
            }

            private IMyInventory SelectOutputFor(IMyInventory inputInventory, MyItemType type)
            {
                foreach (IMyInventory inventory in outputInventories)
                {
                    if (!inventory.IsFull && inventory.GetItemAmount(type).RawValue > 1000f && inputInventory.CanTransferItemTo(inventory, type))
                    {
                        return inventory;
                    }
                }
                foreach (IMyInventory inventory in outputInventories)
                {
                    if (!inventory.IsFull && inputInventory.CanTransferItemTo(inventory, type))
                    {
                        return inventory;
                    }
                }
                return null;
            }
        }
    }
}
