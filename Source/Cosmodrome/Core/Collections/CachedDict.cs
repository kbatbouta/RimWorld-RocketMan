using System.Collections.Generic;
using Verse;

namespace RocketMan
{
    public struct CachedUnit<T>
    {
        public readonly int tick;

        public readonly T value;

        public CachedUnit(T value)
        {
            tick = GenTicks.TicksGame;
            this.value = value;
        }

        public bool IsValid(int expiry = 0)
        {
            if (GenTicks.TicksGame - tick <= expiry)
                return true;
            return false;
        }
    }

    public class CachedDict<A, B>
    {
        private readonly Dictionary<A, CachedUnit<B>> cache = new Dictionary<A, CachedUnit<B>>();

        public B this[A key]
        {
            get => cache[key].value;
            set => AddPair(key, value);
        }

        public bool TryGetValue(A key, out B value, int expiry = 0)
        {
            if (cache.TryGetValue(key, out var store) && store.IsValid(expiry))
            {
                value = store.value;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetValue(A key, out B value, out bool failed, int expiry = 0)
        {
            if (cache.TryGetValue(key, out var store) && store.IsValid(expiry))
            {
                failed = false;
                value = store.value;
                return true;
            }

            failed = true;
            value = default;
            return false;
        }

        public void AddPair(A key, B value)
        {
            cache[key] = new CachedUnit<B>(value);
        }

        public void Remove(A key)
        {
            cache.Remove(key);
        }
    }
}