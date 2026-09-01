using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MobilityDrillingQuantity : MobilityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-15;

        private static MobilityDrillingQuantity instance_ = null;
        public static new MobilityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MobilityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MobilityDrillingQuantity() : base()
        {
            Name = "MobilityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "Mobility (drilling)" };
            ID = new Guid("471673ab-e0a0-4e19-b5c3-c10fc6f3ec15");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
