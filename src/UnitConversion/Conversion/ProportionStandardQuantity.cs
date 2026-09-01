using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ProportionStandardQuantity : ProportionQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;

        private static ProportionStandardQuantity instance_ = null;

        public static new ProportionStandardQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ProportionStandardQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public ProportionStandardQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "proportion (standard)" };
            ID = new Guid("97555d61-9fc3-4769-9143-6bc2bf51b2d7");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += @"The meaningful precision of a standard proportion is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Proportion).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Proportion));
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.Percent));
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.PerThousand));
            this.UnitChoices.Add(ProportionQuantity.Instance.GetUnitChoice(ProportionQuantity.UnitChoicesEnum.PartPerMillion));
            SemanticExample = GetSemanticExample();
        }
    }
}
