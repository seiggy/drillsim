using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TimePerLengthDrillingQuantity : TimePerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static TimePerLengthDrillingQuantity instance_ = null;
        public static new TimePerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new TimePerLengthDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public TimePerLengthDrillingQuantity() : base()
        {
            Name = "TimePerLengthDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "TimePerLength (drilling)" };
            ID = new Guid("3fa686fa-c08b-4a86-9205-ee6100021392");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
