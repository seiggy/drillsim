using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AbsorbedDoseDrillingQuantity : AbsorbedDoseQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static AbsorbedDoseDrillingQuantity instance_ = null;
        public static new AbsorbedDoseDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new AbsorbedDoseDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public AbsorbedDoseDrillingQuantity() : base()
        {
            Name = "AbsorbedDoseDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "AbsorbedDose (drilling)" };
            ID = new Guid("e547919f-3868-4543-9445-46684c630906");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
