using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DoseEquivalentRateDrillingQuantity : DoseEquivalentRateQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-9;

        private static DoseEquivalentRateDrillingQuantity instance_ = null;
        public static new DoseEquivalentRateDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new DoseEquivalentRateDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public DoseEquivalentRateDrillingQuantity() : base()
        {
            Name = "DoseEquivalentRateDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "DoseEquivalentRate (drilling)" };
            ID = new Guid("131547f2-6c75-4ef2-b890-110938403d2d");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
