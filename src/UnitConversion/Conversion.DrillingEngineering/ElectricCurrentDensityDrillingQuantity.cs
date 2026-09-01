using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricCurrentDensityDrillingQuantity : ElectricCurrentDensityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ElectricCurrentDensityDrillingQuantity instance_ = null;
        public static new ElectricCurrentDensityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricCurrentDensityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricCurrentDensityDrillingQuantity() : base()
        {
            Name = "ElectricCurrentDensityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricCurrentDensity (drilling)" };
            ID = new Guid("eb7a8775-95bd-40ed-9651-cb11688a807f");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
