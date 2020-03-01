﻿using g3;
using gs.FillTypes;

namespace gs
{
    /// <summary>
    /// configure dense-fill for sparse infill
    /// </summary>
    public class SparseLinesFillPolygon : ParallelLinesFillPolygon
    {
        public SparseLinesFillPolygon(GeneralPolygon2d poly) : base(poly)
        {
            SimplifyAmount = SimplificationLevel.Moderate;
            TypeFlags = FillTypeFlags.Invalid;
            FillType = new SparseFillType();
        }
    }

    /// <summary>
    /// configure dense-fill for support fill
    /// </summary>
    public class SupportLinesFillPolygon : ParallelLinesFillPolygon
    {
        public SupportLinesFillPolygon(GeneralPolygon2d poly, SingleMaterialFFFSettings settings) : base(poly)
        {
            SimplifyAmount = SimplificationLevel.Aggressive;
            FillType = new SupportFillType(settings);
            TypeFlags = FillTypeFlags.Invalid;
        }
    }

    /// <summary>
    /// configure dense-fill for bridge fill
    /// </summary>
    public class BridgeLinesFillPolygon : ParallelLinesFillPolygon
    {
        public BridgeLinesFillPolygon(GeneralPolygon2d poly, SingleMaterialFFFSettings settings) : base(poly)
        {
            SimplifyAmount = SimplificationLevel.Minor;
            FillType = new BridgeFillType(settings);
            TypeFlags = FillTypeFlags.Invalid;
        }
    }
}