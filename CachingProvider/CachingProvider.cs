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
        public class CachingProvider<T>
        {
            private readonly Func<T> provider;
            private readonly TimeSpan ttl;
            private DateTime lastRefresh;
            private T cachedValue;

            public CachingProvider(Func<T> provider, TimeSpan ttl)
            {
                this.provider = provider;
                this.ttl = ttl;
                lastRefresh = DateTime.MinValue;
            }

            public T Get()
            {
                if (lastRefresh + ttl < DateTime.Now)
                {
                    cachedValue = provider.Invoke();
                    lastRefresh = DateTime.Now;
                }
                return cachedValue;
            }

            public static Func<T> Of(Func<T> provider) {
                return new CachingProvider<T>(provider, TimeSpan.FromMinutes(1)).Get;
            }
            public static Func<T> Of(T rawValue)
            {
                return () => rawValue;
            }
        }
    }
}
