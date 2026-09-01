using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class LuminousEnergyDrillingQuantity : LuminousEnergyQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-2;

        private static LuminousEnergyDrillingQuantity instance_ = null;
        public static new LuminousEnergyDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new LuminousEnergyDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public LuminousEnergyDrillingQuantity() : base()
        {
            Name = "LuminousEnergyDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "LuminousEnergy (drilling)" };
            ID = new Guid("edbc063b-145e-468f-84a5-d0025b0339b5");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
