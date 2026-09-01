using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class InductanceDrillingQuantity : InductanceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static InductanceDrillingQuantity instance_ = null;
        public static new InductanceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new InductanceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public InductanceDrillingQuantity() : base()
        {
            Name = "InductanceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "Inductance (drilling)" };
            ID = new Guid("bee46416-df3a-458e-8a07-29432e767d9b");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
