using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSDC.UnitConversion.Conversion.DrillingEngineering
{

    public partial class AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity : AngleMagneticFluxDensityQuantity
    {
        public override double? MeaningfulPrecisionInSI { get; } = 1e-9;

        private static AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity instance_ = null;

        public static new AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity Instance
        {
            get
            {
                if (instance_ == null)
                {
                    instance_ = new AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity();
                    instance_.PostProcess();
                }
                return instance_;
            }
        }

        public AngleMagneticFluxDensitySurveyInstrumentDrillingQuantity() : base()
        {
            Name = this.GetType().Name.Split("Quantity").ElementAt(0);
            UsualNames = new HashSet<string>() { "angle magnetic flux density (survey instrument) (drilling)" };
            ID = new Guid("a3ad9fd6-a516-42c8-98a5-14e178dc62f2");
            DescriptionMD = base.DescriptionMD;
            DescriptionMD += Environment.NewLine;
            DescriptionMD += "The meaningful precision of angle magnetic flux density related to survey instrument performance models in the drilling context is typically: " + MeaningfulPrecisionInSI.ToString() + " " + AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianTesla).UnitLabel + Environment.NewLine;
            Reset();
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianTesla));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeGauss));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeMaxwellPerSquareCentimetre));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeMicrotesla));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeMilligauss));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeMillitesla));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeNanotesla));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeTesla));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.DegreeWeberPerSquareMetre));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianGauss));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianMaxwellPerSquareCentimetre));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianMicrotesla));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianMilligauss));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianMillitesla));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianNanotesla));
            this.UnitChoices.Add(AngleMagneticFluxDensityQuantity.Instance.GetUnitChoice(AngleMagneticFluxDensityQuantity.UnitChoicesEnum.RadianWeberPerSquareMetre));
            SemanticExample = GetSemanticExample();
        }
    }
}
