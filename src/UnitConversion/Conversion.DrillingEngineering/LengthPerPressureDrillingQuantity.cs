using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LengthPerPressureDrillingQuantity : LengthPerPressureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static LengthPerPressureDrillingQuantity instance_ = null;
        public static new LengthPerPressureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LengthPerPressureDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LengthPerPressureDrillingQuantity() : base()
        {
            Name = "LengthPerPressureDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LengthPerPressure (drilling)" };
            ID = new Guid("022959b9-69d7-4e92-ad1e-119fe870aa5f");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
