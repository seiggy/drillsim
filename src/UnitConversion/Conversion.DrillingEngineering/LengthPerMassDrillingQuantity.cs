using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LengthPerMassDrillingQuantity : LengthPerMassQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static LengthPerMassDrillingQuantity instance_ = null;
        public static new LengthPerMassDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LengthPerMassDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LengthPerMassDrillingQuantity() : base()
        {
            Name = "LengthPerMassDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LengthPerMass (drilling)" };
            ID = new Guid("f99082ae-b1e6-49fd-a82d-401d8a3822d7");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
