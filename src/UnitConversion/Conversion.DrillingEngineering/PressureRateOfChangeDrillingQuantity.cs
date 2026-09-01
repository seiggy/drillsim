using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class PressureRateOfChangeDrillingQuantity : PressureRateOfChangeQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 10000;

        private static PressureRateOfChangeDrillingQuantity instance_ = null;

        public static new PressureRateOfChangeDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new PressureRateOfChangeDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public PressureRateOfChangeDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "pressure rate of change (drilling)" };
            ID = new Guid("15962c4f-9163-44ed-afec-bc9f17e60983");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of pressure rate of change in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.PascalPerSecond).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.PascalPerSecond));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.BarPerSecond));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.PoundPerSquareInchPerSecond));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.KilopascalPerSecond));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.MegapascalPerSecond));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.KilopoundPerSquareInchPerSecond));

            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.PascalPerMinute));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.BarPerMinute));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.PoundPerSquareInchPerMinute));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.KilopascalPerMinute));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.MegapascalPerMinute));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.KilopoundPerSquareInchPerMinute));

            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.PascalPerHour));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.BarPerHour));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.PoundPerSquareInchPerHour));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.KilopascalPerHour));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.MegapascalPerHour));
            this.UnitChoices.Add(PressureRateOfChangeQuantity.Instance.GetUnitChoice(PressureRateOfChangeQuantity.UnitChoicesEnum.KilopoundPerSquareInchPerHour));
            SemanticExample = GetSemanticExample();
        }
    }
}
