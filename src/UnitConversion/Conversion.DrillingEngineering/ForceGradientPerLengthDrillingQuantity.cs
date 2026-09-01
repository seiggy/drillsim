using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class ForceGradientPerLengthDrillingQuantity : ForceGradientPerLengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1;
        private static ForceGradientPerLengthDrillingQuantity instance_ = null;

        public static new ForceGradientPerLengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ForceGradientPerLengthDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public ForceGradientPerLengthDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "force gradient per length (drilling)" };
            ID = new Guid("78942f39-d764-42f1-b270-47a3b35e5112");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of force gradient per length in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPerMetre).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPerMetre));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPer10Metre));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.NewtonPer30Metre));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonPerMetre));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonPer10Metre));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.DecanewtonPer30Metre));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonPerMetre));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonPer10Metre));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilonewtonPer30Metre));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.PoundPerFoot));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.PoundPer30Foot));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.PoundPer100Foot));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilopoundPerFoot));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilopoundPer30Foot));
            this.UnitChoices.Add(ForceGradientPerLengthQuantity.Instance.GetUnitChoice(ForceGradientPerLengthQuantity.UnitChoicesEnum.KilopoundPer100Foot));
            SemanticExample = GetSemanticExample();
        }
    }
}
