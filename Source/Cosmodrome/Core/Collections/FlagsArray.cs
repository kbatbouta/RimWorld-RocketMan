using System;
using Verse;

namespace RocketMan
{
    public class FlagsArray
    {
        private readonly int size;
        private readonly int[] memory;

        public int Size
        {
            get => size;
        }

        public FlagsArray(int size)
        {
            this.memory = new int[(size / 32) + 16];
            this.size = size;
        }

        public bool Get(int key)
        {
            int index = key / 32;
            return (memory[index] & (1 << (key % 32))) == (1 << (key % 32));
        }

        public FlagsArray Set(int key, bool value)
        {
            int index = key / 32;
            memory[index] = value ?
                memory[index] | (1 << (key % 32)) : memory[index] & ((1 << (key % 32)) ^ 0x00);
            return this;
        }

        public bool this[int key]
        {
            get => Get(key);
            set => Set(key, value);
        }
    }
}
