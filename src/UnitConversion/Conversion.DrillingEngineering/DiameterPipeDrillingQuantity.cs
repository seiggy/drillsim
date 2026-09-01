using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DiameterPipeDrillingQuantity : LengthSmallQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.0001;

        private static DiameterPipeDrillingQuantity instance_ = null;

        public static new DiameterPipeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DiameterPipeDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public DiameterPipeDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "diameter (Pipe) (drilling)" };
            ID = new Guid("28a2fe65-db50-46ec-8b1d-2a26286a5813");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of pipe diameter in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre).UnitLabel + Environment.NewLine;
            SemanticExample = GetSemanticExample();
        }

    }
}
