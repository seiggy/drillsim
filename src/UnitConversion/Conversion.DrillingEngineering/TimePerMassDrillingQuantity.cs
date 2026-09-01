using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TimePerMassDrillingQuantity : TimePerMassQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-3;

        private static TimePerMassDrillingQuantity instance_ = null;
        public static new TimePerMassDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new TimePerMassDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public TimePerMassDrillingQuantity() : base()
        {
            Name = "TimePerMassDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "TimePerMass (drilling)" };
            ID = new Guid("24fde5f6-bb08-4319-ac91-b4800563f29e");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
