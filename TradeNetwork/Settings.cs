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
        public class Settings
        {
            private readonly MyIni _ini = new MyIni();
            private readonly Dictionary<long, string> knownTraders;

            public Settings(IMyProgrammableBlock programmableBlock, long id)
            {
                MyIniParseResult result;
                if (!_ini.TryParse(programmableBlock.CustomData, out result))
                    throw new Exception(result.ToString());

                if (String.IsNullOrEmpty(programmableBlock.CustomData))
                {
                    _ini.Set("TradeNetwork", "CommunicationTag", "TradeNetwork-Comms-Public");
                    _ini.SetComment("TradeNetwork", "CommunicationTag", "Comms tag to be used for broadcasts");
                    _ini.Set("TradeNetwork", "HostMarket", false);
                    _ini.SetComment("TradeNetwork", "HostMarket", "Indicates if you are acting as the market host. A market host sets prices and tracks trade requests");
                    _ini.Set("TradeNetwork", "HostMarketDisplay", "TradeNetwork-Host-Display");
                    _ini.SetComment("TradeNetwork", "HostMarketDisplay", "The LCD display that is used as part of the market host interface");
                    _ini.Set("TradeNetwork", "HostMarketContainer", "TradeNetwork-Host-Container");
                    _ini.SetComment("TradeNetwork", "HostMarketContainer", "The container(s) with items to trade");
                    _ini.Set("TradeNetwork", "IdleTimeSeconds", 180);
                    _ini.SetComment("TradeNetwork", "IdleTimeSeconds", "The amount of time until the UI returns to it's idle state");
                    _ini.Set("TradeNetwork", "UpdateInterval", 180);
                    _ini.SetComment("TradeNetwork", "UpdateInterval", "The amount of time between clients requesting an update");
                    _ini.AddSection("KnownTraders");
                    _ini.Set("KnownTraders", "Me", id);
                    _ini.Set("KnownTraders", "ExampleBuyer", 12345);
                    _ini.AddSection("BuyingPrices");
                    _ini.AddSection("SellingPrices");
                    foreach(TradeableItem item in TradeableItem.AllTradeableItems())
                    {
                        _ini.Set("BuyingPrices", item.ToString(),  1f);
                        _ini.Set("SellingPrices", item.ToString(), 1f);
                    }

                    programmableBlock.CustomData = _ini.ToString();
                }

                knownTraders = new Dictionary<long, string>();
                List<MyIniKey> keys = new List<MyIniKey>();
                _ini.GetKeys("KnownTraders", keys);
                foreach(MyIniKey key in keys)
                {
                    long traderId = _ini.Get(key).ToInt64();
                    knownTraders.Add(traderId, key.Name);
                }
            }

            public string CommsTag
            {
                get
                {
                    return _ini.Get("TradeNetwork", "CommunicationTag").ToString("TradeNetwork-Comms-Public");
                }
            }

            public bool HostMarket
            {
                get
                {
                    return _ini.Get("TradeNetwork", "HostMarket").ToBoolean(false);
                }
            }

            public string HostMarketDisplay
            {
                get
                {
                    return _ini.Get("TradeNetwork", "HostMarketDisplay").ToString("TradeNetwork-Host-Display");
                }
            }

            public string HostMarketContainers
            {
                get
                {
                    return _ini.Get("TradeNetwork", "HostMarketContainer").ToString("TradeNetwork-Host-Container");
                }
            }

            public TimeSpan IdleTime
            {
                get
                {
                    return TimeSpan.FromSeconds(_ini.Get("TradeNetwork", "IdleTimeSeconds").ToInt64(180));
                }
            }

            public TimeSpan UpdateInterval
            {
                get
                {
                    return TimeSpan.FromSeconds(_ini.Get("TradeNetwork", "UpdateIntervalSeconds").ToInt64(180));
                }
            }

            public long MyId
            {
                get
                {
                    return _ini.Get("KnownTraders", "Me").ToInt64();
                }
            }

            public bool KnownTrader(long id)
            {
                return knownTraders.ContainsKey(id);
            }

            public string TraderName(long id)
            {
                return knownTraders[id];
            }

            public float SellingPrice(TradeableItem item)
            {
                return (float)_ini.Get("SellingPrices", item.ToString()).ToDouble(float.MaxValue);
            }

            public float BuyingPrice(TradeableItem item)
            {
                return (float)_ini.Get("BuyingPrices", item.ToString()).ToDouble(float.MaxValue);
            }
        }
    }
}
