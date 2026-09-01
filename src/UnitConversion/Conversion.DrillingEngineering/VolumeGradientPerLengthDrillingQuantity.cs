using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumeGradientPerLengthDrillingQuantity : VolumeGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static VolumeGradientPerLengthDrillingQuantity instance_ = null;
        public static new VolumeGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumeGradientPerLengthDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumeGradientPerLengthDrillingQuantity() : base()
        {
            Name = "VolumeGradientPerLengthDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumeGradientPerLength (drilling)" };
            ID = new Guid("0bc98383-f843-4e72-89c3-17a6b7ede034");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
