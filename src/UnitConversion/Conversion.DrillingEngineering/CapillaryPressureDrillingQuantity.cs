using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class CapillaryPressureDrillingQuantity : PressureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 0.1;
        private static CapillaryPressureDrillingQuantity instance_ = null;

        public static new CapillaryPressureDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new CapillaryPressureDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public CapillaryPressureDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "capillary pressure (drilling)" };
            ID = new Guid("72228ff1-9ba8-44f0-b9b1-85fa8dfb32ea");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of capilary pressure in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Pascal).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Pascal));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Bar));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.PoundPerSquareInch));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Kilopascal));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.MegapoundPerSquareInch));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.KilopoundPerSquareInch));
            SemanticExample = GetSemanticExample();
        }
    }
}
