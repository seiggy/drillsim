using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassDensityGradientPerLengthDrillingQuantity : MassDensityGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;

        private static MassDensityGradientPerLengthDrillingQuantity instance_ = null;

        public static new MassDensityGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityGradientPerLengthDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public MassDensityGradientPerLengthDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density gradient per length (drilling)" };
            ID = new Guid("787c3f65-b6d5-4866-885b-12571b1d9734");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass density gradient per length in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerMetre));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.GramPerCubicCentimetrePer100Metre));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.PoundPerGallonUKPer100Foot));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.PoundPerGallonUKPer30Foot));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.PoundPerGallonUKPerFoot));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.PoundPerGallonUSPer100Foot));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.PoundPerGallonUSPer30Foot));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.PoundPerGallonUSPerFoot));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.SpecificGravityPer100Metre));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.SpecificGravityPer10Metre));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.SpecificGravityPer30Metre));
            this.UnitChoices.Add(MassDensityGradientPerLengthQuantity.Instance.GetUnitChoice(MassDensityGradientPerLengthQuantity.UnitChoicesEnum.SpecificGravityPerMetre));
            SemanticExample = GetSemanticExample();
        }
    }
}
