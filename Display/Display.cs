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
            static List<Display> displays = new List<Display>();
            private readonly IMyTextSurface textSurface;
            private readonly Func<List<string>> contentSource;

            public Display(IMyTextSurface textSurface, Func<List<string>> contentSource)
            {
                this.textSurface = textSurface;
                this.contentSource = contentSource;
                textSurface.ContentType = ContentType.TEXT_AND_IMAGE;
                textSurface.FontColor = Color.Green;
                textSurface.FontSize = 0.7f;
                textSurface.Font = "Monospace";
                displays.Add(this);
            }

            public static void render()
            {
                displays.ForEach(renderSingle);
            }

            private static void renderSingle(Display display)
            {
                display.renderDisplay();
            }

            public void renderDisplay()
            {
                this.textSurface.WriteText(String.Join("\n", this.contentSource.Invoke()));
            }
        }
    }
}
