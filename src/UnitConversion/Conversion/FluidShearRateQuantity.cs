using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class FluidShearRateQuantity : FrequencyQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override double? MeaningfulPrecisionInSI { get; } = 1E-02;
        private static FluidShearRateQuantity instance_ = null;

        public static new FluidShearRateQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new FluidShearRateQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public FluidShearRateQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "fluid shear rate" };
            ID = new Guid("d3aa72c5-2fc0-4024-902e-6775d63f3231");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += @"Shear rate in a fluid is the rate at which adjacent layers of the fluid move relative to each other, typically expressed as the change in velocity per unit distance between the layers. It measures how quickly the fluid is being deformed by shear stress." + Environment.NewLine;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += @"The meaningful precision of a shear rate for a fluid is typically: " + MeaningfulPrecisionInSI.ToString() + " " + FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.Hertz));
            this.UnitChoices.Add(FrequencyQuantity.Instance.GetUnitChoice(FrequencyQuantity.UnitChoicesEnum.ReciprocalSecond));
            SemanticExample = GetSemanticExample();
        }
    }
}
