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
        private readonly ScriptDisplay scriptDisplay;
        private readonly NavigatorDisplay navigator;

        public Program()
        {
            scriptDisplay = new ScriptDisplay(Me, Runtime, name: "ShipNavigator", version: "0.0.1");
            IMyCockpit cockpit = new BlockFinder<IMyCockpit>(this).InSameConstructAs(Me).Get();
            navigator = new NavigatorDisplay(cockpit.GetSurface(1), new Navigator(cockpit), maxDeviation: 90);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (!String.IsNullOrEmpty(argument))
            {
                Vector3D? target = ParseVector(argument);
                if (target.HasValue)
                {
                    navigator.NavigateTo(target.Value);
                }
            }
            navigator.Update();
            Display.Render();
        }

        private Vector3D? ParseVector(string argument)
        {
            if (!argument.StartsWith("GPS:"))
            {
                return null;
            }
            string[] parts = argument.Split(':');
            double x = double.Parse(parts[2]);
            double y = double.Parse(parts[3]);
            double z = double.Parse(parts[4]);
            return new Vector3D(x, y, z);
        }
    }
}
