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
        public class DockingRequest
        {
            public static readonly string tag = "AUTODOCK-REQUEST";
            private readonly Vector3D position;
            private readonly Vector3D orientation;

            public DockingRequest(Vector3D position, Vector3D orientation)
            {
                this.position = position;
                this.orientation = orientation;
            }

            public Vector3D Position
            {
                get
                {
                    return position;
                }
            }
            public Vector3D Orientation
            {
                get
                {
                    return orientation;
                }
            }

            public override string ToString()
            {
                string output = "";
                output += position.X.ToString() + "/";
                output += position.Y.ToString() + "/";
                output += position.Z.ToString() + "/";
                output += orientation.X.ToString() + "/";
                output += orientation.Y.ToString() + "/";
                output += orientation.Z.ToString() + "/";
                return output;
            }

            public static DockingRequest Parse(string message)
            {
                string[] parts = message.Split('/');
                if (parts.Length < 6)
                {
                    return null;
                }

                double pX = double.Parse(parts[0]);
                double pY = double.Parse(parts[1]);
                double pZ = double.Parse(parts[2]);
                double oX = double.Parse(parts[3]);
                double oY = double.Parse(parts[4]);
                double oZ = double.Parse(parts[5]);
                return new DockingRequest(new Vector3D(pX, pY, pZ), new Vector3D(oX, oY, oZ));
            }
        }
    }
}
