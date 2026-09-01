using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumetricFlowRatePerAreaDrillingQuantity : VolumetricFlowRatePerAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-8;

        private static VolumetricFlowRatePerAreaDrillingQuantity instance_ = null;
        public static new VolumetricFlowRatePerAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumetricFlowRatePerAreaDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumetricFlowRatePerAreaDrillingQuantity() : base()
        {
            Name = "VolumetricFlowRatePerAreaDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumetricFlowRatePerArea (drilling)" };
            ID = new Guid("ddb87b5f-2cb9-4233-95b4-b6cdfc0bfd49");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
