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
        SquareWalker squareWalker;
        RenderTest renderTest;
        
        int x, y;

        public Program()
        {
            scriptDisplay = new ScriptDisplay(Me, Runtime);
            IMyTextPanel selectionPanel = new BlockFinder<IMyTextPanel>(this).InSameConstructAs(Me).WithCustomData("LCD:TEST").Get();
            IMyTextPanel testPanel = new BlockFinder<IMyTextPanel>(this).InSameConstructAs(Me).WithCustomData("LCD:SPRITE").Get();

            //squareWalker = new SquareWalker(20, 20, selectionPanel, testPanel, scriptDisplay.Write);
            renderTest = new RenderTest(selectionPanel, testPanel);
            Runtime.UpdateFrequency = UpdateFrequency.Once;
        }

      

        public void Save()
        {
           
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (!String.IsNullOrEmpty(argument))
            {
                scriptDisplay.Write(argument);
                switch(argument)
                {
                    case "UP":
                        renderTest?.Up();
                        squareWalker?.Up();
                        break;
                    case "DOWN":
                        renderTest?.Down();
                        squareWalker?.Down();
                        break;
                    case "LEFT":
                        squareWalker?.Left();
                        break;
                    case "RIGHT":
                        squareWalker?.Right();
                        break;
                }
            }
            squareWalker?.Update();
            renderTest?.Update();
            Display.Render();
        }
    }
}
