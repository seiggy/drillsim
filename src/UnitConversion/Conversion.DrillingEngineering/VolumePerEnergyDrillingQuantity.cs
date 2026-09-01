using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class VolumePerEnergyDrillingQuantity : VolumePerEnergyQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-12;

        private static VolumePerEnergyDrillingQuantity instance_ = null;
        public static new VolumePerEnergyDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new VolumePerEnergyDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public VolumePerEnergyDrillingQuantity() : base()
        {
            Name = "VolumePerEnergyDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "VolumePerEnergy (drilling)" };
            ID = new Guid("57687587-c2f7-4307-9269-a5360d578b90");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
