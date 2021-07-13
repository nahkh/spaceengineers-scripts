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
        public class MarketHost
        {
            private static readonly TimeSpan STATE_UPDATE_INTERVAL = TimeSpan.FromSeconds(60);
            private const char STORAGE_PRIMARY_DIVIDER = '%';
            private const char STORAGE_SECONDARY_DIVIDER = '~';

            private readonly IMyIntergridCommunicationSystem comms;
            private readonly Program program;
            private readonly Settings settings;
            private readonly Action<string> logger;
            private readonly List<IMyCargoContainer> containers;
            private MarketState marketState;
            private List<TradeRequest> recordedRequests;
            private List<TradeRequest> completedRequests;

            public MarketHost(Program program, Settings settings, Action<string> logger, List<IMyCargoContainer> containers, IMyTextSurface stateDisplay)
            {
                comms = program.IGC;
                this.program = program;
                this.settings = settings;
                this.logger = logger;
                this.containers = containers;
                recordedRequests = new List<TradeRequest>();
                completedRequests = new List<TradeRequest>();
                new MarketHostDisplay(stateDisplay, type => marketState.GetItem(type));
            }

            public void Update()
            {
                if (marketState == null || marketState.CreationTime + STATE_UPDATE_INTERVAL < DateTime.Now)
                {
                    UpdateMarketState();
                }
            }

            public void ProcessMarketStateUpdateRequest(long requesterAddress)
            {
                if (!settings.KnownTrader(requesterAddress))
                {
                    logger.Invoke("Rejecting market state request, unknown trader: " + requesterAddress);
                    return;
                }
                if (marketState == null)
                {
                    logger.Invoke("Rejecting market state request, no state");
                    return;
                }

                comms.SendUnicastMessage(requesterAddress, MarketState.UPDATE_RESPONSE, marketState.ToString());
            }
            
            public void RecordTradeRequest(TradeRequest tradeRequest)
            {
                if (!settings.KnownTrader(tradeRequest.SenderId))
                {
                    logger.Invoke("Dropping request, unknown trader: " + tradeRequest.SenderId);
                    return;
                }
                recordedRequests.Add(tradeRequest);
            }

            public void CompleteRequest(string transactionId)
            {
                TradeRequest request = recordedRequests.Find(req => req.TransactionId == transactionId);
                if (request != null)
                {
                    recordedRequests.Remove(request);
                    completedRequests.Add(request);
                }
            }

            public void Save()
            {
                string storage = "";
                if (marketState != null)
                {
                    storage += marketState.ToString();
                } else
                {
                    storage += "null";
                }
                storage += STORAGE_PRIMARY_DIVIDER;
                
                for (int i = 0; i < recordedRequests.Count; ++i)
                {
                    if (i > 0)
                    {
                        storage += STORAGE_SECONDARY_DIVIDER;
                    }
                    storage += recordedRequests[i].ToString();
                }

                storage += STORAGE_PRIMARY_DIVIDER;
                for (int i = 0; i < completedRequests.Count; ++i)
                {
                    if (i > 0)
                    {
                        storage += STORAGE_SECONDARY_DIVIDER;
                    }
                    storage += completedRequests[i].ToString();
                }

                program.Storage = storage;
            }

            public void Load()
            {
                try
                {
                    if (String.IsNullOrEmpty(program.Storage))
                    {
                        return;
                    }
                    string[] primaryBlocks = program.Storage.Split(STORAGE_PRIMARY_DIVIDER);
                    if (primaryBlocks.Length > 0)
                    {
                        if (primaryBlocks[0] == "null")
                        {
                            marketState = null;
                        }
                        else
                        {
                            marketState = MarketState.Parse(primaryBlocks[0]);
                        }
                    }
                    if (primaryBlocks.Length > 1)
                    {
                        string[] recordedRequestBlocks = primaryBlocks[1].Split(STORAGE_SECONDARY_DIVIDER);
                        foreach (string requestBlock in recordedRequestBlocks)
                        {
                            recordedRequests.Add(TradeRequest.Parse(requestBlock));
                        }
                    }
                    if (primaryBlocks.Length > 2)
                    {
                        string[] completedRequestBlocks = primaryBlocks[2].Split(STORAGE_SECONDARY_DIVIDER);
                        foreach (string requestBlock in completedRequestBlocks)
                        {
                            completedRequests.Add(TradeRequest.Parse(requestBlock));
                        }
                    }
                } catch (Exception e)
                {
                    logger.Invoke("Could not parse stored data: " + e.Message);
                }
            }

            public void UpdateMarketState()
            {
                Dictionary<TradeableItem, float> itemCount = new Dictionary<TradeableItem, float>();
                List<TradeableItem> allTradeableItems = TradeableItem.AllTradeableItems();
                foreach (IMyCargoContainer container in containers)
                {
                    foreach(TradeableItem item in allTradeableItems)
                    {
                        float amount = container.GetInventory(0).GetItemAmount(item.ToItemType()).RawValue / 1000000f;
                        itemCount[item] = itemCount.GetValueOrDefault(item, 0) + amount;
                    }
                }
                List<MarketState.MarketStateItem> items = new List<MarketState.MarketStateItem>();
                foreach(TradeableItem item in allTradeableItems)
                {
                    items.Add(new MarketState.MarketStateItem(item, settings.SellingPrice(item), settings.BuyingPrice(item), itemCount[item]));
                }

                marketState = new MarketState(DateTime.Now, items.ToArray());
            }
        }
    }
}
