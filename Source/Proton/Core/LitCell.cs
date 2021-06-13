using RimWorld;
using Verse;

namespace Proton
{
    public struct LitCell
    {
        private readonly ColorInt colorInt;

        private bool alpha;

        private bool zeros;

        private CompProperties_Glower props;

        public readonly int index;

        public readonly float distance;

        public readonly LitGlowerInfo glowerInfo;

        public ColorInt Color
        {
            get => colorInt;
        }

        public CompProperties_Glower Props
        {
            get => props;
        }

        public bool ColorIsZeros
        {
            get => zeros;
        }

        public LitCell(LitGlowerInfo glowerInfo, ColorInt colorInt, int index, float distance)
        {
            this.glowerInfo = glowerInfo;
            this.colorInt = colorInt;
            this.index = index;
            this.distance = distance;
            this.alpha = this.distance < glowerInfo.Props.overlightRadius;
            this.zeros = this.colorInt.r == 0 && this.colorInt.g == 0 && this.colorInt.b == 0;
            this.props = glowerInfo.Props;
            if (this.alpha)
            {
                colorInt.a = 1;
            }
        }

        public static ColorInt operator +(LitCell cell, ColorInt color)
        {
            color.a += cell.colorInt.a;
            color.r += cell.colorInt.r;
            color.g += cell.colorInt.g;
            color.b += cell.colorInt.b;
            return color;
        }

        public static ColorInt operator +(ColorInt color, LitCell cell)
        {
            return cell + color;
        }
    }
}
