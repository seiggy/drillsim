using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class SpecificActivityDrillingQuantity : SpecificActivityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static SpecificActivityDrillingQuantity instance_ = null;
        public static new SpecificActivityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new SpecificActivityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public SpecificActivityDrillingQuantity() : base()
        {
            Name = "SpecificActivityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "SpecificActivity (drilling)" };
            ID = new Guid("d7db2a32-e6f9-4257-8d9b-ff1a9a2f3261");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
