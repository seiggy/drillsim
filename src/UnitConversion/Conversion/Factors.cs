using System.Collections.Generic;

namespace OSDC.UnitConversion.Conversion
{
  public struct FactorDescription
  {
    public enum QualificationEnum { exact, approximate, standard, convention }
    public string Definition { get; set; }
    public QualificationEnum Qualification { get; set; }
    public string Reference { get; set; }
    public FactorDescription() { }
    public FactorDescription(string definition, QualificationEnum qualif, string reference)
    {
       Definition = definition;
       Qualification = qualif;
       Reference = reference;
    }
  }
  public static class Factors
  {
     public static Dictionary<string, FactorDescription> Descriptions = new Dictionary<string, FactorDescription>() {
{ "Unit", new FactorDescription("1.0", FactorDescription.QualificationEnum.exact , "")}
,{ "Deca", new FactorDescription("10.0", FactorDescription.QualificationEnum.exact , "")}
,{ "Hecto", new FactorDescription("100.0", FactorDescription.QualificationEnum.exact , "")}
,{ "Kilo", new FactorDescription("1000.0", FactorDescription.QualificationEnum.exact , "")}
,{ "Mega", new FactorDescription("1e6", FactorDescription.QualificationEnum.exact , "")}
,{ "Giga", new FactorDescription("1e9", FactorDescription.QualificationEnum.exact , "")}
,{ "Tera", new FactorDescription("1e12", FactorDescription.QualificationEnum.exact , "")}
,{ "Peta", new FactorDescription("1e15", FactorDescription.QualificationEnum.exact , "")}
,{ "Exa", new FactorDescription("1e18", FactorDescription.QualificationEnum.exact , "")}
,{ "Zetta", new FactorDescription("1e21", FactorDescription.QualificationEnum.exact , "")}
,{ "Yotta", new FactorDescription("1e24", FactorDescription.QualificationEnum.exact , "")}
,{ "Deci", new FactorDescription("0.1", FactorDescription.QualificationEnum.exact , "")}
,{ "Centi", new FactorDescription("0.01", FactorDescription.QualificationEnum.exact , "")}
,{ "Milli", new FactorDescription("0.001", FactorDescription.QualificationEnum.exact , "")}
,{ "Micro", new FactorDescription("1e-6", FactorDescription.QualificationEnum.exact , "")}
,{ "Nano", new FactorDescription("1e-9", FactorDescription.QualificationEnum.exact , "")}
,{ "Pico", new FactorDescription("1e-12", FactorDescription.QualificationEnum.exact , "")}
,{ "Femto", new FactorDescription("1e-15", FactorDescription.QualificationEnum.exact , "")}
,{ "Atto", new FactorDescription("1e-18", FactorDescription.QualificationEnum.exact , "")}
,{ "Zepto", new FactorDescription("1e-21", FactorDescription.QualificationEnum.exact , "")}
,{ "Yocto", new FactorDescription("1e-24", FactorDescription.QualificationEnum.exact , "")}
,{ "BitsPerByte", new FactorDescription("8.0", FactorDescription.QualificationEnum.exact , "")}
,{ "Angstrom", new FactorDescription("1e-10", FactorDescription.QualificationEnum.exact , "")}
,{ "AstronomicalUnit", new FactorDescription("149597870700.0", FactorDescription.QualificationEnum.exact , "https://www.iau.org/static/resolutions/IAU2012_English.pdf")}
,{ "LightYear", new FactorDescription("9460730472580800.0", FactorDescription.QualificationEnum.exact , "https://www.iau.org/public/themes/measuring/")}
,{ "Parsec", new FactorDescription("(60.0*60.0*180.0 / System.Math.PI) * Factors.AstronomicalUnit", FactorDescription.QualificationEnum.exact , "https://imagine.gsfc.nasa.gov/features/cosmic/milkyway_info.html")}
,{ "Inch", new FactorDescription("0.0254", FactorDescription.QualificationEnum.exact , "https://www.nist.gov/pml/owm/si-units-length")}
,{ "Foot", new FactorDescription("12.0 * Factors.Inch", FactorDescription.QualificationEnum.exact , "")}
,{ "Yard", new FactorDescription("3.0 * Factors.Foot", FactorDescription.QualificationEnum.exact , "")}
,{ "USSurveyFoot", new FactorDescription("1200.0 / 3937.0", FactorDescription.QualificationEnum.exact , "https://www.nist.gov/pml/us-surveyfoot")}
,{ "Fathom", new FactorDescription("6.0 * Factors.Foot", FactorDescription.QualificationEnum.exact , "https://www.britannica.com/science/fathom")}
,{ "SurveyorChain", new FactorDescription("22.0 * Factors.Yard", FactorDescription.QualificationEnum.exact , "https://www.britannica.com/technology/surveyors-chain")}
,{ "Mile", new FactorDescription("1760.0 * Factors.Yard", FactorDescription.QualificationEnum.exact , "https://dictionary.cambridge.org/dictionary/english/mile")}
,{ "InternationalNauticalMile", new FactorDescription("1852.0", FactorDescription.QualificationEnum.exact , "https://www.merriam-webster.com/dictionary/nautical%20mile")}
,{ "UKNauticalMile", new FactorDescription("6080 * Factors.Foot", FactorDescription.QualificationEnum.exact , "https://www.rmg.co.uk/stories/topics/nautical-mile")}
,{ "ScandinavianMile", new FactorDescription("10000.0", FactorDescription.QualificationEnum.exact , "")}
,{ "InchPer32", new FactorDescription("Factors.Inch / 32.0", FactorDescription.QualificationEnum.exact , "")}
,{ "Mil", new FactorDescription("0.001 * Factors.Inch", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Thousandth_of_an_inch")}
,{ "Thou", new FactorDescription("Factors.Mil", FactorDescription.QualificationEnum.exact , "")}
,{ "Hand", new FactorDescription("4.0 * Factors.Inch", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Hand_(unit)")}
,{ "Furlong", new FactorDescription("660.0 * Factors.Foot", FactorDescription.QualificationEnum.exact , "https://www.britannica.com/science/furlong")}
,{ "Degree", new FactorDescription("180.0 / System.Math.PI", FactorDescription.QualificationEnum.exact , "")}
,{ "Grad", new FactorDescription("200.0 / System.Math.PI", FactorDescription.QualificationEnum.exact , "")}
,{ "Revolution", new FactorDescription("2.0 * System.Math.PI", FactorDescription.QualificationEnum.exact , "")}
,{ "Minute", new FactorDescription("60.0", FactorDescription.QualificationEnum.exact , "")}
,{ "Hour", new FactorDescription("60.0 * Factors.Minute", FactorDescription.QualificationEnum.exact , "")}
,{ "Day", new FactorDescription("24.0 * Factors.Hour", FactorDescription.QualificationEnum.exact , "")}
,{ "Week", new FactorDescription("7.0 * Factors.Day", FactorDescription.QualificationEnum.exact , "")}
,{ "Fortnight", new FactorDescription("14.0 * Factors.Day", FactorDescription.QualificationEnum.exact , "")}
,{ "YearJulian", new FactorDescription("365.25 * Factors.Day", FactorDescription.QualificationEnum.exact , "")}
,{ "MonthCommon", new FactorDescription("Factors.YearJulian / 12.0", FactorDescription.QualificationEnum.exact , "")}
,{ "MonthSideral", new FactorDescription("27.32166 * Factors.Day", FactorDescription.QualificationEnum.approximate , "https://en.wikipedia.org/wiki/Month")}
,{ "MonthSynodic", new FactorDescription("29.53059 * Factors.Day", FactorDescription.QualificationEnum.approximate , "https://en.wikipedia.org/wiki/Month")}
,{ "QuarterCommon", new FactorDescription("Factors.YearJulian / 4.0", FactorDescription.QualificationEnum.exact , "")}
,{ "YearCommon", new FactorDescription("365 * Factors.Day", FactorDescription.QualificationEnum.exact , "")}
,{ "YearAverageGregorian", new FactorDescription("(365.0 + 97.0 / 400.0) * Factors.Day", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Gregorian_calendar")}
,{ "YearLeap", new FactorDescription("366 * Factors.Day", FactorDescription.QualificationEnum.exact , "")}
,{ "YearTropical", new FactorDescription("365.2422 * Factors.Day", FactorDescription.QualificationEnum.exact , "https://www.grc.nasa.gov/www/k-12/Numbers/Math/Mathematical_Thinking/calendar_calculations.htm")}
,{ "Decade", new FactorDescription("10.0 * Factors.YearJulian", FactorDescription.QualificationEnum.exact , "")}
,{ "Century", new FactorDescription("10.0 * Factors.Decade", FactorDescription.QualificationEnum.exact , "")}
,{ "Millenia", new FactorDescription("10.0 * Factors.Century", FactorDescription.QualificationEnum.exact , "")}
,{ "MillionYear", new FactorDescription("1000.0 * Factors.Millenia", FactorDescription.QualificationEnum.exact , "")}
,{ "ZeroCelsius", new FactorDescription("273.15", FactorDescription.QualificationEnum.exact , "https://www.nist.gov/pml/owm/si-units-temperature")}
,{ "FahrenheitSlope", new FactorDescription("5.0 / 9.0", FactorDescription.QualificationEnum.exact , "https://nn.wikipedia.org/wiki/Fahrenheit")}
,{ "FahrenheitBias", new FactorDescription("459.67", FactorDescription.QualificationEnum.exact , "https://nn.wikipedia.org/wiki/Fahrenheit")}
,{ "ReaumurSlope", new FactorDescription("5.0 / 4.0", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/R%C3%A9aumur_scale")}
,{ "ReaumurBias", new FactorDescription("Factors.ZeroCelsius / Factors.ReaumurSlope", FactorDescription.QualificationEnum.exact , "")}
,{ "C_cgs", new FactorDescription("2.99792458e10", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Statampere")}
,{ "Hefnerkerze", new FactorDescription("0.92", FactorDescription.QualificationEnum.approximate , "https://en.wikipedia.org/wiki/Hefner_lamp")}
,{ "InternationalCandle", new FactorDescription("1.02", FactorDescription.QualificationEnum.approximate , "")}
,{ "DVGWCandle", new FactorDescription("1.162 * Factors.Hefnerkerze", FactorDescription.QualificationEnum.approximate , "https://fr.wikipedia.org/wiki/Bougie_(unit%C3%A9)")}
,{ "Violle", new FactorDescription("60.0", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Jules_Violle")}
,{ "Carcel", new FactorDescription("9.74", FactorDescription.QualificationEnum.approximate , "https://en.wikipedia.org/wiki/Carcel")}
,{ "AtomicMass", new FactorDescription("1.66053906660e-27", FactorDescription.QualificationEnum.approximate , "https://en.wikipedia.org/wiki/Atomic_mass")}
,{ "Pound", new FactorDescription("0.45359237", FactorDescription.QualificationEnum.convention , "https://en.wikipedia.org/wiki/Pound_(mass)")}
,{ "Ounce", new FactorDescription("(1.0 / 16.0) * Factors.Pound", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Ounce")}
,{ "Stone", new FactorDescription("14.0 * Factors.Pound", FactorDescription.QualificationEnum.exact , "https://simple.wikipedia.org/wiki/Stone_(unit)")}
,{ "TonUK", new FactorDescription("2240.0 * Factors.Pound", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Ton")}
,{ "TonUS", new FactorDescription("2000.0 * Factors.Pound", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Ton")}
,{ "EarthMass", new FactorDescription("5.9722e24", FactorDescription.QualificationEnum.approximate , "https://en.wikipedia.org/wiki/Earth_mass")}
,{ "SolarMass", new FactorDescription("332946.0487 * Factors.EarthMass", FactorDescription.QualificationEnum.approximate , "https://en.wikipedia.org/wiki/Earth_mass")}
,{ "Grain", new FactorDescription("(1.0 / 7000.0) * Factors.Pound", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Grain_(unit)")}
,{ "Calorie", new FactorDescription("4.184", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Calorie")}
,{ "Litre", new FactorDescription("0.001", FactorDescription.QualificationEnum.exact , "")}
,{ "GallonUK", new FactorDescription("4.54609e-3", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Gallon")}
,{ "GallonUS", new FactorDescription("231.0 * Factors.Inch * Factors.Inch * Factors.Inch", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Gallon")}
,{ "Barrel", new FactorDescription("42.0 * Factors.GallonUS", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Barrel_(unit)")}
,{ "BTU", new FactorDescription("1054.35", FactorDescription.QualificationEnum.approximate , "https://en.wikipedia.org/wiki/British_thermal_unit")}
,{ "BTUGasPrice", new FactorDescription("1054.80", FactorDescription.QualificationEnum.approximate , "https://en.wikipedia.org/wiki/British_thermal_unit")}
,{ "Poise", new FactorDescription("0.1", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Poise_(unit)")}
,{ "G", new FactorDescription("9.80665", FactorDescription.QualificationEnum.standard , "https://en.wikipedia.org/wiki/Gravity_of_Earth")}
,{ "WaterDensity4degC1Atm", new FactorDescription("999.9720", FactorDescription.QualificationEnum.approximate , "https://en.wikipedia.org/wiki/Relative_density")}
,{ "MercuryDensity32degF", new FactorDescription("13595.065312221248", FactorDescription.QualificationEnum.approximate , "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")}
,{ "MercuryDensity60degF", new FactorDescription("13556.805881080776", FactorDescription.QualificationEnum.approximate , "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")}
,{ "SpecificGavity4degC", new FactorDescription("1.0 / Factors.WaterDensity4degC1Atm", FactorDescription.QualificationEnum.exact , "")}
,{ "PPGUK", new FactorDescription("Factors.Pound / Factors.GallonUK", FactorDescription.QualificationEnum.exact , "")}
,{ "PPGUS", new FactorDescription("Factors.Pound / Factors.GallonUS", FactorDescription.QualificationEnum.exact , "")}
,{ "Knot", new FactorDescription("1.852 * Factors.Kilo / Factors.Hour", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Knot_(unit)")}
,{ "PoundForce", new FactorDescription("Factors.Pound * Factors.G", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Pound_(force)")}
,{ "Dyne", new FactorDescription("1e-5", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Dyne")}
,{ "KilogramForce", new FactorDescription("1.0 * Factors.G", FactorDescription.QualificationEnum.standard , "https://en.wikipedia.org/wiki/Kilogram-force")}
,{ "Bar", new FactorDescription("1e5", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Bar_(unit)")}
,{ "PSI", new FactorDescription("Factors.PoundForce / (Factors.Inch * Factors.Inch)", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Pound_per_square_inch")}
,{ "Atmosphere", new FactorDescription("101325.0", FactorDescription.QualificationEnum.standard , "https://en.wikipedia.org/wiki/Atmospheric_pressure")}
,{ "Torr", new FactorDescription("(1.0 / 760.0) * Factors.Atmosphere", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Torr")}
,{ "MillimetreMercury", new FactorDescription("133.322387415", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Millimetre_of_mercury")}
,{ "InchMercury32degF", new FactorDescription("Factors.MercuryDensity32degF * Factors.G * Factors.Inch", FactorDescription.QualificationEnum.approximate , "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")}
,{ "InchMercury60degF", new FactorDescription("Factors.MercuryDensity60degF * Factors.G * Factors.Inch", FactorDescription.QualificationEnum.approximate , "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")}
,{ "MillimetreWater4degC", new FactorDescription("Factors.WaterDensity4degC1Atm * Factors.G * Factors.Milli", FactorDescription.QualificationEnum.approximate , "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")}
,{ "InchWater4degC", new FactorDescription("Factors.WaterDensity4degC1Atm * Factors.G * Factors.Inch", FactorDescription.QualificationEnum.approximate , "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")}
,{ "FootWater4degC", new FactorDescription("Factors.WaterDensity4degC1Atm * Factors.G * Factors.Foot", FactorDescription.QualificationEnum.approximate , "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")}
,{ "Gauss", new FactorDescription("1e-4", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Gauss_(unit)")}
,{ "Acre", new FactorDescription("Factors.SurveyorChain * Factors.Furlong", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Acre")}
,{ "PlanckConstant", new FactorDescription("6.62607015e-34", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Planck_constant")}
,{ "ElectronCharge", new FactorDescription("1.602176634e-19", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Elementary_charge")}
,{ "AvogadroConstant", new FactorDescription("6.02214076e23", FactorDescription.QualificationEnum.exact , "https://www.bipm.org/en/si-base-units/mole")}
,{ "Maxwell", new FactorDescription("1e-8", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Maxwell_(unit)")}
,{ "Line", new FactorDescription("1e-8", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Maxwell_(unit)")}
,{ "MagneticFluxQuantum", new FactorDescription("Factors.PlanckConstant / (2.0*Factors.ElectronCharge)", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Magnetic_flux_quantum")}
,{ "Darcy", new FactorDescription("0.0000001 / Factors.Atmosphere", FactorDescription.QualificationEnum.exact , "https://en.wikipedia.org/wiki/Darcy_(unit)")}
      };
    public static readonly double Unit = 1.0;
    public static readonly double Deca = 10.0;
    public static readonly double Hecto = 100.0;
    public static readonly double Kilo = 1000.0;
    public static readonly double Mega = 1e6;
    public static readonly double Giga = 1e9;
    public static readonly double Tera = 1e12;
    public static readonly double Peta = 1e15;
    public static readonly double Exa = 1e18;
    public static readonly double Zetta = 1e21;
    public static readonly double Yotta = 1e24;
    public static readonly double Deci = 0.1;
    public static readonly double Centi = 0.01;
    public static readonly double Milli = 0.001;
    public static readonly double Micro = 1e-6;
    public static readonly double Nano = 1e-9;
    public static readonly double Pico = 1e-12;
    public static readonly double Femto = 1e-15;
    public static readonly double Atto = 1e-18;
    public static readonly double Zepto = 1e-21;
    public static readonly double Yocto = 1e-24;
    public static readonly double BitsPerByte = 8.0;
    public static readonly double Angstrom = 1e-10;
    public static readonly double AstronomicalUnit = 149597870700.0;
    public static readonly double LightYear = 9460730472580800.0;
    public static readonly double Parsec = (60.0*60.0*180.0 / System.Math.PI) * Factors.AstronomicalUnit;
    public static readonly double Inch = 0.0254;
    public static readonly double Foot = 12.0 * Factors.Inch;
    public static readonly double Yard = 3.0 * Factors.Foot;
    public static readonly double USSurveyFoot = 1200.0 / 3937.0;
    public static readonly double Fathom = 6.0 * Factors.Foot;
    public static readonly double SurveyorChain = 22.0 * Factors.Yard;
    public static readonly double Mile = 1760.0 * Factors.Yard;
    public static readonly double InternationalNauticalMile = 1852.0;
    public static readonly double UKNauticalMile = 6080 * Factors.Foot;
    public static readonly double ScandinavianMile = 10000.0;
    public static readonly double InchPer32 = Factors.Inch / 32.0;
    public static readonly double Mil = 0.001 * Factors.Inch;
    public static readonly double Thou = Factors.Mil;
    public static readonly double Hand = 4.0 * Factors.Inch;
    public static readonly double Furlong = 660.0 * Factors.Foot;
    public static readonly double Degree = 180.0 / System.Math.PI;
    public static readonly double Grad = 200.0 / System.Math.PI;
    public static readonly double Revolution = 2.0 * System.Math.PI;
    public static readonly double Minute = 60.0;
    public static readonly double Hour = 60.0 * Factors.Minute;
    public static readonly double Day = 24.0 * Factors.Hour;
    public static readonly double Week = 7.0 * Factors.Day;
    public static readonly double Fortnight = 14.0 * Factors.Day;
    public static readonly double YearJulian = 365.25 * Factors.Day;
    public static readonly double MonthCommon = Factors.YearJulian / 12.0;
    public static readonly double MonthSideral = 27.32166 * Factors.Day;
    public static readonly double MonthSynodic = 29.53059 * Factors.Day;
    public static readonly double QuarterCommon = Factors.YearJulian / 4.0;
    public static readonly double YearCommon = 365 * Factors.Day;
    public static readonly double YearAverageGregorian = (365.0 + 97.0 / 400.0) * Factors.Day;
    public static readonly double YearLeap = 366 * Factors.Day;
    public static readonly double YearTropical = 365.2422 * Factors.Day;
    public static readonly double Decade = 10.0 * Factors.YearJulian;
    public static readonly double Century = 10.0 * Factors.Decade;
    public static readonly double Millenia = 10.0 * Factors.Century;
    public static readonly double MillionYear = 1000.0 * Factors.Millenia;
    public static readonly double ZeroCelsius = 273.15;
    public static readonly double FahrenheitSlope = 5.0 / 9.0;
    public static readonly double FahrenheitBias = 459.67;
    public static readonly double ReaumurSlope = 5.0 / 4.0;
    public static readonly double ReaumurBias = Factors.ZeroCelsius / Factors.ReaumurSlope;
    public static readonly double C_cgs = 2.99792458e10;
    public static readonly double Hefnerkerze = 0.92;
    public static readonly double InternationalCandle = 1.02;
    public static readonly double DVGWCandle = 1.162 * Factors.Hefnerkerze;
    public static readonly double Violle = 60.0;
    public static readonly double Carcel = 9.74;
    public static readonly double AtomicMass = 1.66053906660e-27;
    public static readonly double Pound = 0.45359237;
    public static readonly double Ounce = (1.0 / 16.0) * Factors.Pound;
    public static readonly double Stone = 14.0 * Factors.Pound;
    public static readonly double TonUK = 2240.0 * Factors.Pound;
    public static readonly double TonUS = 2000.0 * Factors.Pound;
    public static readonly double EarthMass = 5.9722e24;
    public static readonly double SolarMass = 332946.0487 * Factors.EarthMass;
    public static readonly double Grain = (1.0 / 7000.0) * Factors.Pound;
    public static readonly double Calorie = 4.184;
    public static readonly double Litre = 0.001;
    public static readonly double GallonUK = 4.54609e-3;
    public static readonly double GallonUS = 231.0 * Factors.Inch * Factors.Inch * Factors.Inch;
    public static readonly double Barrel = 42.0 * Factors.GallonUS;
    public static readonly double BTU = 1054.35;
    public static readonly double BTUGasPrice = 1054.80;
    public static readonly double Poise = 0.1;
    public static readonly double G = 9.80665;
    public static readonly double WaterDensity4degC1Atm = 999.9720;
    public static readonly double MercuryDensity32degF = 13595.065312221248;
    public static readonly double MercuryDensity60degF = 13556.805881080776;
    public static readonly double SpecificGavity4degC = 1.0 / Factors.WaterDensity4degC1Atm;
    public static readonly double PPGUK = Factors.Pound / Factors.GallonUK;
    public static readonly double PPGUS = Factors.Pound / Factors.GallonUS;
    public static readonly double Knot = 1.852 * Factors.Kilo / Factors.Hour;
    public static readonly double PoundForce = Factors.Pound * Factors.G;
    public static readonly double Dyne = 1e-5;
    public static readonly double KilogramForce = 1.0 * Factors.G;
    public static readonly double Bar = 1e5;
    public static readonly double PSI = Factors.PoundForce / (Factors.Inch * Factors.Inch);
    public static readonly double Atmosphere = 101325.0;
    public static readonly double Torr = (1.0 / 760.0) * Factors.Atmosphere;
    public static readonly double MillimetreMercury = 133.322387415;
    public static readonly double InchMercury32degF = Factors.MercuryDensity32degF * Factors.G * Factors.Inch;
    public static readonly double InchMercury60degF = Factors.MercuryDensity60degF * Factors.G * Factors.Inch;
    public static readonly double MillimetreWater4degC = Factors.WaterDensity4degC1Atm * Factors.G * Factors.Milli;
    public static readonly double InchWater4degC = Factors.WaterDensity4degC1Atm * Factors.G * Factors.Inch;
    public static readonly double FootWater4degC = Factors.WaterDensity4degC1Atm * Factors.G * Factors.Foot;
    public static readonly double Gauss = 1e-4;
    public static readonly double Acre = Factors.SurveyorChain * Factors.Furlong;
    public static readonly double PlanckConstant = 6.62607015e-34;
    public static readonly double ElectronCharge = 1.602176634e-19;
    public static readonly double AvogadroConstant = 6.02214076e23;
    public static readonly double Maxwell = 1e-8;
    public static readonly double Line = 1e-8;
    public static readonly double MagneticFluxQuantum = Factors.PlanckConstant / (2.0*Factors.ElectronCharge);
    public static readonly double Darcy = 0.0000001 / Factors.Atmosphere;
  }
}
