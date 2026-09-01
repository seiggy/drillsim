using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MolarMassDrillingQuantity : MolarMassQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-3;

        private static MolarMassDrillingQuantity instance_ = null;
        public static new MolarMassDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MolarMassDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MolarMassDrillingQuantity() : base()
        {
            Name = "MolarMassDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MolarMass (drilling)" };
            ID = new Guid("13b9afdf-0e58-4f21-800d-06039022f66e");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
