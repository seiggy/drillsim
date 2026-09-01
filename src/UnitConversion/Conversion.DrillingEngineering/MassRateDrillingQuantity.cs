using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassRateDrillingQuantity : MassRateQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.0001;

        private static MassRateDrillingQuantity instance_ = null;

        public static new MassRateDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassRateDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public MassRateDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass rate (drilling)" };
            ID = new Guid("0e218b8e-bc7c-4902-b88d-1cdab4a5dc94");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass rate in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.KilogramPerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.KilogramPerSecond));
            this.UnitChoices.Add(MassRateQuantity.Instance.GetUnitChoice(MassRateQuantity.UnitChoicesEnum.KilogramPerMinute));
            SemanticExample = GetSemanticExample();
        }
    }
}
