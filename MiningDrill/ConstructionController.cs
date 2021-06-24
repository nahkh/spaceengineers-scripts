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
        public class ConstructionController
        {
            private static readonly float epsilon = 0.01f;
            private readonly IMyPistonBase pistonBase;
            private readonly List<IMyShipWelder> welders;
            private readonly List<IMyShipGrinder> grinders;
            private readonly IMyConveyorSorter constructionSorter;
            private readonly IMyConveyorSorter deconstructionSorter;

            public enum State
            {
                STANDBY,
                PREPARING_TO_BUILD,
                BUILDING,
                PREPARING_TO_REMOVE,
                REMOVING,
            }

            private State state;

            public ConstructionController(IMyPistonBase pistonBase, List<IMyShipWelder> welders, List<IMyShipGrinder> grinders, IMyConveyorSorter constructionSorter, IMyConveyorSorter deconstructionSorter)
            {
                this.pistonBase = pistonBase;
                this.welders = welders;
                this.grinders = grinders;
                this.constructionSorter = constructionSorter;
                this.deconstructionSorter = deconstructionSorter;
                state = State.STANDBY;
                this.pistonBase.Velocity = -0.5f;
                StopEverything();
            }

            public void Update()
            {
                switch(state)
                {
                    case State.BUILDING:
                    case State.REMOVING:
                    case State.STANDBY:
                        // nothing to do
                        break;
                    case State.PREPARING_TO_BUILD:
                        if (Math.Abs(pistonBase.CurrentPosition - pistonBase.MaxLimit) < epsilon)
                        {
                            StopGrinders();
                            StartWelders();
                            state = State.BUILDING;
                        }
                        break;
                    case State.PREPARING_TO_REMOVE:
                        if (Math.Abs(pistonBase.CurrentPosition - pistonBase.MinLimit) < epsilon)
                        {
                            StopWelders();
                            StartGrinders();
                            state = State.REMOVING;
                        }
                        break;
                }
            }

            public void StartBuilding()
            {
                StartGrinders(); // needs to be enabled in case a partially deconstructed object is in the way
                state = State.PREPARING_TO_BUILD;
                pistonBase.Velocity = 0.5f;
            }

            public void StartRemoving()
            {
                StopEverything();
                state = State.PREPARING_TO_REMOVE;
                pistonBase.Velocity = -0.5f;
            }

            public void Stop()
            {
                StopEverything();
                state = State.STANDBY;
                pistonBase.Velocity = -0.5f;
            }

            public State GetState()
            {
                return state;
            }

            public string Info()
            {
                return state.ToString();
            }

            private void StopEverything()
            {
                StopGrinders();
                StopWelders();
            }

            private void StartGrinders()
            {
                grinders.ForEach(grinder => grinder.Enabled = true);
                deconstructionSorter.Enabled = true;
            }

            private void StopGrinders()
            {
                grinders.ForEach(grinder => grinder.Enabled = false);
                deconstructionSorter.Enabled = false;
            }

            private void StartWelders()
            {
                welders.ForEach(welder => welder.Enabled = true);
                constructionSorter.Enabled = true;
            }


            private void StopWelders()
            {
                welders.ForEach(welder => welder.Enabled = false);
                constructionSorter.Enabled = false;
            }
        }
    }
}
