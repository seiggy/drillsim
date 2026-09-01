using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ThermalResistanceDrillingQuantity : ThermalResistanceQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-4;

        private static ThermalResistanceDrillingQuantity instance_ = null;
        public static new ThermalResistanceDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ThermalResistanceDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ThermalResistanceDrillingQuantity() : base()
        {
            Name = "ThermalResistanceDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ThermalResistance (drilling)" };
            ID = new Guid("b0fb4c06-7791-43a6-be10-6ac7dd68a18d");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
