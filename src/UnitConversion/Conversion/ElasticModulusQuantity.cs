using System;
using System.Collections.Generic;
using System.Linq;

namespace OSDC.UnitConversion.Conversion
{
    public partial class ElasticModulusQuantity : PressureQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 10000;
        private static ElasticModulusQuantity instance_ = null;

        public static new ElasticModulusQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new ElasticModulusQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }
        public ElasticModulusQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "elastic modulus" };
            ID = new Guid("7ffbcc35-b46f-4656-baf5-c92be501f0ec");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += @"Elastic modulus is a measure of a material's ability to resist deformation under stress. It quantifies the ratio of stress to strain in the material's elastic region" + Environment.NewLine;
            DescriptionMD += @"The meaningful precision of an elastic modulus is typically: " + MeaningfulPrecisionInSI.ToString() + " " + PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Pascal).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Pascal));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Megapascal));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.Gigapascal));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.PoundPerSquareInch));
            this.UnitChoices.Add(PressureQuantity.Instance.GetUnitChoice(PressureQuantity.UnitChoicesEnum.MegapoundPerSquareInch));
            SemanticExample = GetSemanticExample();
        }
    }
}
