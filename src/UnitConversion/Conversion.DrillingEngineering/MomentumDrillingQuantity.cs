using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MomentumDrillingQuantity : MomentumQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-3;

        private static MomentumDrillingQuantity instance_ = null;
        public static new MomentumDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MomentumDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MomentumDrillingQuantity() : base()
        {
            Name = "MomentumDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "Momentum (drilling)" };
            ID = new Guid("682be8c6-76a2-48ea-98db-7531ad351fac");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
