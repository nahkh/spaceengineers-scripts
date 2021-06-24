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
        public class DrillController
        {
            private readonly IMyMotorAdvancedStator motor;
            private readonly List<IMyShipDrill> drills;

            enum State
            {
                DRILLING,
                STANDBY,
            }

            private State state;

            public DrillController(IMyMotorAdvancedStator motor, List<IMyShipDrill> drills)
            {
                this.motor = motor;
                this.drills = drills;
                Stop();
            }

            public void Start()
            {
                motor.TargetVelocityRPM = 0.5f;
                drills.ForEach(drill => drill.Enabled = true);
                state = State.DRILLING;
            }

            public void Stop()
            {
                motor.TargetVelocityRPM = 0.0f;
                drills.ForEach(drill => drill.Enabled = false);
                state = State.STANDBY;
            }

            public string Info()
            {
                return state.ToString();
            }

            public float Diameter()
            {
                int maxDistance = 0;
                for (int i = 0; i < drills.Count; ++i)
                {
                    for (int j = i + 1; j < drills.Count; ++j)
                    {
                        Vector3I ipos = drills[i].Position;
                        Vector3I jpos = drills[j].Position;
                        int pairDistance = LargestDifference(ipos, jpos);
                        if (pairDistance > maxDistance)
                        {
                            maxDistance = pairDistance;
                        }
                    }
                }

                return 2.5f * maxDistance;
            }

            private static int LargestDifference(Vector3I a, Vector3I b)
            {
                int x = Math.Abs(a.X - b.X);
                int y = Math.Abs(a.Y - b.Y);
                int z = Math.Abs(a.Z - b.Z);
                return Math.Max(Math.Max(x, y), z);
            }
        }
    }
}
