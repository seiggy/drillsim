using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricChargePerMassDrillingQuantity : ElectricChargePerMassQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ElectricChargePerMassDrillingQuantity instance_ = null;
        public static new ElectricChargePerMassDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricChargePerMassDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricChargePerMassDrillingQuantity() : base()
        {
            Name = "ElectricChargePerMassDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricChargePerMass (drilling)" };
            ID = new Guid("bfc30f84-1134-488d-b9c5-8aa2b43f0096");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
