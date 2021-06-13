using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using RocketMan;
using UnityEngine;
using Verse;

namespace Proton
{
    public class LitGlowerCacher
    {
        private bool initialized = false;

        private Color32[] buffer;

        private Color32 zeors = new Color32(0, 0, 0, 0);

        private Stopwatch stopwatch = new Stopwatch();

        public readonly Map map;

        public readonly GlowGrid glowGrid;

        public readonly GlowFlooder flooder;

        public readonly List<LitGlowerInfo> AllLitGlowers = new List<LitGlowerInfo>();

        public readonly Dictionary<CompGlower, LitGlowerInfo> InfoByComp = new Dictionary<CompGlower, LitGlowerInfo>();

        public List<LitCell>[] cell_grid;

        public ColorInt[] color_Grid;

        public ColorInt[] color_GridNoCavePlants;

        public FloodingMode FloodingMode = FloodingMode.none;

        public LitGlowerInfo CurrentFloodingGlower;

        public bool FallingBehind = false;

        public LitGlowerCacher(Map map)
        {
            this.map = map;
            this.glowGrid = map.glowGrid;
            this.flooder = map.glowFlooder;
            this.cell_grid = new List<LitCell>[glowGrid.glowGrid.Length];

            for (int i = 0; i < this.cell_grid.Length; i++)
                this.cell_grid[i] = new List<LitCell>();

            this.buffer = new Color32[glowGrid.glowGrid.Length];
            this.color_Grid = new ColorInt[glowGrid.glowGrid.Length];
            this.color_GridNoCavePlants = new ColorInt[glowGrid.glowGrid.Length];

            for (int i = 0; i < this.cell_grid.Length; i++)
            {
                this.buffer[i] = new Color32(0, 0, 0, 0);
                this.color_Grid[i] = new ColorInt(0, 0, 0, 0);
                this.color_GridNoCavePlants[i] = new ColorInt(0, 0, 0, 0);
            }
        }

        public void Register([NotNull] CompGlower comp)
        {
            if (!initialized)
            {
                Initialize();
            }
            if (comp == null)
            {
                throw new ArgumentNullException("Argument can't be null!");
            }
            if (InfoByComp.TryGetValue(comp, out LitGlowerInfo glowerInfo))
            {
                glowerInfo.Changed = true;
                Log.Warning("PROTON: Tried to regiseter an existing  CompGlower!");
                return;
            }
            AllLitGlowers.Add(InfoByComp[comp] = new LitGlowerInfo(comp));
        }

        public void DeRegister([NotNull] CompGlower comp)
        {
            if (!initialized)
            {
                Initialize();
            }
            if (comp == null)
            {
                throw new ArgumentNullException("Argument can't be null!");
            }
            if (!InfoByComp.TryGetValue(comp, out LitGlowerInfo glowerInfo))
            {
                Log.Warning("PROTON: Tried to deregister a not registered CompGlower!");
                return;
            }
            RemoveAllCells(glowerInfo);
            InfoByComp.Remove(comp);
            AllLitGlowers.Remove(glowerInfo);
        }

        public void Notify_DirtyAt(IntVec3 position)
        {
            if (!initialized)
            {
                Initialize();
            }
            foreach (LitGlowerInfo glowerInfo in AllLitGlowers.Where(g => g.Contains(position.ToVector3().Yto0())))
            {
                RemoveAllCells(glowerInfo);
                glowerInfo.Flooded = false;
            }
        }

        private static readonly HashSet<int> floodedIndices = new HashSet<int>(1024);

        public void Recalculate()
        {
            if (!initialized)
            {
                Initialize();
            }
            floodedIndices.Clear();
            foreach (LitGlowerInfo glowerInfo in AllLitGlowers.Where(g => !g.Flooded))
            {
                Flood(glowerInfo);
            }
            foreach (int index in floodedIndices)
            {
                SumAtAll(index);
            }
        }

        private void Flood(LitGlowerInfo glowerInfo)
        {
            try
            {
                CurrentFloodingGlower = glowerInfo;
                CurrentFloodingGlower.Reset();

                FloodingMode = FloodingMode.normal;
                flooder.AddFloodGlowFor(glowerInfo.glower, buffer);

                CurrentFloodingGlower.Flooded = true;

                floodedIndices.AddRange(glowerInfo.AllGlowingCells.Select(c => c.index));
            }
            catch (Exception er)
            {
                RocketMan.Logger.Debug($"PROTON: Error while flooding {glowerInfo.glower.parent}", exception: er);
            }
            finally
            {
                CurrentFloodingGlower = null;
                FloodingMode = FloodingMode.none;
            }
        }

        private void Initialize()
        {
            int numGridCells = this.map.cellIndices.NumGridCells;
            for (int i = 0; i < numGridCells; i++)
            {
                this.glowGrid.glowGrid[i] = zeors;
                this.glowGrid.glowGridNoCavePlants[i] = zeors;
            }
            this.initialized = true;
        }

        private void MapMechDirty(LitGlowerInfo glowerInfo)
        {
            this.map.mapDrawer.MapMeshDirty(glowerInfo.glower.parent.positionInt, MapMeshFlag.GroundGlow);
        }

        private void RemoveAllCells(LitGlowerInfo glowerInfo)
        {
            foreach (LitCell cell in glowerInfo.AllGlowingCells)
            {
                cell_grid[cell.index].RemoveAll(c => c.glowerInfo.glower == glowerInfo.glower || c.glowerInfo == glowerInfo);
                SetIndex(cell.index, color_Grid[cell.index] -= cell.Color);

                if (cell.glowerInfo.FloodNoCavePlants)
                    SetIndexNoCavePlants(cell.index, color_GridNoCavePlants[cell.index] -= cell.Color);
            }
            glowerInfo.Reset();
        }

        private void SetIndex(int index, ColorInt colorInt)
        {
            glowGrid.glowGrid[index] = new ColorInt(colorInt.r, colorInt.g, colorInt.b, Math.Min(colorInt.a, 1)).ToColor32;
        }

        private void SetIndexNoCavePlants(int index, ColorInt colorInt)
        {
            glowGrid.glowGridNoCavePlants[index] = new ColorInt(colorInt.r, colorInt.g, colorInt.b, Math.Min(colorInt.a, 1)).ToColor32;
        }

        private void SumAtAll(int index)
        {
            ColorInt color = new ColorInt(0, 0, 0, 0);
            ColorInt colorNoCavePlants = new ColorInt(0, 0, 0, 0);
            foreach (LitCell part in cell_grid[index])
            {
                color = part + color;
                if (part.glowerInfo.FloodNoCavePlants)
                    colorNoCavePlants = part + colorNoCavePlants;
            }
            color_Grid[index] = new ColorInt(color.r, color.g, color.b, color.a);
            color_GridNoCavePlants[index] = new ColorInt(colorNoCavePlants.r, colorNoCavePlants.g, colorNoCavePlants.b, colorNoCavePlants.a);
            if (color.a >= 1)
                color.a = 1;
            if (colorNoCavePlants.a >= 1)
                colorNoCavePlants.a = 1;
            glowGrid.glowGrid[index] = color.ToColor32;
            glowGrid.glowGridNoCavePlants[index] = colorNoCavePlants.ToColor32;
        }

        private void SumAt(int index)
        {
            ColorInt a = new ColorInt(0, 0, 0, 0);
            foreach (LitCell part in cell_grid[index])
                a = part + a;
            glowGrid.glowGrid[index] = a.ToColor32;
        }
    }
}
