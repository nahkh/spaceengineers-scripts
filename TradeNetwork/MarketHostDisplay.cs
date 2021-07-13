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
        public class MarketHostDisplay
        {
            private readonly IMyTextSurface surface;
            private readonly Func<TradeableItem, MarketState.MarketStateItem> marketStateProvider;

            public enum Columns {
                SellPrice,
                BuyPrice,
                AmountInStorage,
            }

            public MarketHostDisplay(IMyTextSurface surface, Func<TradeableItem, MarketState.MarketStateItem> marketStateProvider)
            {
                this.surface = surface;
                this.marketStateProvider = marketStateProvider;

                new TableDisplay<TradeableItem, Columns>(surface, 36 * 2, 30)
                    .WithLabel("Market Host")
                    .WithData(Render)
                    .Column(Columns.SellPrice, 10)
                    .Column(Columns.BuyPrice, 10)
                    .Column(Columns.AmountInStorage, 20)
                    .Rows(TradeableItem.AllTradeableItems())
                    .Build();
            }

            private string Render(TradeableItem tradeableItem, Columns col)
            {
                MarketState.MarketStateItem stateItem = marketStateProvider.Invoke(tradeableItem);
                if (stateItem == null)
                {
                    return "N/A";
                }
                switch(col)
                {
                    case Columns.SellPrice:
                        return stateItem.SalePrice.ToString("n2");
                    case Columns.BuyPrice:
                        return stateItem.BuyPrice.ToString("n2");
                    case Columns.AmountInStorage:
                        return stateItem.AmountForSale.ToString("n2");
                    default:
                        throw new ArgumentException("Unknown column: " + col);
                }
            }
        }
    }
}
