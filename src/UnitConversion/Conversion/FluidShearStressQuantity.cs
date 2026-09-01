using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class FluidShearStressQuantity : PressureQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override double? MeaningfulPrecisionInSI { get; } = 1E-04;
        private static FluidShearStressQuantity instance_ = null;

        public static new FluidShearStressQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new FluidShearStressQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public FluidShearStressQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "fluid shear stress" };
            ID = new Guid("b8f8f4f5-1925-4eaf-87c2-2adfdf618454");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += @"Shear stress in a fluid is the force per unit area exerted parallel to the fluid's surface, causing layers of the fluid to slide relative to each other. It measures the fluid's resistance to this shearing action." + Environment.NewLine;
            DescriptionMD += @"The meaningful precision of a shear stress for a fluid is typically: " + MeaningfulPrecisionInSI.ToString() + " " + PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Pascal).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Pascal));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Kilopascal));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Bar));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.PoundPerSquareInch));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.PoundPerSquareFoot));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.PoundPer100SquareFoot));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.DynePerSquareCentimetre));
            SemanticExample = GetSemanticExample();
        }
    }
}
