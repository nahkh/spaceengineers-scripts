using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class BlockFinder<T> where T : class, IMyTerminalBlock
        {
            private readonly IMyGridTerminalSystem gridTerminalSystem;
            private readonly List<Func<T, bool>> predicates;
            private readonly List<string> description;

            public BlockFinder(Program p)
            {
                gridTerminalSystem = p.GridTerminalSystem;
                predicates = new List<Func<T, bool>>();
                description = new List<string>();
            }

            public BlockFinder<T> WithName(string name)
            {
                description.Add("the name '" + name + "'");
                return MatchingPredicate(block => block.CustomName.Equals(name));
            }

            public BlockFinder<T> InSameConstructAs(IMyTerminalBlock terminalBlock)
            {
                description.Add("in the same construct as " + terminalBlock.CustomName);
                return MatchingPredicate(block => block.IsSameConstructAs(terminalBlock));
            }

            public BlockFinder<T> WithCustomData(string data)
            {
                description.Add("'" + data + "' in custom data");
                return MatchingPredicate(block => block.CustomData.Contains(data));
            }

            public BlockFinder<T> WithoutCustomData(string data)
            {
                description.Add("'" + data + "' NOT in custom data");
                return MatchingPredicate(block => !block.CustomData.Contains(data));
            }

            public BlockFinder<T> WithCustomPredicate(Func<T, bool> predicate)
            {
                description.Add("a custom predicate");
                return MatchingPredicate(predicate);
            }

            private BlockFinder<T> MatchingPredicate(Func<T, bool> predicate)
            {
                predicates.Add(predicate);
                return this;
            }

            public T Get()
            {
                T value = TryGet();
                if (value == null)
                {
                    string error = "Could not find block of type " + typeof(T).Name;
                    if (description.Count > 0)
                    {
                        error += " with " + description[0];
                        for (int i = 1; i < description.Count; ++i)
                        {
                            error += " AND " + description[i];
                        }
                    }
                    throw new Exception(error);
                }

                return value;
            }

            public T TryGet()
            {
                List<T> results = GetAll();
                if (results.Count == 0)
                {
                    return null;
                }
                return results[0];
            }

            public List<T> GetAll()
            {
                List<T> list = new List<T>();
                gridTerminalSystem.GetBlocksOfType(list, Test);
                return list;
            }

            private bool Test(T block)
            {
                foreach (Func<T, bool> predicate in predicates)
                {
                    if (!predicate.Invoke(block))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
