using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Proton
{
    public class GlowerProperties
    {
        public readonly CompGlower Glower;

        public readonly ThingWithComps Parent;

        public readonly List<int> AllIndices = new List<int>();

        public Vector3 Position;

        public bool Valid
        {
            get => !this.Parent.Destroyed && this.Glower.ShouldBeLitNow;
        }

        public float GlowRadius
        {
            get => this.Glower.Props.glowRadius;
        }

        public CompProperties_Glower ParentCompProp
        {
            get => this.Glower.Props;
        }

        public GlowerProperties(CompGlower glower)
        {
            this.Glower = glower;
            this.Parent = glower.parent;
            this.Position = Parent.TrueCenter().Yto0();
        }

        public void Reset()
        {
            this.AllIndices.Clear();
        }

        public float DistanceTo(GlowerProperties other)
        {
            return Vector3.Distance(this.Position, other.Position);
        }

        public bool Intersects(GlowerProperties other)
        {
            return DistanceTo(other) + 1f < this.GlowRadius + other.GlowRadius;
        }

        public bool ContainsLocation(Vector3 loc)
        {
            return Vector3.Distance(Position, loc) + 1 < this.GlowRadius;
        }

        public bool ContainsLocation(IntVec3 loc)
        {
            return ContainsLocation(loc.ToVector3());
        }
    }
}
