using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LuminousFluxDrillingQuantity : LuminousFluxQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-2;

        private static LuminousFluxDrillingQuantity instance_ = null;
        public static new LuminousFluxDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LuminousFluxDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LuminousFluxDrillingQuantity() : base()
        {
            Name = "LuminousFluxDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LuminousFlux (drilling)" };
            ID = new Guid("2dfa2649-dd6f-4be3-b5ae-1e959d31aa3d");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
