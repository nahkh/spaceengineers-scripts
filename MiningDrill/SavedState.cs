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
        public class SavedState
        {
            private readonly Program p;
            private int stepCount;
            private Vector3D startingPosition;

            public SavedState(Program p)
            {
                this.p = p;
            }

            public void Save()
            {
                p.Storage = stepCount.ToString() + ":" + startingPosition.X.ToString() + ":" + startingPosition.Y.ToString() + ":" + startingPosition.Z.ToString();
            }

            public void Parse(Vector3D currentPosition)
            {
                if (p.Storage.Length == 0)
                {
                    stepCount = 0;
                    startingPosition = new Vector3D(currentPosition.X, currentPosition.Y, currentPosition.Z);
                    return;
                }

                string[] parts = p.Storage.Split(':');
                stepCount = int.Parse(parts[0]);
                startingPosition = new Vector3D(double.Parse(parts[1]), double.Parse(parts[2]), double.Parse(parts[3]));
            }

            public int StepCount
            {
                get
                {
                    return stepCount;
                }
                set
                {
                    stepCount = value;
                }
            }

            public Vector3D StartingPosition
            {
                get
                {
                    return startingPosition;
                }
                set
                {
                    startingPosition = new Vector3D(value.X, value.Y, value.Z);
                }
            }

            public void Reset()
            {
                stepCount = 0;
            }
        }
    }
}
