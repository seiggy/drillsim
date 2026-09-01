using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumetricFlowRateGradientPerLengthDrillingQuantity : VolumetricFlowRateGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-8;

        private static VolumetricFlowRateGradientPerLengthDrillingQuantity instance_ = null;
        public static new VolumetricFlowRateGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumetricFlowRateGradientPerLengthDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumetricFlowRateGradientPerLengthDrillingQuantity() : base()
        {
            Name = "VolumetricFlowRateGradientPerLengthDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumetricFlowRateGradientPerLength (drilling)" };
            ID = new Guid("28f0acd0-cfc0-463a-87ec-430c4b2d5671");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
