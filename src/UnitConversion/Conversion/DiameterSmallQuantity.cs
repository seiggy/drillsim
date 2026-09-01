using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion
{
    public partial class DiameterSmallQuantity : LengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.0001;

        private static DiameterSmallQuantity instance_ = null;

        public static new DiameterSmallQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DiameterSmallQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public DiameterSmallQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "diameter (small)" };
            ID = new Guid("d07d00aa-35aa-41c6-a52d-ad51c3f4e97f");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += @"The meaningful precision of small diameter is typically: " + MeaningfulPrecisionInSI.ToString() + " " + LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Centimetre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Decimetre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Foot));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Inch));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Micrometre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Millimetre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Nanometre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Picometre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Ångstrøm));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.InchPer32));
            SemanticExample = GetSemanticExample();
        }
    }
}
