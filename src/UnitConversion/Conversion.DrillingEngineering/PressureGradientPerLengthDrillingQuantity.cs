using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PressureGradientPerLengthDrillingQuantity : PressureGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 10000;
        private static PressureGradientPerLengthDrillingQuantity instance_ = null;

        public static new PressureGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PressureGradientPerLengthDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public PressureGradientPerLengthDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "pressure gradient per length (drilling)" };
            ID = new Guid("92a284e7-9898-41f7-950d-4ba9f1ec550b");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of pressure gradient per length in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.PascalPerMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.PascalPerMetre));
            this.UnitChoices.Add(PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.BarPerMetre));
            this.UnitChoices.Add(PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.PsiPerFoot));
            this.UnitChoices.Add(PressureGradientPerLengthQuantity.Instance.GetUnitChoice(PressureGradientPerLengthQuantity.UnitChoicesEnum.PsiPerMetre));
            SemanticExample = GetSemanticExample();
        }
    }
}
