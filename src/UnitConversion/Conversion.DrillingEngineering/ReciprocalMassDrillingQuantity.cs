using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ReciprocalMassDrillingQuantity : ReciprocalMassQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-3;

        private static ReciprocalMassDrillingQuantity instance_ = null;
        public static new ReciprocalMassDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ReciprocalMassDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ReciprocalMassDrillingQuantity() : base()
        {
            Name = "ReciprocalMassDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ReciprocalMass (drilling)" };
            ID = new Guid("e6c7854a-c1aa-48fa-80b7-b532a8a7a00e");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
