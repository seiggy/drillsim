using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassGradientPerLengthDrillingQuantity : MassGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.05;
        private static MassGradientPerLengthDrillingQuantity instance_ = null;

        public static new MassGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassGradientPerLengthDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public MassGradientPerLengthDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass gradient per length (drilling)" };
            ID = new Guid("dc474098-a2b5-44fb-b56a-0ae20ff62ad6");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass gradient per length in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassGradientPerLengthQuantity.Instance.GetUnitChoice(MassGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MassGradientPerLengthQuantity.Instance.GetUnitChoice(MassGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerMetre));
            this.UnitChoices.Add(MassGradientPerLengthQuantity.Instance.GetUnitChoice(MassGradientPerLengthQuantity.UnitChoicesEnum.PoundPerFoot));
            SemanticExample = GetSemanticExample();
        }
    }
}
