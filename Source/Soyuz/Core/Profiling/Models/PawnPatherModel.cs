using System;

namespace Soyuz.Profiling
{
    public class PawnPatherModel : IPawnModel
    {
        public PawnPatherModel(string name) : base(name)
        {
            this.grapher.MaxTWithoutAddtion = 21;
        }
    }
}
