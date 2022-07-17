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
        public class CachingProvider
        {
            public class Cache<T>
            {
                private readonly Func<T> provider;
                private readonly TimeSpan ttl;
                private DateTime lastRefresh;
                private T cachedValue;

                public Cache(Func<T> provider, TimeSpan ttl)
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

                public void Invalidate()
                {
                    lastRefresh = DateTime.MinValue;
                }
            }

            public static Func<T> Of<T>(Func<T> provider) {
                return Of(provider, TimeSpan.FromMinutes(1));
            }

            public static Func<T> Of<T>(Func<T> provider, TimeSpan ttl)
            {
                return new Cache<T>(provider, ttl).Get;
            }

            public static Func<T> Of<T>(T rawValue)
            {
                return () => rawValue;
            }
        }
    }
}
