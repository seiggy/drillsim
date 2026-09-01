using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassRateGradientPerLengthDrillingQuantity : MassRateGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static MassRateGradientPerLengthDrillingQuantity instance_ = null;
        public static new MassRateGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MassRateGradientPerLengthDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MassRateGradientPerLengthDrillingQuantity() : base()
        {
            Name = "MassRateGradientPerLengthDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MassRateGradientPerLength (drilling)" };
            ID = new Guid("c8927c10-2760-412f-953e-591fcb1bc6bb");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
