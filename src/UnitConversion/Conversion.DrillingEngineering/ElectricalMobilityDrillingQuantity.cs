using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricalMobilityDrillingQuantity : ElectricalMobilityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ElectricalMobilityDrillingQuantity instance_ = null;
        public static new ElectricalMobilityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricalMobilityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricalMobilityDrillingQuantity() : base()
        {
            Name = "ElectricalMobilityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricalMobility (drilling)" };
            ID = new Guid("4032f03d-fce1-4997-966b-b7b98d0ddd7f");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
