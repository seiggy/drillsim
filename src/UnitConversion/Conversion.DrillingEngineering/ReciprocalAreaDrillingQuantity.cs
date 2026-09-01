using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ReciprocalAreaDrillingQuantity : ReciprocalAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ReciprocalAreaDrillingQuantity instance_ = null;
        public static new ReciprocalAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ReciprocalAreaDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ReciprocalAreaDrillingQuantity() : base()
        {
            Name = "ReciprocalAreaDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ReciprocalArea (drilling)" };
            ID = new Guid("823c07c5-7e42-47a4-b3a4-de8c5685c436");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
