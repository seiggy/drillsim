using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ForceAreaDrillingQuantity : ForceAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ForceAreaDrillingQuantity instance_ = null;
        public static new ForceAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ForceAreaDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ForceAreaDrillingQuantity() : base()
        {
            Name = "ForceAreaDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ForceArea (drilling)" };
            ID = new Guid("01a61a24-884a-4f5e-b9dc-de963aa6fd19");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
