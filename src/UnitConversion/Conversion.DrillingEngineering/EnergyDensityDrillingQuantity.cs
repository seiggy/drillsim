using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class EnergyDensityDrillingQuantity : EnergyDensityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.01;

        private static EnergyDensityDrillingQuantity instance_ = null;

        public static new EnergyDensityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new EnergyDensityDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public EnergyDensityDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "energy density (drilling)" };
            ID = new Guid("04bc9209-c5c0-4f42-98b1-f1f63a3bee52");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of energy density in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerCubicMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerCubicMetre));
            this.UnitChoices.Add(EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerCubicFoot));
            this.UnitChoices.Add(EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerCubicInch));
            this.UnitChoices.Add(EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerGallonUK));
            this.UnitChoices.Add(EnergyDensityQuantity.Instance.GetUnitChoice(EnergyDensityQuantity.UnitChoicesEnum.JoulePerGallonUS));
            SemanticExample = GetSemanticExample();
        }
    }
}
