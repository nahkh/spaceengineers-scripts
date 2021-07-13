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
        public class TradeClient
        {
            private readonly Program program;
            private readonly Settings settings;
            private readonly IMyIntergridCommunicationSystem comms;
            IMyBroadcastListener marketListener;

            State state;
            private long hostId;

            public enum State
            {
                NO_HOST,
                CONNECTED,
            }

            public TradeClient(Program program, Settings settings)
            {
                this.program = program;
                this.settings = settings;
                comms = program.IGC;
                state = State.NO_HOST;
                marketListener = comms.RegisterBroadcastListener(settings.CommsTag);
            }

            public void Update()
            {
                while(marketListener.HasPendingMessage)
                {
                    MyIGCMessage message = marketListener.AcceptMessage();
                    if (state == State.NO_HOST)
                    {
                        state = State.CONNECTED;
                        hostId = message.Source;
                    }
                }
            }

            public bool Connected
            {
                get
                {
                    return state == State.CONNECTED;
                }
            }

            public bool SendTradeRequest(TradeRequest req)
            {
                if (state == State.CONNECTED)
                {
                    comms.SendUnicastMessage(hostId, TradeRequest.TAG, req.ToString());
                    return true;
                } else
                {
                    return false;
                }
            }

            public bool RequestMarketState()
            {
                if (state == State.CONNECTED)
                {
                    comms.SendUnicastMessage(hostId, MarketState.UPDATE_REQUEST, "");
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Save()
            {

            }
        }
    }
}
