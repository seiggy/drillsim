using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class SectionModulusDrillingQuantity : SectionModulusQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static SectionModulusDrillingQuantity instance_ = null;
        public static new SectionModulusDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new SectionModulusDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public SectionModulusDrillingQuantity() : base()
        {
            Name = "SectionModulusDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "SectionModulus (drilling)" };
            ID = new Guid("d6e491cf-3c24-438a-b356-96f717cd433b");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
