using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PermittivityDrillingQuantity : PermittivityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-12;

        private static PermittivityDrillingQuantity instance_ = null;
        public static new PermittivityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new PermittivityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public PermittivityDrillingQuantity() : base()
        {
            Name = "PermittivityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "Permittivity (drilling)" };
            ID = new Guid("fbd7c31d-a08c-46aa-b08d-4cb11893a3d1");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
