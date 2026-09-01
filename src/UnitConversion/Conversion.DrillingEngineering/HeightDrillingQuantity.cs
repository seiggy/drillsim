using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class HeightDrillingQuantity : LengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;
        private static HeightDrillingQuantity instance_ = null;

        public static new HeightDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new HeightDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public HeightDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "height (drilling)" };
            ID = new Guid("8ec20e78-3307-4363-9fc1-97c64a4b6e6e");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of height in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Metre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Millimetre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Centimetre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Decimetre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Hectometre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Kilometre));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Inch));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Foot));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Yard));
            this.UnitChoices.Add(LengthQuantity.Instance.GetUnitChoice(LengthQuantity.UnitChoicesEnum.Mile));
            SemanticExample = GetSemanticExample();
        }
    }
}
