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
        Walker walker;
        ConstructionController constructionController;
        DrillController drillController;
        BracingLeg bracingLeg;
        IMyMotorAdvancedStator drillMotor;
        SavedState savedState;
        Settings settings;
        SpeedTracker speedTracker;

        enum State
        {
            ERROR,
            STANDBY,
            START_DRILLING,
            DRILLING,
            STOPPING,
            START_RETRACTING,
            RETRACTING,
        }

        private State state;

        public Program()
        {
            settings = new Settings(Me);
            new ScriptDisplay(Me, Runtime);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            savedState = new SavedState(this);
            walker = buildWalker();
            constructionController = buildConstructionController();
            bracingLeg = buildBracingLeg();
            drillMotor = getDrillMotor();
            savedState.Parse(drillMotor.WorldMatrix.Translation);
            drillController = buildDrillController(drillMotor);
            speedTracker = new SpeedTracker(() => drillMotor.WorldMatrix.Translation);
            state = State.STANDBY;
            new ScriptDisplay(Me, Runtime);

            buildStatusDisplay();
            buildOreDisplay();
            buildPartsDisplay();
        }

        public Walker buildWalker()
        {
            string tagA = settings.ForwardLegTag;
            string tagB = settings.RearLegTag;
            string extenderTag = settings.ExtenderLegTag;
            IMyPistonBase pistonA = new BlockFinder<IMyPistonBase>(this).WithCustomData(tagA).Get();
            IMyShipMergeBlock mergeA = new BlockFinder<IMyShipMergeBlock>(this).WithCustomData(tagA).Get();
            IMyShipConnector connectorA = new BlockFinder<IMyShipConnector>(this).WithCustomData(tagA).Get();
            IMyPistonBase pistonB = new BlockFinder<IMyPistonBase>(this).WithCustomData(tagB).Get();
            IMyShipMergeBlock mergeB = new BlockFinder<IMyShipMergeBlock>(this).WithCustomData(tagB).Get();
            IMyShipConnector connectorB = new BlockFinder<IMyShipConnector>(this).WithCustomData(tagB).Get();
            IMyPistonBase pistonExtender = new BlockFinder<IMyPistonBase>(this).WithCustomData(extenderTag).Get();
            return new Walker(0.1f, new PistonAssembly(pistonA, connectorA, mergeA), new PistonAssembly(pistonB, connectorB, mergeB), pistonExtender);
        }

        public ConstructionController buildConstructionController()
        {
            List<IMyShipWelder> welders = new BlockFinder<IMyShipWelder>(this).InSameConstructAs(Me).GetAll();
            List<IMyShipGrinder> grinders = new BlockFinder<IMyShipGrinder>(this).InSameConstructAs(Me).GetAll();
            IMyPistonBase piston = new BlockFinder<IMyPistonBase>(this).WithCustomData(settings.ConstructionTag).Get();
            IMyConveyorSorter contructionSorter = new BlockFinder<IMyConveyorSorter>(this).InSameConstructAs(Me).WithCustomData(settings.BuildFilterTag).Get();
            IMyConveyorSorter decontructionSorter = new BlockFinder<IMyConveyorSorter>(this).InSameConstructAs(Me).WithCustomData(settings.DestructionFilterTag).Get();
            return new ConstructionController(piston, welders, grinders, contructionSorter, decontructionSorter);
        }

        public DrillController buildDrillController(IMyMotorAdvancedStator motor)
        {
            List<IMyShipDrill> drills = new BlockFinder<IMyShipDrill>(this).InSameConstructAs(Me).GetAll();
            return new DrillController(motor, drills);
        }

        public IMyMotorAdvancedStator getDrillMotor()
        {
            return new BlockFinder<IMyMotorAdvancedStator>(this).InSameConstructAs(Me).Get();
        }

        public BracingLeg buildBracingLeg()
        {
            string tag = settings.BraceLegTag;
            IMyLandingGear landingGear = new BlockFinder<IMyLandingGear>(this).WithCustomData(tag).Get();
            IMyPistonBase piston = new BlockFinder<IMyPistonBase>(this).WithCustomData(tag).Get();
            return new BracingLeg(landingGear, piston);
        }

        public Display buildStatusDisplay()
        {
            return new StatusDisplay(new BlockFinder<IMyTextPanel>(this).WithCustomData(settings.StatusDisplayTag).Get(), 36, 30)
                .WithCenteredLabel("Mining drill")
                .WithHorizontalLine()
                .WithRow("State", state.ToString)
                .WithRow("Depth", () => CurrentDepth().ToString("n2") + " m")
                .WithRow("Target depth", () => settings.DesiredDepth.ToString("n2") + " m")
                .WithRow("Average velocity", VelocityInfo)
                .WithHorizontalLine()
                .WithRow("Drill", drillController.Info)
                .WithRow("Walker", () => walker.GetState().ToString())
                .WithRow("Constructor", constructionController.Info)
                .WithRow("Step count", savedState.StepCount.ToString)
                .WithRow("Bracing leg", bracingLeg.Info)
                .Build();
        }

        public Display buildOreDisplay()
        {
            OreSummary oreSummary = new OreSummary(new BlockFinder<IMyTerminalBlock>(this).WithCustomPredicate(block => block is IMyCargoContainer).InSameConstructAs(Me).GetAll());
            IMyTextPanel textPanel = new BlockFinder<IMyTextPanel>(this).InSameConstructAs(Me).WithCustomData(settings.OreDisplaytag).Get();
            return new OreDisplay(textPanel, oreSummary.FloatAmounts)
                .Build();
        }

        public Display buildPartsDisplay()
        {
            ComponentSummary partSummary = new ComponentSummary(new BlockFinder<IMyTerminalBlock>(this).WithCustomPredicate(block => block is IMyCargoContainer).InSameConstructAs(Me).GetAll());
            IMyTextPanel textPanel = new BlockFinder<IMyTextPanel>(this).InSameConstructAs(Me).WithCustomData(settings.PartDisplaytag).Get();
            return new PartDisplay(textPanel, partSummary.FloatAmounts)
                .WithSubassemblyCounter(new PartDisplay.SubassemblyCounter.Builder("Sections")
                    .WithPart(Component.ComponentType.Motor, 28)
                    .WithPart(Component.ComponentType.SmallTube, 56)
                    .WithPart(Component.ComponentType.Construction, 125)
                    .WithPart(Component.ComponentType.InteriorPlate, 48)
                    .WithPart(Component.ComponentType.SteelPlate, 162)
                    .WithPart(Component.ComponentType.Computer, 22)
                    .WithPart(Component.ComponentType.LargeTube, 6)
                    .Build())
                .Build();
        }

        public void Save()
        {
            savedState.Save();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            RunArgument(argument);
            Update();
            walker.Update();
            constructionController.Update();
            bracingLeg.Update();
            Display.Render();
        }

        public void RunArgument(string argument)
        {
            if (argument == null || argument.Length == 0)
            {
                return;
            }

            switch(argument)
            {
                case "ATTACH BRACE":
                    bracingLeg.Connect();
                    break;
                case "DETACH BRACE":
                    bracingLeg.Disconnect();
                    break;
                case "START":
                    StartDrilling();
                    break;
                case "STOP":
                    StopDrilling();
                    break;
                case "RETRACT":
                    StartRetracting();
                    break;
                case "EMERGENCYSTOP":
                    walker.EmergencyShutdown();
                    drillController.Stop();
                    state = State.ERROR;
                    break;
                case "RESET":
                    savedState.Reset();
                    break;
                case "FORCESTEP":
                    savedState.StepCount = savedState.StepCount + 1;
                    break;
            }
        }

        public void StartDrilling()
        {
            if (state == State.STANDBY)
            {
                speedTracker.Reset();
                state = State.START_DRILLING;
                drillController.Start();
                constructionController.StartBuilding();
                if (savedState.StepCount == 0)
                {
                    savedState.StartingPosition = drillMotor.WorldMatrix.Translation;
                }
            }
        }

        public void StopDrilling()
        {
            if (state == State.DRILLING || state == State.RETRACTING)
            {
                state = State.STOPPING;
            }
        }

        public void StartRetracting()
        {
            if (state == State.STANDBY)
            {
                speedTracker.Reset();
                state = State.START_RETRACTING;
                constructionController.StartRemoving();

            }
        }

        public void Update()
        {
            switch(state)
            {
                case State.START_RETRACTING:
                    if (constructionController.GetState() == ConstructionController.State.REMOVING)
                    {
                        state = State.RETRACTING;
                    }
                    break;
                case State.RETRACTING:
                    if (walker.GetState() == Walker.State.STANDBY)
                    {
                        if (savedState.StepCount == 0)
                        {
                            state = State.STANDBY;
                        } else
                        {
                            walker.StepBackward();
                            --savedState.StepCount;
                        }
                    }
                    break;
                case State.DRILLING:
                    if (walker.GetState() == Walker.State.STANDBY)
                    {
                        if (CurrentDepth() > settings.DesiredDepth)
                        {
                            StopDrilling();
                        }
                        else
                        {
                            walker.StepForward();
                            ++savedState.StepCount;
                        }
                    }
                    break;
                case State.STOPPING:
                    if (walker.GetState() == Walker.State.STANDBY)
                    {
                        drillController.Stop();
                        constructionController.Stop();
                        state = State.STANDBY;
                    }
                    break;
                case State.START_DRILLING:
                    if (constructionController.GetState() == ConstructionController.State.BUILDING)
                    {
                        state = State.DRILLING;
                    }
                    break;
                case State.ERROR:
                case State.STANDBY:
                    // nothing to do
                    break;

            }
        }

        private double CurrentDepth()
        {
            if (savedState.StepCount == 0)
            {
                return 0.0;
            } else
            {
                return (savedState.StartingPosition - drillMotor.WorldMatrix.Translation).Length();
            }
        }

        private string VelocityInfo()
        {
            if (state == State.DRILLING || state == State.RETRACTING)
            {
                return speedTracker.AverageSpeed.ToString("n2") + " m/s";
            } 
            else
            {
                return "";
            }
        }
    }
}
