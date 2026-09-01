using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassLengthDrillingQuantity : MassLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static MassLengthDrillingQuantity instance_ = null;
        public static new MassLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MassLengthDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MassLengthDrillingQuantity() : base()
        {
            Name = "MassLengthDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MassLength (drilling)" };
            ID = new Guid("ff3a4ff1-fa92-4c11-b9bd-9fe7fe6d88a5");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
