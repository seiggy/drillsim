using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassRatePerAreaDrillingQuantity : MassRatePerAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static MassRatePerAreaDrillingQuantity instance_ = null;
        public static new MassRatePerAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MassRatePerAreaDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MassRatePerAreaDrillingQuantity() : base()
        {
            Name = "MassRatePerAreaDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MassRatePerArea (drilling)" };
            ID = new Guid("98553367-bd99-4eb5-b721-9d2329e8dc1f");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
