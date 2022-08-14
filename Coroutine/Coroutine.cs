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
        public class Coroutine
        {
            private readonly Program p;
            private readonly int instructionCountMargin;

            public Coroutine(Program p, int instructionCountMargin = 1000)
            {
                if (p == null)
                {
                    throw new ArgumentNullException(nameof(p));
                }
                this.p = p;
                this.instructionCountMargin = instructionCountMargin;
            }

            public Result<I, T> Execute<I, T>(IEnumerator<I> inputs, Func<I, T> method)
            {
                int prevInstructionCount = p.Runtime.CurrentInstructionCount;
                int expectedCountIncrease = 0;
                List<T> results = new List<T>();
                bool finished = true;
                
                while (inputs.MoveNext() && p.Runtime.CurrentInstructionCount + expectedCountIncrease + instructionCountMargin < p.Runtime.MaxInstructionCount)
                {
                    finished = false;
                    results.Add(method.Invoke(inputs.Current));
                    expectedCountIncrease = p.Runtime.CurrentInstructionCount - prevInstructionCount;
                    prevInstructionCount = p.Runtime.CurrentInstructionCount;
                }
                return new Result<I, T>(results, inputs, finished);
            }

            public class Result<I, T>
            {
                public readonly bool finished;
                public readonly List<T> result;
                public readonly IEnumerator<I> remaining;

                public Result(List<T> result, IEnumerator<I> remaining, bool finished) {
                    this.result = result;
                    this.remaining = remaining;
                    this.finished = finished;
                }
            }
        }
    }
}
