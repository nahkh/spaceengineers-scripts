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

        private static readonly string CALLBACK_NAME = "UNICAST";
        private ScriptDisplay scriptDisplay;
        private Settings settings;
        private MarketHost marketHost;
        private TradeClient tradeClient;
        private MarketState marketState;

        public Program()
        {
            settings = new Settings(Me, IGC.Me);
            scriptDisplay = new ScriptDisplay(Me, Runtime);
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            IGC.UnicastListener.SetMessageCallback(CALLBACK_NAME);

            if (settings.HostMarket)
            {
                marketHost = new MarketHost(
                    this, 
                    settings, 
                    scriptDisplay.Write, 
                    new BlockFinder<IMyCargoContainer>(this)
                        .WithCustomData(settings.HostMarketContainers)
                        .GetAll(), 
                    new BlockFinder<IMyTextPanel>(this)
                        .WithCustomData(settings.HostMarketDisplay)
                        .Get());
                marketHost.Load();
            } else
            {
                tradeClient = new TradeClient(this, settings);
            }
        }

        public void Save()
        {
            marketHost?.Save();
            tradeClient?.Save();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (argument == CALLBACK_NAME)
            {
                ProcessMessages();
            }
            marketHost?.Update();
            tradeClient?.Update();
            Display.Render();
        }

        private void ProcessMessages()
        {
            while(IGC.UnicastListener.HasPendingMessage)
            {
                MyIGCMessage message = IGC.UnicastListener.AcceptMessage();
                switch(message.Tag)
                {
                    case TradeRequest.TAG:
                        TradeRequest request = TradeRequest.Parse(message.As<string>());
                        marketHost?.RecordTradeRequest(request);
                        break;
                    case MarketState.UPDATE_RESPONSE:
                        marketState = MarketState.Parse(message.As<string>());
                        break;
                    case MarketState.UPDATE_REQUEST:
                        marketHost?.ProcessMarketStateUpdateRequest(message.Source);
                        break;
                }
            }
        }
    }
}
