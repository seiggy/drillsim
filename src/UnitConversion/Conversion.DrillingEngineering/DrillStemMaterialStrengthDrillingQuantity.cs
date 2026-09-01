using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{
    public partial class DrillStemMaterialStrengthDrillingQuantity : MaterialStrengthQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 10000;
        private static DrillStemMaterialStrengthDrillingQuantity instance_ = null;

        public static new DrillStemMaterialStrengthDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new DrillStemMaterialStrengthDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public DrillStemMaterialStrengthDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "drill stem material strength (drilling)" };
            ID = new Guid("fd58fca3-6221-4e85-a7aa-a021ee04e8a8");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of drill stem material strength in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Pascal).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Pascal));
            this.UnitChoices.Add(MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Gigapascal));
            this.UnitChoices.Add(MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Megapascal));
            this.UnitChoices.Add(MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.MegapoundPerSquareInch));
            this.UnitChoices.Add(MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.PoundPer100SquareFoot));
            this.UnitChoices.Add(MaterialStrengthQuantity.Instance.GetUnitChoice(MaterialStrengthQuantity.UnitChoicesEnum.Psi));
            SemanticExample = GetSemanticExample();
        }
    }
}
