using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricFieldStrengthDrillingQuantity : ElectricFieldStrengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static ElectricFieldStrengthDrillingQuantity instance_ = null;
        public static new ElectricFieldStrengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricFieldStrengthDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricFieldStrengthDrillingQuantity() : base()
        {
            Name = "ElectricFieldStrengthDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricFieldStrength (drilling)" };
            ID = new Guid("df492329-f0a9-4f1f-b83d-58faf9cf3dc4");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
