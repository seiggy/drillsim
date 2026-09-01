using System;
using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DataStorageDrillingQuantity : DataStorageQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1.0;

        private static DataStorageDrillingQuantity instance_ = null;
        public static new DataStorageDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null) { instance_ = new DataStorageDrillingQuantity(); instance_.PostProcess(); }
                return instance_;
            }
        }

        public DataStorageDrillingQuantity() : base()
        {
            Name = "DataStorageDrilling";
            UsualNames = new HashSet<string>(base.UsualNames ?? new HashSet<string>()) { "DataStorage (drilling)" };
            ID = new Guid("59c2f7d1-0022-405d-bb91-b9206311600c");
            DescriptionMD = base.DescriptionMD + Environment.NewLine + "The meaningful precision in the drilling context is " + MeaningfulPrecisionInSI + " in coherent SI units." + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }
    }
}
