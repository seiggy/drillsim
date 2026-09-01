using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ThermalInsulanceDrillingQuantity : ThermalInsulanceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-4;

        private static ThermalInsulanceDrillingQuantity instance_ = null;
        public static new ThermalInsulanceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ThermalInsulanceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ThermalInsulanceDrillingQuantity() : base()
        {
            Name = "ThermalInsulanceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ThermalInsulance (drilling)" };
            ID = new Guid("04bf21e6-db29-4007-8e5e-9f5c3045c113");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
