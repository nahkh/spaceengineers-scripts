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
        public class SpeedTracker
        {
            private readonly Func<Vector3D> locationProvider;
            private Vector3D startPosition;
            private DateTime startTime;
            
            public SpeedTracker(Func<Vector3D> locationProvider)
            {
                this.locationProvider = locationProvider;
                Reset();
            }

            public void Reset()
            {
                startPosition = locationProvider.Invoke();
                startTime = DateTime.Now;
            }

            public double AverageSpeed
            {
                get
                {
                    Vector3D currentPosition = locationProvider.Invoke();
                    double distance = (currentPosition - startPosition).Length();
                    double seconds = (DateTime.Now - startTime).TotalSeconds;
                    return distance / seconds;
                }
            }
        }
    }
}
