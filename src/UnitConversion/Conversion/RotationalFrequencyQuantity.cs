using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;

namespace OSDC.UnitConversion.Conversion
{
    public partial class RotationalFrequencyQuantity : FrequencyQuantity
    {
        public override string TypicalSymbol { get; } = "f";
        public override double? MeaningfulPrecisionInSI { get; } = 0.016666666666666666;
        private static RotationalFrequencyQuantity instance_ = null;

        public static new RotationalFrequencyQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new RotationalFrequencyQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public RotationalFrequencyQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "rotational frequency", "rotary speed" };
            ID = new Guid("f6f7ab6f-3003-49d2-a17d-92a0f81938f2");
            DescriptionMD = string.Empty;
            DescriptionMD += @"Rotation frequency is the number of complete rotations or cycles an object makes per unit of time." + Environment.NewLine;
            DescriptionMD += @"The dimension of rotational frequency is:" + Environment.NewLine;
            DescriptionMD += "$" + GetDimensionsEnclosed() + "$." + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz));
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Rpm));
            SemanticExample = GetSemanticExample();
        }

    }
}
