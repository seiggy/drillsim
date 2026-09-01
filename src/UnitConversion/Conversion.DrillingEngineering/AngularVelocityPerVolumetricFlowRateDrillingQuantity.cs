using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AngularVelocityPerVolumetricFlowRateDrillingQuantity : AngularVelocityPerVolumetricFlowRateQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-8;

        private static AngularVelocityPerVolumetricFlowRateDrillingQuantity instance_ = null;
        public static new AngularVelocityPerVolumetricFlowRateDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new AngularVelocityPerVolumetricFlowRateDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public AngularVelocityPerVolumetricFlowRateDrillingQuantity() : base()
        {
            Name = "AngularVelocityPerVolumetricFlowRateDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "AngularVelocityPerVolumetricFlowRate (drilling)" };
            ID = new Guid("2e3eb781-f2ac-4904-ba5e-0d70224b26be");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
