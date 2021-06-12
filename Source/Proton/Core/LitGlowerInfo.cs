using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Proton
{
    public class LitGlowerInfo
    {
        private class CellCollection
        {
            public readonly List<LitCell> AllCells = new List<LitCell>();
        }

        private bool flooded = false;

        private bool changed = false;

        private float overlightRadius;

        private float glowRadius;

        private ColorInt glowColor;

        private CellCollection[] collections = new CellCollection[2];

        private Vector3 position;

        public CompGlower glower;

        public Vector3 Position
        {
            get => new Vector3(position.x, position.y, position.z);
        }

        public List<LitCell> AllGlowingCells
        {
            get => collections[0] == null ? (collections[0] = new CellCollection()).AllCells : collections[0].AllCells;
        }

        public List<LitCell> AllGlowingCellsNoCavePlants
        {
            get => collections[1] == null ? (collections[1] = new CellCollection()).AllCells : collections[1].AllCells;
        }

        public bool FloodNoCavePlants
        {
            get => glower.parent.def.category != ThingCategory.Plant || !glower.parent.def.plant.cavePlant;
        }

        public bool Flooded
        {
            get => flooded;
            set => flooded = value;
        }

        public bool Changed
        {
            get => changed
                || glowRadius != Props.glowRadius
                || glowColor != Props.glowColor
                || overlightRadius != Props.overlightRadius
                || position != glower.parent.TrueCenter().Yto0();
            set => changed = value;
        }

        public bool ShouldBeLitNow
        {
            get => !(glower.parent?.Destroyed ?? true) && (glower.parent?.Spawned ?? false) && glower.ShouldBeLitNow;
        }

        public CompProperties_Glower Props
        {
            get => glower.Props;
        }

        public LitGlowerInfo(CompGlower glower)
        {
            this.glower = glower;
            this.Reset();
        }

        public bool Contains(Vector3 location)
        {
            return Vector3.Distance(location, position) - 5f < Props.glowRadius;
        }

        public void Reset()
        {
            glowRadius = Props.glowRadius;
            glowColor = Props.glowColor;
            overlightRadius = Props.overlightRadius;
            position = glower.parent.TrueCenter().Yto0();
            collections[0]?.AllCells?.Clear();
            collections[0] = null;
            collections[1]?.AllCells?.Clear();
            collections[1] = null;
            changed = false;
            flooded = false;
        }
    }
}
