using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class WorkGradientDrillingQuantity : WorkGradientQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static WorkGradientDrillingQuantity instance_ = null;
        public static new WorkGradientDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new WorkGradientDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public WorkGradientDrillingQuantity() : base()
        {
            Name = "WorkGradientDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "WorkGradient (drilling)" };
            ID = new Guid("7a07507f-a91a-471d-8194-b3ae8260286d");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
