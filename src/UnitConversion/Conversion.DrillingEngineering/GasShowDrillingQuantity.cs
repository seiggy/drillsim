using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class GasShowDrillingQuantity : ProportionQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1E-07;
        private static GasShowDrillingQuantity instance_ = null;

        public static new GasShowDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new GasShowDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public GasShowDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "gas show (drilling)" };
            ID = new Guid("c81adbce-b90b-4889-8b79-921d95b35179");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of gas show in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Proportion).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Proportion));
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Percent));
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.PerThousand));
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.PartPerMillion));
            SemanticExample = GetSemanticExample();
        }
    }
}
