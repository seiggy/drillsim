using System;
using System.Collections.Generic;
using System.Linq;
using OSDC.UnitConversion.Conversion;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class TorqueGradientPerLengthDrillingQuantity : TorqueGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1;
        private static TorqueGradientPerLengthDrillingQuantity instance_ = null;

        public static new TorqueGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new TorqueGradientPerLengthDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public TorqueGradientPerLengthDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "torque gradient per length (drilling)" };
            ID = new Guid("6ad57f76-fb74-4099-a257-1d47216bfe65");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of torque gradient per length in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMetrePerMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.NewtonMetrePerMetre));
            this.UnitChoices.Add(TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonMetrePerMetre));
            this.UnitChoices.Add(TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonMetrePerMetre));
            this.UnitChoices.Add(TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.FootPoundPerFoot));
            this.UnitChoices.Add(TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilofootPoundPerFoot));
            this.UnitChoices.Add(TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.FootPoundPerMetre));
            this.UnitChoices.Add(TorqueGradientPerLengthQuantity.Instance.GetUnitChoice(TorqueGradientPerLengthQuantity.UnitChoicesEnum.KilofootPoundPerMetre));
            SemanticExample = GetSemanticExample();
        }
    }
}
