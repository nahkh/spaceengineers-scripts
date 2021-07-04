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
        ScriptDisplay scriptDisplay;
        int currentIndex;
        List<string> sprites;
        IMyTextPanel selectionPanel;
        IMyTextPanel testPanel;

        public Program()
        {
            scriptDisplay = new ScriptDisplay(Me, Runtime);
            selectionPanel = new BlockFinder<IMyTextPanel>(this).inSameConstructAs(Me).withCustomData("LCD:TEST").get();
            sprites = new List<string>();
            selectionPanel.GetSprites(sprites);
            selectionPanel.ContentType = ContentType.TEXT_AND_IMAGE;
            selectionPanel.FontColor = Color.Green;
            selectionPanel.FontSize = 0.7f;
            selectionPanel.Font = "Monospace";
            testPanel = new BlockFinder<IMyTextPanel>(this).inSameConstructAs(Me).withCustomData("LCD:SPRITE").get();
            testPanel.ContentType = ContentType.SCRIPT;
            Runtime.UpdateFrequency = UpdateFrequency.Once;
        }

        public void RenderSelection()
        {
            string content = "";
            for (int i = currentIndex - 10; i < currentIndex + 10; ++i)
            {
                if (i == currentIndex)
                {
                    content += "> ";
                }
                content += sprites[(i + sprites.Count) % sprites.Count] + "\n";
            }
            selectionPanel.WriteText(content);
        }

        public void RenderSprite()
        {
            
            MySpriteDrawFrame frame = testPanel.DrawFrame();
            Vector2 textureSize = testPanel.TextureSize;
            MySprite sprite = MySprite.CreateSprite(sprites[currentIndex], new Vector2(textureSize.X/2, textureSize.Y/2), textureSize);

            sprite.Color = new Color(0xFF0000FF);
            //sprite.Position = testPanel.TextureSize / 2f + (new Vector2(50f) / 2f);
            frame.Add(sprite);
            frame.Dispose();
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument == "UP")
            {
                currentIndex = (currentIndex - 1 + sprites.Count) % sprites.Count;
            }
            if (argument == "DOWN")
            {
                currentIndex = (currentIndex + 1) % sprites.Count;
            }

            RenderSelection();
            RenderSprite();
            Display.render();
        }
    }
}
