using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class SpecificEnergyDrillingQuantity : SpecificEnergyQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static SpecificEnergyDrillingQuantity instance_ = null;
        public static new SpecificEnergyDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new SpecificEnergyDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public SpecificEnergyDrillingQuantity() : base()
        {
            Name = "SpecificEnergyDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "SpecificEnergy (drilling)" };
            ID = new Guid("5d8b8019-9e81-4317-ad88-0ba4728ac672");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
