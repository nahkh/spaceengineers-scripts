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
        public class ArmAssemblyA : ArmComponent
        {
            private readonly RotorController headController;
            private readonly RotorController baseController;
            private readonly RotorController hinge1;
            private readonly RotorController hinge2;
            private readonly RotorController hinge3;
            private readonly RotorController hinge4;
            private readonly StackedPistons boom1;
            private readonly StackedPistons boom2;
            private readonly List<ArmComponent> components;

            public ArmAssemblyA(RotorController headController, RotorController baseController, RotorController hinge1, RotorController hinge2, RotorController hinge3, RotorController hinge4, StackedPistons boom1, StackedPistons boom2)
            {
                if (headController == null)
                {
                    throw new ArgumentNullException(nameof(headController));
                }

                if (baseController == null)
                {
                    throw new ArgumentNullException(nameof(baseController));
                }

                if (hinge1 == null)
                {
                    throw new ArgumentNullException(nameof(hinge1));
                }

                if (hinge2 == null)
                {
                    throw new ArgumentNullException(nameof(hinge2));
                }

                if (hinge3 == null)
                {
                    throw new ArgumentNullException(nameof(hinge3));
                }

                if (hinge4 == null)
                {
                    throw new ArgumentNullException(nameof(hinge4));
                }

                if (boom1 == null)
                {
                    throw new ArgumentNullException(nameof(boom1));
                }

                if (boom2 == null)
                {
                    throw new ArgumentNullException(nameof(boom2));
                }

                this.headController = headController;
                this.baseController = baseController;
                this.hinge1 = hinge1;
                this.hinge2 = hinge2;
                this.hinge3 = hinge3;
                this.hinge4 = hinge4;
                this.boom1 = boom1;
                this.boom2 = boom2;
                components = new List<ArmComponent>();
                components.Add(baseController);
                components.Add(headController);
                components.Add(hinge1);
                components.Add(hinge2);
                components.Add(hinge3);
                components.Add(hinge4);
                components.Add(boom1);
                components.Add(boom2);
            }

            void ArmComponent.emergencyStop()
            {
                foreach(ArmComponent component in components)
                {
                    component.emergencyStop();
                }
            }

            ErrorReport ErrorReporter.getError()
            {
                ErrorReport report = ErrorReport.NONE;
                foreach (ArmComponent component in components)
                {
                    report = ErrorReport.merge(report, component.getError());
                }
                return report;
            }

            string ArmComponent.getStatus()
            {
                return "";
            }

            bool ArmComponent.isIdle()
            {
                foreach (ArmComponent component in components)
                {
                    if (!component.isIdle())
                    {
                        return false;
                    }
                }
                return true;
            }

            bool ArmComponent.isStowed()
            {
                foreach (ArmComponent component in components)
                {
                    if (!component.isStowed())
                    {
                        return false;
                    }
                }
                return true;
            }

            void ArmComponent.returnToIdle()
            {
                foreach (ArmComponent component in components)
                {
                    component.returnToIdle();
                }
            }

            void ArmComponent.stow()
            {
                foreach (ArmComponent component in components)
                {
                    component.stow();
                }
            }

            void ArmComponent.tick()
            {
                headController.setDesiredAngle(baseController.Angle);
                hinge2.setDesiredAngle((float)-Math.PI / 2 - hinge1.Angle);
                if (hinge1.Angle > 0)
                {
                    hinge4.setDesiredAngle(hinge1.Angle);
                } else
                {
                    hinge4.setDesiredAngle((float)Math.PI / 2 + hinge3.Angle);
                }
                foreach (ArmComponent component in components)
                {
                    component.tick();
                }
            }

            public void handleUserInput(UserInput input)
            {
                if (input.IsIdle)
                {
                    return;
                }

                Vector3D headPosition = headController.Position;
                Vector3D direction = Vector3D.Zero;

                switch (input.yAxis)
                {
                    case UserInput.YAxis.LEFT:
                        direction += baseController.Left;
                        break;
                    case UserInput.YAxis.RIGHT:
                        direction -= baseController.Left;
                        break;
                    case UserInput.YAxis.STILL:
                        break;
                }


                switch (input.xAxis)
                {
                    case UserInput.XAxis.FORWARD:
                        direction += baseController.Forward;
                        break;
                    case UserInput.XAxis.BACKWARD:
                        direction -= baseController.Forward;
                        break;
                    case UserInput.XAxis.STILL:
                        break;
                }

                switch (input.zAxis)
                {
                    case UserInput.ZAxis.UP:
                        direction += baseController.Up;
                        break;
                    case UserInput.ZAxis.DOWN:
                        direction -= baseController.Up;
                        break;
                    case UserInput.ZAxis.STILL:
                        break;
                }

                direction.Normalize();
                // TODO figure out the correct multiplier for direction per tick.

                Vector3D targetPosition = headPosition + direction;
                
            }
        }
    }
}
