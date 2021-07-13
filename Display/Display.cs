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
        public class Display
        {
            private static List<Display> displays = new List<Display>();
            private readonly IMyTextSurface textSurface;
            private readonly Func<List<string>> contentSource;

            public Display(IMyTextSurface textSurface, Func<List<string>> contentSource, float fontSize=0.7f)
            {
                this.textSurface = textSurface;
                this.contentSource = contentSource;
                textSurface.ContentType = ContentType.TEXT_AND_IMAGE;
                textSurface.FontColor = Color.Green;
                textSurface.FontSize = fontSize;
                textSurface.Font = "Monospace";
                displays.Add(this);
            }

            public static void Render()
            {
                displays.ForEach(RenderSingle);
            }

            private static void RenderSingle(Display display)
            {
                display.RenderDisplay();
            }

            public void RenderDisplay()
            {
                this.textSurface.WriteText(String.Join("\n", this.contentSource.Invoke()));
            }
        }
    }
}
