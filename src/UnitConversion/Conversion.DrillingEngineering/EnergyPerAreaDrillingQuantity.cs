using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class EnergyPerAreaDrillingQuantity : EnergyPerAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-1;

        private static EnergyPerAreaDrillingQuantity instance_ = null;
        public static new EnergyPerAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new EnergyPerAreaDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public EnergyPerAreaDrillingQuantity() : base()
        {
            Name = "EnergyPerAreaDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "EnergyPerArea (drilling)" };
            ID = new Guid("75b134a4-84ee-4600-af99-78782093bca4");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
