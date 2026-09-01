using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MagneticPermeabilityDrillingQuantity : MagneticPermeabilityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-9;

        private static MagneticPermeabilityDrillingQuantity instance_ = null;
        public static new MagneticPermeabilityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new MagneticPermeabilityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public MagneticPermeabilityDrillingQuantity() : base()
        {
            Name = "MagneticPermeabilityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "MagneticPermeability (drilling)" };
            ID = new Guid("bcbfd27c-289a-4494-8656-64c98611ac43");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
