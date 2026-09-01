using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricResistanceGradientPerLengthDrillingQuantity : ElectricResistanceGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-3;

        private static ElectricResistanceGradientPerLengthDrillingQuantity instance_ = null;
        public static new ElectricResistanceGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricResistanceGradientPerLengthDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricResistanceGradientPerLengthDrillingQuantity() : base()
        {
            Name = "ElectricResistanceGradientPerLengthDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricResistanceGradientPerLength (drilling)" };
            ID = new Guid("45caf248-944b-4d56-82b3-3cd871f2ba0a");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
