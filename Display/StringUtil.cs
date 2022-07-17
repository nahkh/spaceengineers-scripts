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
        public class StringUtil
        {
            private StringUtil() { }

            public enum Alignment
            {
                LEFT,
                RIGHT,
                MIDDLE,
            }

            public static string Pad(string input, Alignment alignment, int desiredWidth)
            {
                if (input.Length > desiredWidth)
                {
                    return input.Substring(0, desiredWidth);
                }
                int spareWidth = desiredWidth - input.Length;
                switch (alignment)
                {
                    case Alignment.LEFT:
                        return input + new string(' ', spareWidth);
                    case Alignment.RIGHT:
                        return new string(' ', spareWidth) + input;
                    case Alignment.MIDDLE:
                        int left = spareWidth / 2;
                        int right = spareWidth - left;
                        return new string(' ', left) + input + new string(' ', right);
                    default:
                        throw new ArgumentException("Unknown alignment: " + alignment);
                }
            }
        }
    }
}
