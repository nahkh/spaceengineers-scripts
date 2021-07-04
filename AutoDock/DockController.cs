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
        public class DockController
        {
            private static readonly double epsilon = 0.01;
            private readonly IMyShipConnector connector;
            private readonly List<IMyMotorAdvancedStator> hinges;
            private readonly List<IMyPistonBase> perpendicularPistons;
            private readonly List<IMyPistonBase> extendingPistons;

            private Vector3D target;
            private Vector3D targetAlignment;
            
            enum State
            {
                IDLE,
                ALIGNING_ORIENTATION,
                ALIGNING_PERPENDICULAR,
                EXTENDING,
                CONNECTED,
                RETRACTING,
                RETURNING_TO_NEUTRAL,
            }

            private State state;

            public DockController(IMyShipConnector connector, List<IMyPistonBase> perpendicularPistons, List<IMyPistonBase> extendingPistons, List<IMyMotorAdvancedStator> hinges)
            {
                this.connector = connector;
                this.hinges = hinges;
                this.perpendicularPistons = perpendicularPistons;
                this.extendingPistons = extendingPistons;
                state = connector.Status == MyShipConnectorStatus.Connected ? State.CONNECTED : State.IDLE;
            }

            public void Update()
            {
                switch(state)
                {
                    case State.IDLE:
                        break;
                    case State.ALIGNING_ORIENTATION:
                        AlignConnector();
                        break;
                    case State.ALIGNING_PERPENDICULAR:
                        AlignPerpendicular();
                        break;
                    case State.EXTENDING:
                        Extend();
                        break;
                    case State.CONNECTED:
                        CheckConnected();
                        break;
                    case State.RETRACTING:
                        Retract();
                        break;
                    case State.RETURNING_TO_NEUTRAL:
                        ReturnToNeutral();
                        break;
                }
            }


            private void AlignConnector()
            {
                bool allAligned = true;
                foreach (IMyMotorAdvancedStator hinge in hinges)
                {
                    Vector3D up = hinge.WorldMatrix.Up;
                    Vector3D freePlaneProjection = Vector3D.ProjectOnPlane(ref targetAlignment, ref up);
                    freePlaneProjection.Normalize();
                    double angle = freePlaneProjection.Dot(hinge.WorldMatrix.Forward);
                    if (Math.Abs(angle - hinge.Angle) < epsilon)
                    {
                        hinge.TargetVelocityRPM = 0f;
                    } else
                    {
                        allAligned = false;
                        if (angle - hinge.Angle < 0)
                        {
                            hinge.TargetVelocityRPM = -1;
                        } else
                        {
                            hinge.TargetVelocityRPM = 1;
                        }
                    }
                }

                if (allAligned)
                {
                    state = State.ALIGNING_PERPENDICULAR;
                }
            }

            private void AlignPerpendicular()
            {
                Vector3D directionToTarget = target - connector.WorldMatrix.Translation;
                bool allAligned = true;
                bool allMaxed = true;

                foreach(IMyPistonBase piston in perpendicularPistons)
                {
                    double dotProduct = piston.WorldMatrix.Up.Dot(directionToTarget);
                    if (Math.Abs(dotProduct) < epsilon)
                    {
                        piston.Velocity = 0;
                    } else
                    {
                        allAligned = false;
                        if (dotProduct > 0)
                        {
                            if (Math.Abs(piston.CurrentPosition - piston.MaxLimit) > epsilon)
                            {
                                allMaxed = false;
                            }
                            if (dotProduct < 1)
                            {
                                piston.Velocity = 0.05f;
                            }
                            else
                            {
                                piston.Velocity = 0.5f;
                            }
                        } else
                        {
                            if (Math.Abs(piston.CurrentPosition - piston.MinLimit) > epsilon)
                            {
                                allMaxed = false;
                            }
                            if (dotProduct > -1)
                            {
                                piston.Velocity = -0.05f;
                            }
                            else
                            {
                                piston.Velocity = -0.5f;
                            }
                        }
                        
                        
                    }
                }

                if (allAligned)
                {
                    state = State.EXTENDING;
                } else if (allMaxed)
                {
                    state = State.RETURNING_TO_NEUTRAL;
                }
            }

            private void Extend()
            {
                if (connector.Status == MyShipConnectorStatus.Unconnected)
                {
                    bool allMaxed = true;
                    foreach (IMyPistonBase piston in extendingPistons)
                    {
                        if (Math.Abs(piston.CurrentPosition - piston.MaxLimit) > epsilon)
                        {
                            allMaxed = false;
                        }
                        piston.Velocity = 0.3f;
                    }
                    if (allMaxed)
                    {
                        state = State.RETRACTING;
                    }
                } else
                {
                    foreach (IMyPistonBase piston in extendingPistons)
                    {
                        piston.Velocity = 0f;
                    }
                    if (connector.Status == MyShipConnectorStatus.Connectable)
                    {
                        connector.Connect();
                    } 
                    if (connector.Status == MyShipConnectorStatus.Connected)
                    {
                        state = State.CONNECTED;
                    }
                }
            }

            private void CheckConnected()
            {
                if (connector.Status != MyShipConnectorStatus.Connected)
                {
                    state = State.RETRACTING;
                }
            }

            private void Retract()
            {
                bool allMaxed = true;
                foreach (IMyPistonBase piston in extendingPistons)
                {
                    if (Math.Abs(piston.CurrentPosition - piston.MinLimit) > epsilon)
                    {
                        allMaxed = false;
                    }
                    piston.Velocity = -0.5f;
                }
                if (allMaxed)
                {
                    state = State.RETURNING_TO_NEUTRAL;
                }
            }

            private void ReturnToNeutral()
            {
                bool allMaxed = true;
                foreach (IMyPistonBase piston in perpendicularPistons)
                {
                    if (Math.Abs(piston.CurrentPosition - piston.MinLimit) > epsilon)
                    {
                        allMaxed = false;
                    }
                    piston.Velocity = -0.5f;
                }
                foreach(IMyMotorAdvancedStator hinge in hinges)
                {
                    if (Math.Abs(hinge.Angle) > epsilon)
                    {
                        allMaxed = false;
                        hinge.TargetVelocityRPM = -hinge.Angle * 5;
                    } else
                    {
                        hinge.TargetVelocityRPM = 0;
                    }
                }
                if (allMaxed)
                {
                    state = State.IDLE;
                }
            }

            public void ConnectTo(Vector3D target, Vector3D orientation)
            {
                if (state == State.IDLE) {
                    state = State.ALIGNING_ORIENTATION;
                    this.target = target;
                    targetAlignment = orientation;
                }
            }
            
            public void Disconnect()
            {
                if (state == State.CONNECTED)
                {
                    state = State.RETRACTING;
                }
            }

            public string StateInfo()
            {
                return state.ToString();
            }

            public string PistonCount()
            {
                return perpendicularPistons.Count.ToString() + "/" + extendingPistons.Count.ToString();
            }
        }
    }
}
