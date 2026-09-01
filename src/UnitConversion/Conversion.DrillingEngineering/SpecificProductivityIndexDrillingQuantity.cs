using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class SpecificProductivityIndexDrillingQuantity : SpecificProductivityIndexQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-10;

        private static SpecificProductivityIndexDrillingQuantity instance_ = null;
        public static new SpecificProductivityIndexDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new SpecificProductivityIndexDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public SpecificProductivityIndexDrillingQuantity() : base()
        {
            Name = "SpecificProductivityIndexDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "SpecificProductivityIndex (drilling)" };
            ID = new Guid("1a7802f5-b861-42fe-951d-598c01a6124f");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
