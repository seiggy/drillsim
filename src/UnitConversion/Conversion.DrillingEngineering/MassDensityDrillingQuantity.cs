using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassDensityDrillingQuantity : MassDensityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1;

        private static MassDensityDrillingQuantity instance_ = null;

        public static new MassDensityDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public MassDensityDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density (drilling)" };
            ID = new Guid("60f2af98-56e4-4f9c-8438-59646a35fc0d");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass density in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.KilogramPerCubicMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.KilogramPerCubicMetre));
            this.UnitChoices.Add(MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.GramPerCubicCentimetre));
            this.UnitChoices.Add(MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.GramPerCubicMetre));
            this.UnitChoices.Add(MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.PoundPerCubicFoot));
            this.UnitChoices.Add(MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.PoundPerGallonUK));
            this.UnitChoices.Add(MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.PoundPerGallonUS));
            this.UnitChoices.Add(MassDensityQuantity.Instance.GetUnitChoice(MassDensityQuantity.UnitChoicesEnum.SpecificGravity));
            SemanticExample = GetSemanticExample();
        }
    }
}
