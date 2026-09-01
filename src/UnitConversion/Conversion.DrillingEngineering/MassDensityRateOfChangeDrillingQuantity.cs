using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class MassDensityRateOfChangeDrillingQuantity : MassDensityRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;

        private static MassDensityRateOfChangeDrillingQuantity instance_ = null;

        public static new MassDensityRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new MassDensityRateOfChangeDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public MassDensityRateOfChangeDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "mass density rate of change (drilling)" };
            ID = new Guid("af63f164-0fb7-42c0-ac55-06e40b6c12e5");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of mass density rate of change in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.KilogramPerCubicMetrePerSecond));
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.GramPerCubicCentimetrePerHour));
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUKPerHour));
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUKPerMinute));
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUKPerSecond));
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUSPerHour));
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUSPerMinute));
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.PoundPerGallonUSPerSecond));
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.SpecificGravityPerHour));
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.SpecificGravityPerMinute));
            this.UnitChoices.Add(MassDensityRateOfChangeQuantity.Instance.GetUnitChoice(MassDensityRateOfChangeQuantity.UnitChoicesEnum.SpecificGravityPerSecond));
            SemanticExample = GetSemanticExample();
        }
    }
}
