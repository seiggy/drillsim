using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassPerEnergyDrillingQuantity : MassPerEnergyQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-9;

        private static MassPerEnergyDrillingQuantity instance_ = null;
        public static new MassPerEnergyDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MassPerEnergyDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MassPerEnergyDrillingQuantity() : base()
        {
            Name = "MassPerEnergyDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MassPerEnergy (drilling)" };
            ID = new Guid("650486f6-912c-44a5-af11-e89e524f5136");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
