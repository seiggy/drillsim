using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ElongationGradientPerLengthDrillingQuantity : ElongationGradientPerLengthQuantity
    {
        
        public override double? MeaningfulPrecisionInSI { get; } = 0.00001;
        private static ElongationGradientPerLengthDrillingQuantity instance_ = null;

        public static new ElongationGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElongationGradientPerLengthDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public ElongationGradientPerLengthDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "elongation gradient per length (drilling)" };
            ID = new Guid("4410a514-a65a-48ca-82d1-ab788b3d2df9");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of elongation gradient per length in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MetrePerMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MetrePerMetre));
            this.UnitChoices.Add(ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MillimetrePerMetre));
            this.UnitChoices.Add(ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.MillimetrePerKilometre));
            this.UnitChoices.Add(ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.InchPerFoot));
            this.UnitChoices.Add(ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.InchPerYard));
            this.UnitChoices.Add(ElongationGradientPerLengthQuantity.Instance.GetUnitChoice(ElongationGradientPerLengthQuantity.UnitChoicesEnum.InchPerMile));
            SemanticExample = GetSemanticExample();
        }
    }
}
