using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MolarEnergyDrillingQuantity : MolarEnergyQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-3;

        private static MolarEnergyDrillingQuantity instance_ = null;
        public static new MolarEnergyDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MolarEnergyDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MolarEnergyDrillingQuantity() : base()
        {
            Name = "MolarEnergyDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MolarEnergy (drilling)" };
            ID = new Guid("2bfe9778-5b42-4c3e-a4c9-ca08b83010f6");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
