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
        public class ExtendingArm : ArmComponent
        {
            private readonly RotorController h1;
            private readonly RotorController h2;
            private readonly RotorController h3;
            private readonly RotorController h4;
            private readonly StackedPistons boom1;
            private readonly StackedPistons boom2;
            private readonly Action<string> logger;
            private List<ArmComponent> components;
            private float alpha, beta, gamma;

            public ExtendingArm(RotorController h1, RotorController h2, RotorController h3, RotorController h4, StackedPistons boom1, StackedPistons boom2, Action<string> logger) 
            {
                if (h1 == null)
                {
                    throw new ArgumentNullException(nameof(h1));
                }

                if (h2 == null)
                {
                    throw new ArgumentNullException(nameof(h2));
                }

                if (h3 == null)
                {
                    throw new ArgumentNullException(nameof(h3));
                }

                if (h4 == null)
                {
                    throw new ArgumentNullException(nameof(h4));
                }

                if (boom1 == null)
                {
                    throw new ArgumentNullException(nameof(boom1));
                }

                if (boom2 == null)
                {
                    throw new ArgumentNullException(nameof(boom2));
                }

                if (logger == null)
                {
                    throw new ArgumentNullException(nameof(logger));
                }

                this.h1 = h1;
                this.h2 = h2;
                this.h3 = h3;
                this.h4 = h4;
                this.boom1 = boom1;
                this.boom2 = boom2;
                this.logger = logger;
                components = new List<ArmComponent>();
                components.Add(h1);
                components.Add(h2);
                components.Add(h3);
                components.Add(h4);
                components.Add(boom1);
                components.Add(boom2);
                logger.Invoke("ExtendingArm");
                logger.Invoke("H1 " + h1.Angle.ToString("n2"));
                logger.Invoke("H2 " + h2.Angle.ToString("n2"));
                logger.Invoke("H3 " + h3.Angle.ToString("n2"));
                logger.Invoke("H4 " + h4.Angle.ToString("n2"));
            }

            public void emergencyStop()
            {
                foreach(ArmComponent component in components)
                {
                    component.emergencyStop();
                }
            }

            public ErrorReport getError()
            {
                ErrorReport report = ErrorReport.NONE;
                foreach (ArmComponent component in components)
                {
                    report = ErrorReport.merge(report, component.getError());
                }
                return report;
            }

            public string getStatus()
            {
                return "";
            }

            public bool isIdle()
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

            public bool isStowed()
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

            public void returnToIdle()
            {
                foreach (ArmComponent component in components)
                {
                    component.returnToIdle();
                }
            }

            public void stow()
            {
                foreach (ArmComponent component in components)
                {
                    component.stow();
                }
            }

            public void tick()
            {
                foreach (ArmComponent component in components)
                {
                    component.tick();
                }
            }
        }
    }
}
