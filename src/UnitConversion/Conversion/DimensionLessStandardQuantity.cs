using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class DimensionLessStandardQuantity : DimensionlessQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.001;

        private static DimensionLessStandardQuantity instance_ = null;

        public static new DimensionLessStandardQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DimensionLessStandardQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public DimensionLessStandardQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "dimensionless (standard)" };
            ID = new Guid("5d356437-ab4e-4de7-8219-1f4988315dee");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += @"The meaningful precision of standard dimensionless values is typically: " + MeaningfulPrecisionInSI.ToString() + " " + DimensionlessQuantity.Instance.GetUnitChoice(DimensionlessQuantity.UnitChoicesEnum.Dimensionless).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(DimensionlessQuantity.Instance.GetUnitChoice(DimensionlessQuantity.UnitChoicesEnum.Dimensionless));
            SemanticExample = GetSemanticExample();
        }
    }
}
