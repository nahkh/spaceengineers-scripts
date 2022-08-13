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
        public class InputController
        {
            private readonly IMyCockpit cockpit;

            public InputController(IMyCockpit cockpit)
            {
                if (cockpit == null)
                {
                    throw new ArgumentNullException(nameof(cockpit));
                }

                this.cockpit = cockpit;
            }

            public UserInput getUserInput()
            {
                Vector3 moveIndicator = cockpit.MoveIndicator;
                return new UserInput(getXAxis(moveIndicator.Y), getYAxis(moveIndicator.X), getZAxis(moveIndicator.Z));
            }

            private UserInput.XAxis getXAxis(float value)
            {
                if (Math.Abs(value) < 0.1)
                {
                    return UserInput.XAxis.STILL;
                }
                return value > 0 ? UserInput.XAxis.FORWARD : UserInput.XAxis.BACKWARD;
            }


            private UserInput.YAxis getYAxis(float value)
            {
                if (Math.Abs(value) < 0.1)
                {
                    return UserInput.YAxis.STILL;
                }
                return value > 0 ? UserInput.YAxis.RIGHT : UserInput.YAxis.LEFT;
            }


            private UserInput.ZAxis getZAxis(float value)
            {
                if (Math.Abs(value) < 0.1)
                {
                    return UserInput.ZAxis.STILL;
                }
                return value > 0 ? UserInput.ZAxis.UP: UserInput.ZAxis.DOWN;
            }
        }
    }
}
