using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class KinematicViscosityDrillingQuantity : KinematicViscosityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static KinematicViscosityDrillingQuantity instance_ = null;
        public static new KinematicViscosityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new KinematicViscosityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public KinematicViscosityDrillingQuantity() : base()
        {
            Name = "KinematicViscosityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "KinematicViscosity (drilling)" };
            ID = new Guid("f3572495-0935-4ec3-9b05-4d7692fb4bce");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
