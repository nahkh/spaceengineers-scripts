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
        IMyCockpit cockpit;
        Balancer balancer;
        Hoverer hoverer;

        enum State
        {
            INACTIVE,
            ACTIVE,
            LANDING,
        }

        State state;
        public Program()
        {
            cockpit = new BlockFinder<IMyCockpit>(this).Get();
            balancer = new Balancer(new BlockFinder<IMyGyro>(this).Get(), cockpit);
            List<IMyCameraBlock> cameras = new BlockFinder<IMyCameraBlock>(this).GetAll();
            hoverer = new Hoverer(cockpit, new BlockFinder<IMyThrust>(this).WithCustomData("UP").GetAll(), cameras[0], cameras[1]);
            new ScriptDisplay(Me, Runtime);
            new StatusDisplay(cockpit.GetSurface(0), 35, 15)
                .WithCenteredLabel("System status")
                .WithRow("Status", () => state.ToString())
                .WithHorizontalLine()
                .WithRow("Balancer", balancer.Info)
                .WithRow("Pitch", balancer.PitchDeviation)
                .WithRow("Roll", balancer.RollDeviation)
                .WithRow("Yaw", balancer.YawDeviation)
                .WithHorizontalLine()
                .WithRow("Hoverer", hoverer.Info)
                .WithRow("Current altitude", hoverer.AltitudeInfo)
                .WithRow("Target altitude", hoverer.DesiredAltitude)
                .WithRow("Projected altitude", hoverer.ProjectedAltitudeInfo)
                .WithRow("Last raycast", hoverer.LastTargetInfo)
                .WithTime()
                .Build();
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            state = State.INACTIVE;
        }

        public void Save()
        {
     
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument != null && argument.Length > 0)
            {
                HandleArgument(argument);
            }

            Update();
        }

        private void HandleArgument(string argument)
        {
            switch(argument)
            {
                case "START":
                    Activate();
                    break;
                case "STOP":
                    Deactivate();
                    break;
            }
        }

        private void Update()
        {
            if (state == State.LANDING)
            {
                if (hoverer.IsInactive())
                {
                    balancer.Shutdown();
                    state = State.INACTIVE;
                }
            }
            balancer.Update();
            hoverer.Update();
            Display.Render();
        }

        private void Activate()
        {
            if (state != State.INACTIVE)
            {
                return;
            }
            balancer.Activate();
            hoverer.Activate();
            state = State.ACTIVE;
        }

        private void Deactivate()
        {
            if (state != State.ACTIVE)
            {
                return;
            }
            balancer.Deactivate();
            hoverer.Deactivate();
            state = State.LANDING;
        }
    }
}
