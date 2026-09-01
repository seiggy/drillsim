using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MagneticVectorPotentialDrillingQuantity : MagneticVectorPotentialQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-9;

        private static MagneticVectorPotentialDrillingQuantity instance_ = null;
        public static new MagneticVectorPotentialDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MagneticVectorPotentialDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MagneticVectorPotentialDrillingQuantity() : base()
        {
            Name = "MagneticVectorPotentialDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MagneticVectorPotential (drilling)" };
            ID = new Guid("6048a2a4-f865-4987-845d-3a67553ad5f0");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
