using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ProportionSmallQuantity : ProportionQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-8;

        private static ProportionSmallQuantity instance_ = null;

        public static new ProportionSmallQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ProportionSmallQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public ProportionSmallQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "proportion (small)" };
            ID = new Guid("875392e2-ef43-45f7-a19b-19c51eaba248");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += @"The meaningful precision of small proportion is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Proportion).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Proportion));
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Percent));
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.PerThousand));
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.PartPerMillion));
            SemanticExample = GetSemanticExample();
        }
    }
}
