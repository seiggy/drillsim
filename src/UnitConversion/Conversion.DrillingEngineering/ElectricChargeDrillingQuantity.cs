using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElectricChargeDrillingQuantity : ElectricChargeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-6;

        private static ElectricChargeDrillingQuantity instance_ = null;
        public static new ElectricChargeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new ElectricChargeDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public ElectricChargeDrillingQuantity() : base()
        {
            Name = "ElectricChargeDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "ElectricCharge (drilling)" };
            ID = new Guid("4797a81b-8763-4765-afee-f7b903b53e43");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
