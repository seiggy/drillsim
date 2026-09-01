using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ConsistencyIndexRheologyQuantity : DynamicViscosityQuantity
    {
        public override string TypicalSymbol { get; } = null;
        public override double? MeaningfulPrecisionInSI { get; } = 1E-04;
        private static ConsistencyIndexRheologyQuantity instance_ = null;

        public static new ConsistencyIndexRheologyQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ConsistencyIndexRheologyQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public ConsistencyIndexRheologyQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "consistency index (rheology)" };
            ID = new Guid("05571702-00e6-47d7-8590-fd3983645406");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += @"The consistency index in rheology measures a fluid's resistance to flow; it quantifies the fluid's viscosity and how it changes with shear rate." + Environment.NewLine;
            DescriptionMD += @"The meaningful precision of a consistency index in rheology is typically: " + MeaningfulPrecisionInSI.ToString() + " " + DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.PascalSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.PascalSecond));
            this.UnitChoices.Add(DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.MicropascalSecond));
            this.UnitChoices.Add(DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.Centipoise));
            this.UnitChoices.Add(DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.Micropoise));
            this.UnitChoices.Add(DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.PoundSecondPer100SquareFoot));
            this.UnitChoices.Add(DynamicViscosityQuantity.Instance.GetUnitChoice(DynamicViscosityQuantity.UnitChoicesEnum.DyneSecondPerSquareCentimetre));
            SemanticExample = GetSemanticExample();
        }
    }
}
