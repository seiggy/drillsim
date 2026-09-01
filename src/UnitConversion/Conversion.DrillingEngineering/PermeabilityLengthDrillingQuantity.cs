using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PermeabilityLengthDrillingQuantity : PermeabilityLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-15;

        private static PermeabilityLengthDrillingQuantity instance_ = null;
        public static new PermeabilityLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new PermeabilityLengthDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public PermeabilityLengthDrillingQuantity() : base()
        {
            Name = "PermeabilityLengthDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "PermeabilityLength (drilling)" };
            ID = new Guid("bb3256ea-df69-44ef-a243-aa8b008647b8");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
