using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricChargePerAreaDrillingQuantity : ElectricChargePerAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ElectricChargePerAreaDrillingQuantity instance_ = null;
        public static new ElectricChargePerAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricChargePerAreaDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricChargePerAreaDrillingQuantity() : base()
        {
            Name = "ElectricChargePerAreaDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricChargePerArea (drilling)" };
            ID = new Guid("88b3fc64-2f64-4980-8484-37135ac012f4");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
