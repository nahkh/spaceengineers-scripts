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
        public class LogDisplay
        {
            private readonly IMyTextSurface surface;
            private readonly int maxLines;
            private readonly int maxLineLength;
            private readonly List<string> messages;
            private readonly ClockAnimation animation;

            public LogDisplay(IMyTextSurface surface, int maxLines, int maxLineLength, bool showTimeIndicator = true)
            {
                this.surface = surface;
                this.maxLines = maxLines;
                this.maxLineLength = maxLineLength;
                if (showTimeIndicator)
                {
                    animation = new ClockAnimation();
                }
                messages = new List<string>();
                new Display(surface, Render, fontSize:1.5f);
            }
            public void Log(string message)
            {
                message = DateTime.Now.ToString("HH:mm") + " " + message;
                while (message.Length > maxLineLength)
                {
                    messages.Add(message.Substring(0, maxLineLength));
                    message = message.Substring(maxLineLength);
                }
                messages.Add(message);
                
                while(messages.Count > maxLines)
                {
                    messages.RemoveAt(0);
                }
            }


            private List<string> Render()
            {
                if (animation != null)
                {
                    List<string> output = new List<string>(messages);
                    output.Add(animation.Get());
                    return output;
                } else
                {
                    return messages;
                }
            }
        }
    }
}
