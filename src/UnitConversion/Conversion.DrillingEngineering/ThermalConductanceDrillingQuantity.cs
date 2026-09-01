using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ThermalConductanceDrillingQuantity : ThermalConductanceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-1;

        private static ThermalConductanceDrillingQuantity instance_ = null;
        public static new ThermalConductanceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ThermalConductanceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ThermalConductanceDrillingQuantity() : base()
        {
            Name = "ThermalConductanceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ThermalConductance (drilling)" };
            ID = new Guid("307d1d79-ef4d-40c6-b0f7-d3e0f17606a4");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
