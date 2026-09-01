using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ReciprocalTimeDrillingQuantity : ReciprocalTimeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ReciprocalTimeDrillingQuantity instance_ = null;
        public static new ReciprocalTimeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ReciprocalTimeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ReciprocalTimeDrillingQuantity() : base()
        {
            Name = "ReciprocalTimeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ReciprocalTime (drilling)" };
            ID = new Guid("7041f958-aa34-4db5-aa71-19892051fe0d");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
