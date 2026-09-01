using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class RadioactivityDrillingQuantity : RadioactivityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static RadioactivityDrillingQuantity instance_ = null;
        public static new RadioactivityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new RadioactivityDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public RadioactivityDrillingQuantity() : base()
        {
            Name = "RadioactivityDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "Radioactivity (drilling)" };
            ID = new Guid("4813ac11-bbec-4217-809b-84541478c8e9");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
