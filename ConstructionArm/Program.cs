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
        private readonly Settings settings;
        private readonly IMyCockpit cockpit;
        enum State
        {
            UNKNOWN,
            STOWED,
            ACTIVATING,
            ACTIVE,
            STOWING,
            EMERGENCY_STOP,
        };

        private State state;
        private ScriptDisplay scriptDisplay;
        private LogDisplay log;
        private RotorController headRotor;
        private RotorController baseRotor;
        private List<ArmComponent> components;
        private ArmAssemblyA assemblyA;
        private InputController inputController;

        public Program()
        {
            state = State.UNKNOWN;
            settings = new Settings(Me);
            cockpit = new BlockFinder<IMyCockpit>(this).WithCustomData(settings.ArmTag).InSameConstructAs(Me).Get();
            inputController = new InputController(cockpit);
            scriptDisplay = new ScriptDisplay(Me, Runtime, "Construction Arm", "0.0.1");
            log = new LogDisplay(cockpit.GetSurface(0), 8, 14);
            components = new List<ArmComponent>();
            baseRotor = getRotorController("R1", 0, 0);
            headRotor = getRotorController("R2", 0, 0);
            StackedPistons boom1 = getBoom("B1");
            StackedPistons boom2 = getBoom("B2");
            new StatusDisplay(cockpit.GetSurface(1), 35, 22)
               .WithCenteredLabel("Construction Arm")
               .WithHorizontalLine()
               .WithRow("Status", () => state.ToString())
               .WithOptionalRow("", () => "Recompile script to continue", () => state == State.EMERGENCY_STOP)
               .WithRow("R1", (baseRotor as ArmComponent).getStatus)
               .WithRow("R2", (headRotor as ArmComponent).getStatus)
               .WithRow("B1", boom1.getStatus)
               .WithRow("B2", boom2.getStatus)
               .WithHorizontalLine()
               .WithRow("Control", () => inputController.getUserInput().ToString())
               .Build();
            assemblyA = new ArmAssemblyA(
                headRotor,
                baseRotor,
                getRotorController("H1", (float)-Math.PI / 6, (float)Math.PI / 2),
                getRotorController("H2", (float)-Math.PI / 3, (float)-Math.PI / 2),
                getRotorController("H3", (float)-Math.PI / 3, (float)-Math.PI / 2),
                getRotorController("H4", (float)Math.PI / 6, (float)Math.PI / 2),
                boom1,
                boom2);
            components.Add(assemblyA);

            if ((assemblyA as ArmComponent).isIdle())
            {
                state = State.ACTIVE;
                Runtime.UpdateFrequency = UpdateFrequency.Update1;
            } else if ((assemblyA as ArmComponent).isStowed()) 
            {
                state = State.STOWED;
            } else
            {
                scriptDisplay.Write("Starting in an unexpected state");
                state = State.ACTIVE;
                Runtime.UpdateFrequency = UpdateFrequency.Update1;
            }
            Display.Render();
        }

        private RotorController getRotorController(string tag, float idleAngle, float stowedAngle)
        {
            return new RotorController(
                    new BlockFinder<IMyMotorAdvancedStator>(this)
                        .InSameConstructAs(Me)
                        .WithCustomData(settings.ArmTag)
                        .WithCustomData(tag)
                        .Get(), 
                    idleAngle, 
                    stowedAngle, 
                    settings.MaxRadians,
                    settings.MaxAcceleration,
                    settings.IdleDelta);
        }

        private StackedPistons getBoom(string tag)
        {
            return new StackedPistons(
                    "Piston " + tag,
                    new BlockFinder<IMyPistonBase>(this)
                        .InSameConstructAs(Me)
                        .WithCustomData(settings.ArmTag)
                        .WithCustomData(tag)
                        .GetAll(),
                    log.Log);

        }

            public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (String.IsNullOrEmpty(argument))
            {
                processTick();
            }
            else
            {
                processInput(argument);
            }
            Display.Render();
        }

        private void processTick()
        {
            if (state == State.EMERGENCY_STOP || state == State.UNKNOWN)
            {
                return;
            }
            if (state == State.ACTIVATING)
            {
                if ((assemblyA as ArmComponent).isIdle())
                {
                    scriptDisplay.Write("Started, idling");
                    state = State.ACTIVE;
                }
            }

            if (state == State.STOWING)
            {
                if ((assemblyA as ArmComponent).isIdle())
                {
                    scriptDisplay.Write("Stowing");
                    (assemblyA as ArmComponent).stow();
                }
                if ((assemblyA as ArmComponent).isStowed())
                {
                    scriptDisplay.Write("Stowed. Shutting down");
                    state = State.STOWED;
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                }
            }
            bool emergencyStop = false;
            foreach (ArmComponent component in components)
            {
                ErrorReport report = component.getError();
                if (report.RequiresEmergencyStop)
                {
                    emergencyStop = true;
                }
                if (!String.IsNullOrEmpty(report.Message))
                {
                    log.Log(report.Message);
                }
            }

            if (emergencyStop)
            {
                performEmergencyStop();
            }

            if (state == State.ACTIVE)
            {
                assemblyA.handleUserInput(inputController.getUserInput());
            }

            foreach (ArmComponent component in components)
            {
                component.tick();
            }
        }

        private void processInput(string argument)
        {
            log.Log("Arg " + argument + " received");
            switch (argument)
            {
                case "START":
                    if (state == State.STOWED)
                    {
                        state = State.ACTIVATING;
                        (assemblyA as ArmComponent).returnToIdle();
                        Runtime.UpdateFrequency = UpdateFrequency.Update1;
                        scriptDisplay.Write("Starting");
                    }
                    break;
                case "STOP":
                    if (state == State.ACTIVE)
                    {
                        (assemblyA as ArmComponent).returnToIdle();
                        state = State.STOWING;
                        scriptDisplay.Write("Stopping");
                    }
                    break;
                case "EMERGENCY STOP":
                    performEmergencyStop();
                    break;
                case "IDLE":
                    if (state == State.ACTIVE)
                    {
                        (assemblyA as ArmComponent).returnToIdle();
                        scriptDisplay.Write("Returning to idle");
                    }
                    break;
                default:
                    log.Log("Invalid argument: " + argument);
                    break;
            }
        }

        private void performEmergencyStop()
        {
            log.Log("Performing emergency stop!");
            scriptDisplay.Write("EMERGENCY STOP");
            state = State.EMERGENCY_STOP;
            foreach (ArmComponent component in components)
            {
                component.emergencyStop();
            }
        }
    }
}
