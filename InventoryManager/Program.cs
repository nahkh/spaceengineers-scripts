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
    partial class Program : MyGridProgram
    {
        Settings settings;
        List<IMyTerminalBlock> inventoriesOnStation;
        ItemMover ingotMover;
        ItemMover componentMover;
        ItemMover ammoMover;
        ItemMover toolMover;
        ScriptDisplay scriptDisplay;

        public Program()
        {
            settings = new Settings(Me);
            scriptDisplay = new ScriptDisplay(Me, Runtime);
            inventoriesOnStation = new BlockFinder<IMyTerminalBlock>(this)
                .inSameConstructAs(Me)
                .withoutCustomData(settings.IgnoreTag)
                .withCustomPredicate(block => block.HasInventory)
                .getAll();
            BuildIngotDisplay();
            BuildComponentDisplay();
            if (settings.MoveItems)
            {
                BuildComponentMover();
                BuildIngotMover();
                BuildAmmoMover();
                BuildToolMover();
            }
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        private void BuildIngotDisplay()
        {
            IngotSummary ingotSummary = new IngotSummary(new BlockFinder<IMyTerminalBlock>(this)
                .inSameConstructAs(Me)
                .withCustomPredicate(item => item.HasInventory)
                .getAll());
            new IngotDisplay(new BlockFinder<IMyTextPanel>(this)
                .withCustomData(settings.IngotDisplayTag).get(), ingotSummary.Amounts)
                .Build();
        }

        private void BuildComponentDisplay()
        {
            PartSummary partSummary = new PartSummary(new BlockFinder<IMyTerminalBlock>(this).inSameConstructAs(Me).withCustomPredicate(item => item.HasInventory).getAll());
            new PartDisplay(new BlockFinder<IMyTextPanel>(this).withCustomData(settings.ComponentDisplayTag).get(), partSummary.Amounts)
                .AlwaysShow()
                .Build();
        }

        private void BuildIngotMover()
        {
            List<IMyTerminalBlock> inputInventories = new List<IMyTerminalBlock>();
            List<IMyInventory> outputInventories = new List<IMyInventory>();
            foreach(IMyTerminalBlock block in inventoriesOnStation)
            {
                if (!block.CustomData.Contains(settings.IngotStorageTag)) {
                    inputInventories.Add(block);
                } else
                {
                    for (int i = 0; i < block.InventoryCount; ++i)
                    {
                        outputInventories.Add(block.GetInventory(i));
                    }
                }
            }

            ingotMover = new ItemMover("ingots", inputInventories, outputInventories, Ingot.ItemTypeList(), IsNotActiveAssembler, scriptDisplay.Write);
        }

        private void BuildComponentMover()
        {
            List<IMyTerminalBlock> inputInventories = new List<IMyTerminalBlock>();
            List<IMyInventory> outputInventories = new List<IMyInventory>();
            foreach (IMyTerminalBlock block in inventoriesOnStation)
            {
                if (!block.CustomData.Contains(settings.ComponentStorageTag))
                {
                    inputInventories.Add(block);
                }
                else
                {
                    for (int i = 0; i < block.InventoryCount; ++i)
                    {
                        outputInventories.Add(block.GetInventory(i));
                    }
                }
            }

            componentMover = new ItemMover("components", inputInventories, outputInventories, Component.ItemTypeList(), IsNotActiveWelder, scriptDisplay.Write);
        }

        private void BuildAmmoMover()
        {
            List<IMyTerminalBlock> inputInventories = new List<IMyTerminalBlock>();
            List<IMyInventory> outputInventories = new List<IMyInventory>();
            foreach (IMyTerminalBlock block in inventoriesOnStation)
            {
                if (!block.CustomData.Contains(settings.AmmoStorageTag))
                {
                    inputInventories.Add(block);
                }
                else if (!(block is IMyUserControllableGun))
                {
                    
                    for (int i = 0; i < block.InventoryCount; ++i)
                    {
                        outputInventories.Add(block.GetInventory(i));
                    }
                }
            }

            List<MyItemType> types = new List<MyItemType>();
            types.Add(MyItemType.MakeAmmo("SemiAutoPistolMagazine"));
            types.Add(MyItemType.MakeAmmo("FullAutoPistolMagazine"));
            types.Add(MyItemType.MakeAmmo("ElitePistolMagazine"));
            types.Add(MyItemType.MakeAmmo("AutomaticRifleGun_Mag_20rd"));
            types.Add(MyItemType.MakeAmmo("RapidFireAutomaticRifleGun_Mag_50rd"));
            types.Add(MyItemType.MakeAmmo("PreciseAutomaticRifleGun_Mag_5rd"));
            types.Add(MyItemType.MakeAmmo("UltimateAutomaticRifleGun_Mag_30rd"));
            types.Add(MyItemType.MakeAmmo("NATO_5p56x45mm"));
            types.Add(MyItemType.MakeAmmo("NATO_25x184mm"));
            types.Add(MyItemType.MakeAmmo("Missile200mm"));
            types.Add(MyItemType.MakeAmmo("SmallCaliber"));
            types.Add(MyItemType.MakeAmmo("PistolCaliber"));
            types.Add(MyItemType.MakeAmmo("LargeCaliber"));
            types.Add(MyItemType.MakeAmmo("Missile"));
            ammoMover = new ItemMover("ammo", inputInventories, outputInventories, types, block => false, scriptDisplay.Write);
        }

        private void BuildToolMover()
        {
            List<IMyTerminalBlock> inputInventories = new List<IMyTerminalBlock>();
            List<IMyInventory> outputInventories = new List<IMyInventory>();
            foreach (IMyTerminalBlock block in inventoriesOnStation)
            {
                if (!block.CustomData.Contains(settings.ToolsStorageTag))
                {
                    inputInventories.Add(block);
                }
                else
                {

                    for (int i = 0; i < block.InventoryCount; ++i)
                    {
                        outputInventories.Add(block.GetInventory(i));
                    }
                }
            }

            List<MyItemType> types = new List<MyItemType>();
            types.Add(MyItemType.MakeTool("WelderItem"));
            types.Add(MyItemType.MakeTool("Welder2Item"));
            types.Add(MyItemType.MakeTool("Welder3Item"));
            types.Add(MyItemType.MakeTool("Welder4Item"));
            types.Add(MyItemType.MakeTool("AngleGrinderItem"));
            types.Add(MyItemType.MakeTool("AngleGrinder2Item"));
            types.Add(MyItemType.MakeTool("AngleGrinder3Item"));
            types.Add(MyItemType.MakeTool("AngleGrinder4Item"));
            types.Add(MyItemType.MakeTool("HandDrillItem"));
            types.Add(MyItemType.MakeTool("HandDrill2Item"));
            types.Add(MyItemType.MakeTool("HandDrill3Item"));
            types.Add(MyItemType.MakeTool("HandDrill4Item"));
            types.Add(MyItemType.MakeTool("OxygenBottle"));
            types.Add(MyItemType.MakeTool("HydrogenBottle"));
            toolMover = new ItemMover("tools", inputInventories, outputInventories, types, block => false, scriptDisplay.Write);
        }

        private bool IsNotActiveAssembler(IMyTerminalBlock terminalBlock)
        {
            if (terminalBlock is IMyAssembler)
            {
                IMyAssembler assembler = terminalBlock as IMyAssembler;
                return !assembler.IsQueueEmpty;
            }
            return false;
        }

        private bool IsNotActiveWelder(IMyTerminalBlock terminalBlock)
        {
            if (terminalBlock is IMyShipWelder)
            {
                IMyShipWelder welder = terminalBlock as IMyShipWelder;
                return welder.Enabled;
            }
            return false;
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            componentMover?.Update();
            ingotMover?.Update();
            ammoMover?.Update();
            toolMover?.Update();
            Display.render();
        }
    }
}
