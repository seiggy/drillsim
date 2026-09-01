using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MagneticFluxDensityGradientPerLengthDrillingQuantity : MagneticFluxDensityGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-9;

        private static MagneticFluxDensityGradientPerLengthDrillingQuantity instance_ = null;
        public static new MagneticFluxDensityGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MagneticFluxDensityGradientPerLengthDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MagneticFluxDensityGradientPerLengthDrillingQuantity() : base()
        {
            Name = "MagneticFluxDensityGradientPerLengthDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MagneticFluxDensityGradientPerLength (drilling)" };
            ID = new Guid("977c0837-4722-418d-8fe7-263c48cfca0b");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
