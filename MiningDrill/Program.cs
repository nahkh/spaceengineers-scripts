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
            IMyPistonBase pistonA = new BlockFinder<IMyPistonBase>(this).withCustomData(tagA).get();
            IMyShipMergeBlock mergeA = new BlockFinder<IMyShipMergeBlock>(this).withCustomData(tagA).get();
            IMyShipConnector connectorA = new BlockFinder<IMyShipConnector>(this).withCustomData(tagA).get();
            IMyPistonBase pistonB = new BlockFinder<IMyPistonBase>(this).withCustomData(tagB).get();
            IMyShipMergeBlock mergeB = new BlockFinder<IMyShipMergeBlock>(this).withCustomData(tagB).get();
            IMyShipConnector connectorB = new BlockFinder<IMyShipConnector>(this).withCustomData(tagB).get();
            IMyPistonBase pistonExtender = new BlockFinder<IMyPistonBase>(this).withCustomData(extenderTag).get();
            return new Walker(0.1f, new PistonAssembly(pistonA, connectorA, mergeA), new PistonAssembly(pistonB, connectorB, mergeB), pistonExtender);
        }

        public ConstructionController buildConstructionController()
        {
            List<IMyShipWelder> welders = new BlockFinder<IMyShipWelder>(this).inSameConstructAs(Me).getAll();
            List<IMyShipGrinder> grinders = new BlockFinder<IMyShipGrinder>(this).inSameConstructAs(Me).getAll();
            IMyPistonBase piston = new BlockFinder<IMyPistonBase>(this).withCustomData(settings.ConstructionTag).get();
            IMyConveyorSorter contructionSorter = new BlockFinder<IMyConveyorSorter>(this).inSameConstructAs(Me).withCustomData(settings.BuildFilterTag).get();
            IMyConveyorSorter decontructionSorter = new BlockFinder<IMyConveyorSorter>(this).inSameConstructAs(Me).withCustomData(settings.DestructionFilterTag).get();
            return new ConstructionController(piston, welders, grinders, contructionSorter, decontructionSorter);
        }

        public DrillController buildDrillController(IMyMotorAdvancedStator motor)
        {
            List<IMyShipDrill> drills = new BlockFinder<IMyShipDrill>(this).inSameConstructAs(Me).getAll();
            return new DrillController(motor, drills);
        }

        public IMyMotorAdvancedStator getDrillMotor()
        {
            return new BlockFinder<IMyMotorAdvancedStator>(this).inSameConstructAs(Me).get();
        }

        public BracingLeg buildBracingLeg()
        {
            string tag = settings.BraceLegTag;
            IMyLandingGear landingGear = new BlockFinder<IMyLandingGear>(this).withCustomData(tag).get();
            IMyPistonBase piston = new BlockFinder<IMyPistonBase>(this).withCustomData(tag).get();
            return new BracingLeg(landingGear, piston);
        }

        public Display buildStatusDisplay()
        {
            return new StatusDisplay(new BlockFinder<IMyTextPanel>(this).withCustomData(settings.StatusDisplayTag).get(), 36, 30)
                .withCenteredLabel("Mining drill")
                .withHorizontalLine()
                .withRow("State", () => state.ToString())
                .withRow("Depth", () => CurrentDepth().ToString("n2") + " m")
                .withRow("Target depth", () => settings.DesiredDepth.ToString("n2") + " m")
                .withRow("Average velocity", VelocityInfo)
                .withHorizontalLine()
                .withRow("Drill", () => drillController.Info())
                .withRow("Walker", () => walker.GetState().ToString())
                .withRow("Constructor", () => constructionController.Info())
                .withRow("Step count", () => savedState.StepCount.ToString())
                .withRow("Bracing leg", () => bracingLeg.Info())
                .build();
        }

        public Display buildOreDisplay()
        {
            OreSummary oreSummary = new OreSummary(new BlockFinder<IMyTerminalBlock>(this).withCustomPredicate(block => block is IMyCargoContainer).inSameConstructAs(Me).getAll());
            IMyTextPanel textPanel = new BlockFinder<IMyTextPanel>(this).inSameConstructAs(Me).withCustomData(settings.OreDisplaytag).get();
            return new OreDisplay(textPanel, oreSummary.Amounts)
                .Build();
        }

        public Display buildPartsDisplay()
        {
            PartSummary partSummary = new PartSummary(new BlockFinder<IMyTerminalBlock>(this).withCustomPredicate(block => block is IMyCargoContainer).inSameConstructAs(Me).getAll());
            IMyTextPanel textPanel = new BlockFinder<IMyTextPanel>(this).inSameConstructAs(Me).withCustomData(settings.PartDisplaytag).get();
            return new PartDisplay(textPanel, partSummary.Amounts)
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
            Display.render();
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
