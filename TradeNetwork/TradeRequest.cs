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
        public class TradeRequest
        {
            public const string TAG = "TradeRequest";
            private readonly string transactionId;
            private readonly long senderId;
            private readonly DateTime requestTime;
            private readonly TradeRequestItem[] tradeRequestItems;

            public TradeRequest(string transactionId, long senderId, DateTime requestTime, TradeRequestItem[] tradeRequestItems)
            {
                this.transactionId = transactionId;
                this.senderId = senderId;
                this.requestTime = requestTime;
                this.tradeRequestItems = tradeRequestItems;
            }

            public string TransactionId
            {
                get
                {
                    return transactionId;
                }
            }

            public long SenderId
            {
                get
                {
                    return senderId;
                }
            }

            public override string ToString()
            {
                string output = "";
                output += transactionId + "\n";
                output += senderId + "\n";
                output += requestTime.ToString() + "\n";
                foreach(TradeRequestItem item in tradeRequestItems)
                {
                    output += item.ToString() + "\n";
                }
                return output;
            }

            public static TradeRequest Parse(string serialized)
            {
                string[] rows = serialized.Split('\n');
                string transactionId = rows[0];
                long senderId = long.Parse(rows[1]);
                DateTime requestTime = DateTime.Parse(rows[2]);
                List<TradeRequestItem> items = new List<TradeRequestItem>();
                for (int i = 3; i < rows.Length; ++i)
                {
                    items.Add(TradeRequestItem.Parse(rows[i]));
                }
                return new TradeRequest(transactionId, senderId, requestTime, items.ToArray());
            }

            public class TradeRequestItem
            {
                private readonly TradeType type;
                private readonly TradeableItem tradeableItem;
                private readonly float amount;

                public enum TradeType
                {
                    WANT_TO_BUY,
                    WANT_TO_SELL,
                }

                public TradeRequestItem(TradeType type, TradeableItem tradeableItem, float amount)
                {
                    this.type = type;
                    this.tradeableItem = tradeableItem;
                    this.amount = amount;
                }

                public override string ToString()
                {
                    return type.ToString() + '/' + tradeableItem.ToString() + '/' + amount.ToString("n2");
                }

                public static TradeRequestItem Parse(string serialized)
                {
                    string[] elements = serialized.Split('/');
                    TradeType tradeType = FromString(elements[0]);
                    TradeableItem item = TradeableItem.Parse(elements[1]);
                    float amount = float.Parse(elements[2]);
                    return new TradeRequestItem(tradeType, item, amount);
                }

                private static TradeType FromString(string input)
                {
                    if (input == "WANT_TO_BUY")
                    {
                        return TradeType.WANT_TO_BUY;
                    } else
                    {
                        return TradeType.WANT_TO_SELL;
                    }
                }
            }

            public class Builder
            {
                private readonly string transactionId;
                private readonly long senderId;
                private readonly List<TradeRequestItem> tradeRequestItems;
                public Builder(Settings settings)
                {
                    transactionId = new Random().Next().ToString();
                    senderId = settings.MyId;
                    tradeRequestItems = new List<TradeRequestItem>();
                }

                public Builder WantToBuy(TradeableItem item, float amount)
                {
                    if (amount <= 0.0f)
                    {
                        throw new ArgumentException("Amount must be positive");
                    }

                    tradeRequestItems.Add(new TradeRequestItem(TradeRequestItem.TradeType.WANT_TO_BUY, item, amount));
                    return this;

                }
                public Builder WantToSell(TradeableItem item, float amount)
                {
                    if (amount <= 0.0f)
                    {
                        throw new ArgumentException("Amount must be positive");
                    }

                    tradeRequestItems.Add(new TradeRequestItem(TradeRequestItem.TradeType.WANT_TO_SELL, item, amount));
                    return this;
                }

                public TradeRequest Build()
                {
                    return new TradeRequest(transactionId, senderId, DateTime.Now, tradeRequestItems.ToArray());
                }
            }
        }
    }
}
