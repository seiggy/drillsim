using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassDrillingQuantity : MassQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.1;
        private static MassDrillingQuantity instance_ = null;

        public static new MassDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public MassDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass (drilling)" };
            ID = new Guid("f4c0a6fd-5000-4507-8612-ae4374c0cacc");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass density in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Kilogram).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Kilogram));
            this.UnitChoices.Add(MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.TonneMetric));
            this.UnitChoices.Add(MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Pound));
            this.UnitChoices.Add(MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.Kilopound));
            this.UnitChoices.Add(MassQuantity.Instance.GetUnitChoice(MassQuantity.UnitChoicesEnum.TonUK));
            SemanticExample = GetSemanticExample();
        }
    }
}
