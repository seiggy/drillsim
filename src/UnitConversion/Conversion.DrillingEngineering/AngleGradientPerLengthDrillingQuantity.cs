using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class AngleGradientPerLengthDrillingQuantity : AngleGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.00017453292519943296;

        private static AngleGradientPerLengthDrillingQuantity instance_ = null;

        public static new AngleGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AngleGradientPerLengthDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public AngleGradientPerLengthDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "angle gradient per length (drilling)" };
            ID = new Guid("e6f22876-ca88-4d0e-a4a5-c76b0db3556f");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "In a drilling engineering perspective, the meaningful precision of an angle gradient per length is: " + MeaningfulPrecisionInSI.ToString() + " " + AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.RadianPerMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.RadianPerMetre));
            this.UnitChoices.Add(AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerMetre));
            this.UnitChoices.Add(AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerFoot));
            this.UnitChoices.Add(AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerCentimetre));
            this.UnitChoices.Add(AngleGradientPerLengthQuantity.Instance.GetUnitChoice(AngleGradientPerLengthQuantity.UnitChoicesEnum.DegreePerInch));
            SemanticExample = GetSemanticExample();
        }
    }
}
