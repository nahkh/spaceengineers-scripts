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
        public class AssemblerManager
        {
            private readonly List<IMyAssembler> assemblers;
            private readonly Action<string> logger;
            private readonly IMyAssembler primaryAssembler;

            public AssemblerManager(List<IMyAssembler> assemblers, Action<string> logger)
            {
                this.assemblers = assemblers;
                this.logger = logger;
                primaryAssembler = assemblers.Find(assembler => !assembler.CooperativeMode);
                if (primaryAssembler == null)
                {
                    throw new NullReferenceException("Primary assembler not found");
                }
            }

            public void ClearQueues()
            {
                assemblers.ForEach(assembler => assembler.ClearQueue());
            }

            public void AddToQueue(Component.ComponentType type, decimal amount)
            {
                if (amount <= 0)
                {
                    return;
                }
                MyDefinitionId id = Component.DefinitionId(type);
                if (id == null)
                {
                    logger.Invoke(type.ToString());
                } else
                {
                    try
                    {
                        primaryAssembler.AddQueueItem(id, amount);
                    } catch (Exception e)
                    {
                        logger.Invoke(type.ToString() + " " + id.ToString());
                    }
                }
                
            }
        }
    }
}
