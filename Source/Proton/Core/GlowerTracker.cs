using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RocketMan;
using UnityEngine;
using Verse;

namespace Proton
{
    public class GlowerTracker
    {
        public readonly Map map;

        public readonly GlowGrid glowGrid;

        public readonly GlowFlooder glowFlooder;

        public readonly List<GlowerProperties> AllProps = new List<GlowerProperties>();

        public readonly Dictionary<CompGlower, GlowerProperties> PropsByComp = new Dictionary<CompGlower, GlowerProperties>();

        public HashSet<GlowerProperties> AllNewProps = new HashSet<GlowerProperties>();

        public HashSet<GlowerProperties> AllChangedProps = new HashSet<GlowerProperties>();

        public HashSet<GlowerProperties> AllRemovedProps = new HashSet<GlowerProperties>();

        public HashSet<GlowerProperties> AllFlooded = new HashSet<GlowerProperties>();

        private Color32[] grid;
        private Color32[] gridNoCavePlants;

        private readonly int numGridCells;

        private readonly Color32[] zeros;

        public GlowerTracker(Map map)
        {
            this.map = map;
            this.glowGrid = map.glowGrid;
            this.glowFlooder = map.glowFlooder;
            this.numGridCells = map.cellIndices.NumGridCells;
            this.zeros = new Color32[numGridCells];
            this.BuildPrepareGrid();
        }

        public void RecalculateAllGlow()
        {
            this.AllFlooded.Clear();
            this.zeros.CopyTo(grid, 0);
            this.zeros.CopyTo(gridNoCavePlants, 0);

            foreach (GlowerProperties props in AllProps.Where(p => !p.Valid))
            {
                FloodRemove(props);
            }

            foreach (GlowerProperties props in AllChangedProps)
            {
                FloodDiff(props);
            }

            foreach (GlowerProperties props in AllNewProps)
            {
                FloodAdd(props);
            }

            AllNewProps.Clear();
            AllRemovedProps.Clear();
            AllChangedProps.Clear();
        }

        public void Register(CompGlower glower)
        {
            GlowerProperties props = GetProperties(glower);
            if (glower.ShouldBeLitNow)
            {
                AllNewProps.Add(props);
            }
            else
            {
                AllProps.Add(props);
            }
        }

        public void Notify_ChangeAt(IntVec3 loc)
        {
            AllChangedProps.AddRange(AllProps.Where(p => p.ContainsLocation(loc)));
        }

        public void DeRegister(CompGlower glower)
        {
            AllRemovedProps.Add(GetProperties(glower));
        }

        public GlowerProperties GetProperties(CompGlower glower)
        {
            return PropsByComp.TryGetValue(glower, out var props) ? props : PropsByComp[glower] = new GlowerProperties(glower);
        }

        private void FloodDiff(GlowerProperties props)
        {
            List<int> oldIndices = props.AllIndices;

            foreach (int index in oldIndices)
            {
                glowGrid.glowGrid[index] = new Color32(0, 0, 0, 0);
                glowGrid.glowGridNoCavePlants[index] = new Color32(0, 0, 0, 0);
            }
            FloodNeighbors(props);

            foreach (int index in oldIndices)
            {
                glowGrid.glowGrid[index] = grid[index];
                glowGrid.glowGridNoCavePlants[index] = gridNoCavePlants[index];
            }
            FloodAdd(props);
        }

        private void FloodRemove(GlowerProperties props)
        {
            bool floodNoCavePlants = props.Glower.parent.def.category != ThingCategory.Plant || !props.Glower.parent.def.plant.cavePlant;
            foreach (int index in props.AllIndices)
            {
                glowGrid.glowGrid[index] = new Color32();
                if (floodNoCavePlants)
                    glowGrid.glowGridNoCavePlants[index] = new Color32();
            }
            foreach (int index in props.AllIndices)
            {
                glowGrid.glowGrid[index] = grid[index];
            }
            FloodNeighbors(props);
            foreach (int index in props.AllIndices)
            {
                glowGrid.glowGrid[index] = grid[index];
                if (floodNoCavePlants)
                    glowGrid.glowGridNoCavePlants[index] = gridNoCavePlants[index];
            }
        }

        private void FloodAdd(GlowerProperties props)
        {
            bool floodNoCavePlants = props.Glower.parent.def.category != ThingCategory.Plant || !props.Glower.parent.def.plant.cavePlant;
            Flood(props, glowGrid.glowGrid, isFinal: !floodNoCavePlants);
            if (floodNoCavePlants)
            {
                Flood(props, glowGrid.glowGridNoCavePlants);
            }
        }

        private void FloodNeighbors(GlowerProperties props)
        {
            bool floodNoCavePlants = props.Glower.parent.def.category != ThingCategory.Plant || !props.Glower.parent.def.plant.cavePlant;
            foreach (GlowerProperties other in AllProps.Where(p => p.Intersects(p) && p.Valid))
            {
                if (other != props)
                {
                    this.Flood(other, grid, !floodNoCavePlants);
                    if (floodNoCavePlants && (other.Glower.parent.def.category != ThingCategory.Plant || !other.Glower.parent.def.plant.cavePlant))
                    {
                        this.Flood(other, glowGrid.glowGridNoCavePlants);
                    }
                }
            }
        }

        private void Flood(GlowerProperties props, Color32[] grid, bool isFinal = true)
        {
            if (AllFlooded.Contains(props) || !props.Valid)
            {
                return;
            }
            glowFlooder.AddFloodGlowFor(props.Glower, grid);
            if (isFinal)
            {
                AllFlooded.Add(props);
            }
        }

        private void BuildPrepareGrid()
        {
            this.grid = new Color32[numGridCells];
            this.gridNoCavePlants = new Color32[numGridCells];
            for (int i = 0; i < numGridCells; i++)
            {
                this.zeros[i] = new Color32(0, 0, 0, 0);
                this.grid[i] = new Color32(0, 0, 0, 0);
                this.gridNoCavePlants[i] = new Color32(0, 0, 0, 0);
            }
        }
    }
}
