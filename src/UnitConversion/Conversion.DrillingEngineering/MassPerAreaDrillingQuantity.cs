using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassPerAreaDrillingQuantity : MassPerAreaQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static MassPerAreaDrillingQuantity instance_ = null;
        public static new MassPerAreaDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MassPerAreaDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MassPerAreaDrillingQuantity() : base()
        {
            Name = "MassPerAreaDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MassPerArea (drilling)" };
            ID = new Guid("f95365ad-7e03-4f23-ab2b-234f1ea62d7c");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
