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
        public class Settings
        {
            MyIni _ini = new MyIni();

            public Settings(IMyProgrammableBlock programmableBlock)
            {
                MyIniParseResult result;
                if (!_ini.TryParse(programmableBlock.CustomData, out result))
                    throw new Exception(result.ToString());
                if (String.IsNullOrEmpty(programmableBlock.CustomData))
                {
                    _ini.AddSection("ConstructionArm");
                    _ini.Set("ConstructionArm", "ArmTag", "arm1");
                    _ini.SetComment("ConstructionArm", "ArmTag", "The string that must be present in the Custom Data section of every component of this particular construction arm");
                    _ini.Set("ConstructionArm", "DesiredVelocity", "2.5");
                    _ini.SetComment("ConstructionArm", "DesiredVelocity", "The desired velocity the arm head should move in the indicated direction, in meters per second");
                    _ini.Set("ConstructionArm", "MaxRadiansPerSecond", "5.0");
                    _ini.SetComment("ConstructionArm", "MaxRadiansPerSecond", "The maximum amount of radians per second that a rotor should move");
                    _ini.Set("ConstructionArm", "MaxAcceleration", "1.0");
                    _ini.SetComment("ConstructionArm", "MaxAcceleration", "The maximum amount of radians per second^2 that a rotor should accelerate");
                    _ini.Set("ConstructionArm", "IdleDelta", "0.001");
                    _ini.SetComment("ConstructionArm", "IdleDelta", "The maximum error between actual and desired position for calculating whether the arm is idle or not");

                    programmableBlock.CustomData = _ini.ToString();
                }
            }


            public string ArmTag
            {
                get
                {
                    return _ini.Get("ConstructionArm", "ArmTag").ToString();
                }
            }

            public float MaxRadians
            {
                get
                {
                    return Math.Abs((float) _ini.Get("ConstructionArm", "MaxRadiansPerSecond").ToDouble());
                }
            }

            public float MaxAcceleration
            {
                get
                {
                    return Math.Abs((float)_ini.Get("ConstructionArm", "MaxAcceleration").ToDouble());
                }
            }

            public float DesiredVelocity
            {
                get
                {
                    return Math.Abs((float)_ini.Get("ConstructionArm", "DesiredVelocity").ToDouble());
                }
            }


            public float IdleDelta
            {
                get
                {
                    return Math.Abs((float)_ini.Get("ConstructionArm", "IdleDelta").ToDouble());
                }
            }
        }
    }
}
