using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ReciprocalForceDrillingQuantity : ReciprocalForceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-3;

        private static ReciprocalForceDrillingQuantity instance_ = null;
        public static new ReciprocalForceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ReciprocalForceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ReciprocalForceDrillingQuantity() : base()
        {
            Name = "ReciprocalForceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ReciprocalForce (drilling)" };
            ID = new Guid("ca907420-a3b3-4caa-ba34-6bb8e335eba0");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
