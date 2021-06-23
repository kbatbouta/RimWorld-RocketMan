using System;
using Verse;

namespace RocketMan
{
    public class FlagArray
    {
        private readonly int size;
        private readonly int[] memory;

        private const int Bit = 1;
        private const int ChunkSize = 32;

        public int Size
        {
            get => size;
        }

        public FlagArray(int size)
        {
            this.memory = new int[(size / ChunkSize) + ChunkSize];
            this.size = size;
        }

        public bool Get(int key)
        {
            return (memory[key / ChunkSize] & GetOp(key)) != 0;
        }

        public FlagArray Set(int key, bool value)
        {
            memory[key / ChunkSize] = value ?
                memory[key / ChunkSize] | GetOp(key) :
                memory[key / ChunkSize] & ~GetOp(key);
            return this;
        }

        public bool this[int key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        private int GetOp(int key)
        {
            return Bit << (key % ChunkSize);
        }
    }
}
