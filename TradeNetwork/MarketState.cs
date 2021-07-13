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
        public class MarketState
        {
            public const string UPDATE_REQUEST = "MarketState-Request";
            public const string UPDATE_RESPONSE = "MarketState-Response";
            private readonly DateTime dateTime;
            private readonly MarketStateItem[] marketStateItems;

            public MarketState(DateTime dateTime, MarketStateItem[] marketStateItems)
            {
                this.dateTime = dateTime;
                this.marketStateItems = marketStateItems;
            }

            public override string ToString()
            {
                string output = dateTime.ToString() + "\n";
                foreach(MarketStateItem item in marketStateItems)
                {
                    output += item.ToString() + "\n";
                }
                return output;
            }

            public DateTime CreationTime
            {
                get
                {
                    return dateTime;
                }
            }

            public static MarketState Parse(string serialized)
            {
                string[] parts = serialized.Split('\n');
                DateTime dateTime = DateTime.Parse(parts[0]);
                MarketStateItem[] items = new MarketStateItem[parts.Length - 1];
                for (int i = 1; i < parts.Length; ++i)
                {
                    items[i-1] = MarketStateItem.Parse(parts[i]);
                }
                return new MarketState(dateTime, items);
            }

            public MarketStateItem GetItem(TradeableItem item)
            {
                foreach(MarketStateItem marketStateItem in marketStateItems)
                {
                    if (item.Equals(marketStateItem.TradeableItem))
                    {
                        return marketStateItem;
                    }
                }
                return null;
            }

            public class MarketStateItem
            {
                private readonly TradeableItem item;
                private readonly float salePrice;
                private readonly float buyPrice;
                private readonly float amountForSale;

                public MarketStateItem(TradeableItem item, float salePrice, float buyPrice, float amountForSale)
                {
                    this.item = item;
                    this.salePrice = salePrice;
                    this.buyPrice = buyPrice;
                    this.amountForSale = amountForSale;
                }

                public override string ToString()
                {
                    return item.ToString() + '/' + salePrice + '/' + buyPrice + '/' + amountForSale;
                }
                
                public static MarketStateItem Parse(string serialized)
                {
                    string[] parts = serialized.Split('/');
                    TradeableItem item = TradeableItem.Parse(parts[0]);
                    float salePrice = float.Parse(parts[1]);
                    float buyPrice = float.Parse(parts[2]);
                    float amountForSale = float.Parse(parts[3]);
                    return new MarketStateItem(item, salePrice, buyPrice, amountForSale);
                }

                public TradeableItem TradeableItem
                {
                    get
                    {
                        return item;
                    }
                }

                public float SalePrice
                {
                    get
                    {
                        return salePrice;
                    }
                }

                public float BuyPrice
                {
                    get
                    {
                        return buyPrice;
                    }
                }

                public float AmountForSale
                {
                    get
                    {
                        return amountForSale;
                    }
                }
                
            }
          
        }
    }
}
