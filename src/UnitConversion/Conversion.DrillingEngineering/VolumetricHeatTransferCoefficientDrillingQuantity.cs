using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumetricHeatTransferCoefficientDrillingQuantity : VolumetricHeatTransferCoefficientQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static VolumetricHeatTransferCoefficientDrillingQuantity instance_ = null;
        public static new VolumetricHeatTransferCoefficientDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumetricHeatTransferCoefficientDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumetricHeatTransferCoefficientDrillingQuantity() : base()
        {
            Name = "VolumetricHeatTransferCoefficientDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumetricHeatTransferCoefficient (drilling)" };
            ID = new Guid("63fe0902-31e4-4fdc-8424-1ad4ec71f2a1");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
