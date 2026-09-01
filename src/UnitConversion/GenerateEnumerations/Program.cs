using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;
using System.Reflection;

namespace OSDC.UnitConversion.GenerateEnumerations
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo? currentFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (currentFolder != null && !"UnitConversion".Equals(currentFolder.Name, StringComparison.OrdinalIgnoreCase))
            {
                currentFolder = currentFolder.Parent;
            }
            if (currentFolder == null)
            {
                throw new DirectoryNotFoundException("The generator must be run from within the UnitConversion repository.");
            }
            string baseFolder = currentFolder.FullName + Path.DirectorySeparatorChar;
            // generate the Factors class
            GenerateAtomically(baseFolder + "Conversion\\Factors.cs", GenerateFactors);
            List<BasePhysicalQuantity>? quantities = AvailableBasePhysicalQuantities;
            List<BasePhysicalQuantity>? drillingQuantities = AvailableDrillingQuantities;
            if (quantities != null)
            {
                ValidateQuantities(quantities, "base");
                GenerateAtomically(baseFolder + "Conversion\\Constructors.cs", path => GenerateConstructors(path, quantities));
                GenerateAtomically(baseFolder + "Conversion\\EnumerationQuantities.cs", path => GenerateEnumerations(path, typeof(BasePhysicalQuantity), quantities));
            }
            if (drillingQuantities != null)
            {
                ValidateQuantities(drillingQuantities, "drilling");
                GenerateAtomically(baseFolder + "Conversion.DrillingEngineering\\EnumerationQuantities.cs", path => GenerateEnumerations(path, typeof(DrillingPhysicalQuantity), drillingQuantities));
            }
        }

        private static void GenerateAtomically(string filename, Action<string> generate)
        {
            string temporaryFilename = filename + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                generate(temporaryFilename);
                File.Move(temporaryFilename, filename, true);
            }
            finally
            {
                if (File.Exists(temporaryFilename))
                {
                    File.Delete(temporaryFilename);
                }
            }
        }

        private static void ValidateQuantities(IEnumerable<BasePhysicalQuantity> quantities, string catalogName)
        {
            foreach (IGrouping<string, BasePhysicalQuantity> duplicate in quantities.GroupBy(quantity => quantity.ID.ToString()).Where(group => group.Count() > 1))
            {
                throw new InvalidOperationException($"The {catalogName} catalog has duplicate quantity ID '{duplicate.Key}'.");
            }
            foreach (IGrouping<string, BasePhysicalQuantity> duplicate in quantities.GroupBy(quantity => Convert(quantity.Name), StringComparer.Ordinal).Where(group => group.Count() > 1))
            {
                throw new InvalidOperationException($"The {catalogName} catalog has duplicate enumeration name '{duplicate.Key}'.");
            }
            foreach (BasePhysicalQuantity quantity in quantities)
            {
                IEnumerable<IGrouping<string, UnitChoice>> duplicateUnits = quantity.UnitChoices
                    .GroupBy(choice => choice.UnitName, StringComparer.OrdinalIgnoreCase)
                    .Where(group => group.Count() > 1);
                foreach (IGrouping<string, UnitChoice> duplicate in duplicateUnits)
                {
                    throw new InvalidOperationException($"Quantity '{quantity.Name}' has duplicate unit name '{duplicate.Key}'.");
                }
            }
        }
        private static List<Type> GetAllSubclasses(Type baseType)
        {
            List<Type> result = new List<Type>();
            Assembly? assy = Assembly.GetAssembly(baseType);
            if (assy != null)
            {
                Queue<Type> typesToProcess = new Queue<Type>();

                typesToProcess.Enqueue(baseType);

                while (typesToProcess.Count > 0)
                {
                    Type currentType = typesToProcess.Dequeue();

                    IEnumerable<Type> subclasses = assy.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(currentType))
                        .OrderBy(t => t.FullName, StringComparer.Ordinal);

                    foreach (var subclass in subclasses)
                    {
                        if (!result.Contains(subclass))
                        {
                            result.Add(subclass);
                            typesToProcess.Enqueue(subclass);
                        }
                    }
                }
            }
            return result;
        }
        private static List<BasePhysicalQuantity>? availableBasePhysicalQuantities_ = null;

        /// <summary>
        /// Prefer the hand-authored unit descriptions over a previously generated
        /// InitializeUnitChoices override. This removes the bootstrap cycle where a
        /// new choice could not be generated until the old generated constructor was
        /// edited or removed first.
        /// </summary>
        private static void RefreshUnitChoicesFromSource(BasePhysicalQuantity quantity, Type quantityType)
        {
            FieldInfo? descriptionsField = quantityType.GetField(
                "UnitChoiceDescriptions",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
            if (descriptionsField?.GetValue(null) is not List<UnitChoice> descriptions)
            {
                return;
            }

            PropertyInfo? unitChoicesProperty = typeof(BasePhysicalQuantity).GetProperty(nameof(BasePhysicalQuantity.UnitChoices));
            unitChoicesProperty?.SetValue(quantity, descriptions);
        }

        public static List<BasePhysicalQuantity>? AvailableBasePhysicalQuantities
        {
            get
            {
                if (availableBasePhysicalQuantities_ == null)
                {
                    List<Type> subclasses = GetAllSubclasses(typeof(BasePhysicalQuantity));
                    if (subclasses != null)
                    {
                        foreach (Type typ in subclasses)
                        {
                            if (typ.IsSubclassOf(typeof(BasePhysicalQuantity)))
                            {
                                MethodInfo? method = null;
                                foreach (MethodInfo meth in typ.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                                {
                                    if (meth.IsStatic &&
                                        meth.Name == "Instance" &&
                                        meth.ReturnType.IsSubclassOf(typeof(BasePhysicalQuantity)))
                                    {
                                        method = meth;
                                        break;
                                    }
                                }
                                // call the method
                                if (method != null)
                                {
                                    object? obj = method.Invoke(null, null);
                                    if (obj != null)
                                    {
                                        var res = (BasePhysicalQuantity)obj;
                                        RefreshUnitChoicesFromSource(res, typ);
                                        if (availableBasePhysicalQuantities_ == null)
                                        {
                                            availableBasePhysicalQuantities_ = new List<BasePhysicalQuantity>();
                                        }
                                        availableBasePhysicalQuantities_.Add(res);
                                    }
                                }
                            }
                        }
                    }
                }
                return availableBasePhysicalQuantities_;
            }
        }
        private static List<BasePhysicalQuantity>? availablePhysicalQuantities_ = null;

        public static List<BasePhysicalQuantity>? AvailableDrillingQuantities
        {
            get
            {
                if (availablePhysicalQuantities_ == null)
                {
                    Assembly? assembly = Assembly.GetAssembly(typeof(DrillingPhysicalQuantity));
                    if (assembly != null)
                    {
                        foreach (Type typ in assembly.GetTypes().OrderBy(t => t.FullName, StringComparer.Ordinal))
                        {
                            if (!typ.IsAbstract && typ.IsSubclassOf(typeof(BasePhysicalQuantity)))
                            {
                                MethodInfo? method = null;
                                foreach (MethodInfo meth in typ.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
                                {
                                    if (meth.IsStatic &&
                                        meth.Name == "Instance" &&
                                        meth.ReturnType.IsSubclassOf(typeof(BasePhysicalQuantity)))
                                    {
                                        method = meth;
                                        break;
                                    }
                                }
                                // call the method
                                if (method != null)
                                {
                                    object? obj = null;
                                    try
                                    {
                                        obj = method.Invoke(null, null);
                                    }
                                    catch (Exception exception)
                                    {
                                        throw new InvalidOperationException($"Could not create drilling quantity '{typ.FullName}'.", exception);
                                    }
                                    if (obj != null)
                                    {
                                        var res = (BasePhysicalQuantity)obj;
                                        RefreshUnitChoicesFromSource(res, typ);
                                        if (availablePhysicalQuantities_ == null)
                                        {
                                            availablePhysicalQuantities_ = new List<BasePhysicalQuantity>();
                                        }
                                        availablePhysicalQuantities_.Add(res);
                                    }
                                }
                            }
                        }
                    }
                }
                return availablePhysicalQuantities_;
            }
        }
        static void GenerateFactors(string filename)
        {
            Dictionary<string, FactorDescription> descriptions = new Dictionary<string, FactorDescription>() {
              { "Unit", new FactorDescription("1.0", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Deca", new FactorDescription("10.0", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Hecto", new FactorDescription("100.0", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Kilo", new FactorDescription("1000.0", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Mega", new FactorDescription("1e6", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Giga", new FactorDescription("1e9", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Tera", new FactorDescription("1e12", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Peta", new FactorDescription("1e15", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Exa", new FactorDescription("1e18", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Zetta", new FactorDescription("1e21", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Yotta", new FactorDescription("1e24", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Deci", new FactorDescription("0.1", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Centi", new FactorDescription("0.01", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Milli", new FactorDescription("0.001", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Micro", new FactorDescription("1e-6", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Nano", new FactorDescription("1e-9", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Pico", new FactorDescription("1e-12", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Femto", new FactorDescription("1e-15", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Atto", new FactorDescription("1e-18", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Zepto", new FactorDescription("1e-21", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Yocto", new FactorDescription("1e-24", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "BitsPerByte", new FactorDescription("8.0", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "Angstrom", new FactorDescription("1e-10", FactorDescription.QualificationEnum.exact, string.Empty)},
              { "AstronomicalUnit", new FactorDescription("149597870700.0", FactorDescription.QualificationEnum.exact, "https://www.iau.org/static/resolutions/IAU2012_English.pdf")},
              { "LightYear", new FactorDescription("9460730472580800.0", FactorDescription.QualificationEnum.exact, "https://www.iau.org/public/themes/measuring/")},
              { "Parsec", new FactorDescription("(60.0*60.0*180.0 / System.Math.PI) * Factors.AstronomicalUnit", FactorDescription.QualificationEnum.exact, "https://imagine.gsfc.nasa.gov/features/cosmic/milkyway_info.html")},
              { "Inch", new FactorDescription("0.0254", FactorDescription.QualificationEnum.exact, "https://www.nist.gov/pml/owm/si-units-length")},
              { "Foot", new FactorDescription("12.0 * Factors.Inch", FactorDescription.QualificationEnum.exact, "")},
              { "Yard", new FactorDescription("3.0 * Factors.Foot", FactorDescription.QualificationEnum.exact, "")},
              { "USSurveyFoot", new FactorDescription("1200.0 / 3937.0", FactorDescription.QualificationEnum.exact, "https://www.nist.gov/pml/us-surveyfoot")},
              { "Fathom", new FactorDescription("6.0 * Factors.Foot", FactorDescription.QualificationEnum.exact, "https://www.britannica.com/science/fathom")},
              { "SurveyorChain", new FactorDescription("22.0 * Factors.Yard", FactorDescription.QualificationEnum.exact, "https://www.britannica.com/technology/surveyors-chain")},
              { "Mile", new FactorDescription("1760.0 * Factors.Yard", FactorDescription.QualificationEnum.exact, "https://dictionary.cambridge.org/dictionary/english/mile")},
              { "InternationalNauticalMile", new FactorDescription("1852.0", FactorDescription.QualificationEnum.exact, "https://www.merriam-webster.com/dictionary/nautical%20mile")},
              { "UKNauticalMile", new FactorDescription("6080 * Factors.Foot", FactorDescription.QualificationEnum.exact, "https://www.rmg.co.uk/stories/topics/nautical-mile")},
              { "ScandinavianMile", new FactorDescription("10000.0", FactorDescription.QualificationEnum.exact, "")},
              { "InchPer32", new FactorDescription("Factors.Inch / 32.0", FactorDescription.QualificationEnum.exact, "")},
              { "Mil", new FactorDescription("0.001 * Factors.Inch", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Thousandth_of_an_inch")},
              { "Thou", new FactorDescription("Factors.Mil", FactorDescription.QualificationEnum.exact, "")},
              { "Hand", new FactorDescription("4.0 * Factors.Inch", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Hand_(unit)")},
              { "Furlong", new FactorDescription("660.0 * Factors.Foot", FactorDescription.QualificationEnum.exact, "https://www.britannica.com/science/furlong")},
              { "Degree", new FactorDescription("180.0 / System.Math.PI", FactorDescription.QualificationEnum.exact, "")},
              { "Grad", new FactorDescription("200.0 / System.Math.PI", FactorDescription.QualificationEnum.exact, "")},
              { "Revolution", new FactorDescription("2.0 * System.Math.PI", FactorDescription.QualificationEnum.exact, "")},
              { "Minute", new FactorDescription("60.0", FactorDescription.QualificationEnum.exact, "")},
              { "Hour", new FactorDescription("60.0 * Factors.Minute", FactorDescription.QualificationEnum.exact, "")},
              { "Day", new FactorDescription("24.0 * Factors.Hour", FactorDescription.QualificationEnum.exact, "")},
              { "Week", new FactorDescription("7.0 * Factors.Day", FactorDescription.QualificationEnum.exact, "")},
              { "Fortnight", new FactorDescription("14.0 * Factors.Day", FactorDescription.QualificationEnum.exact, "")},
              { "YearJulian", new FactorDescription("365.25 * Factors.Day", FactorDescription.QualificationEnum.exact, "")},
              { "MonthCommon", new FactorDescription("Factors.YearJulian / 12.0", FactorDescription.QualificationEnum.exact, "")},
              { "MonthSideral", new FactorDescription("27.32166 * Factors.Day", FactorDescription.QualificationEnum.approximate, "https://en.wikipedia.org/wiki/Month")},
              { "MonthSynodic", new FactorDescription("29.53059 * Factors.Day", FactorDescription.QualificationEnum.approximate, "https://en.wikipedia.org/wiki/Month")},
              { "QuarterCommon", new FactorDescription("Factors.YearJulian / 4.0", FactorDescription.QualificationEnum.exact, "")},
              { "YearCommon", new FactorDescription("365 * Factors.Day", FactorDescription.QualificationEnum.exact, "")},
              { "YearAverageGregorian", new FactorDescription("(365.0 + 97.0 / 400.0) * Factors.Day", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Gregorian_calendar")},
              { "YearLeap", new FactorDescription("366 * Factors.Day", FactorDescription.QualificationEnum.exact, "")},
              { "YearTropical", new FactorDescription("365.2422 * Factors.Day", FactorDescription.QualificationEnum.exact, "https://www.grc.nasa.gov/www/k-12/Numbers/Math/Mathematical_Thinking/calendar_calculations.htm")},
              { "Decade", new FactorDescription("10.0 * Factors.YearJulian", FactorDescription.QualificationEnum.exact, "")},
              { "Century", new FactorDescription("10.0 * Factors.Decade", FactorDescription.QualificationEnum.exact, "")},
              { "Millenia", new FactorDescription("10.0 * Factors.Century", FactorDescription.QualificationEnum.exact, "")},
              { "MillionYear", new FactorDescription("1000.0 * Factors.Millenia", FactorDescription.QualificationEnum.exact, "")},
              { "ZeroCelsius", new FactorDescription("273.15", FactorDescription.QualificationEnum.exact, "https://www.nist.gov/pml/owm/si-units-temperature")},
              { "FahrenheitSlope", new FactorDescription("5.0 / 9.0", FactorDescription.QualificationEnum.exact, "https://nn.wikipedia.org/wiki/Fahrenheit")},
              { "FahrenheitBias", new FactorDescription("459.67", FactorDescription.QualificationEnum.exact, "https://nn.wikipedia.org/wiki/Fahrenheit")},
              { "ReaumurSlope", new FactorDescription("5.0 / 4.0", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/R%C3%A9aumur_scale")},
              { "ReaumurBias", new FactorDescription("Factors.ZeroCelsius / Factors.ReaumurSlope", FactorDescription.QualificationEnum.exact, "")},
              { "C_cgs", new FactorDescription("2.99792458e10", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Statampere")},
              { "Hefnerkerze", new FactorDescription("0.92", FactorDescription.QualificationEnum.approximate, "https://en.wikipedia.org/wiki/Hefner_lamp")},
              { "InternationalCandle", new FactorDescription("1.02", FactorDescription.QualificationEnum.approximate, "")},
              { "DVGWCandle", new FactorDescription("1.162 * Factors.Hefnerkerze", FactorDescription.QualificationEnum.approximate, "https://fr.wikipedia.org/wiki/Bougie_(unit%C3%A9)")},
              { "Violle", new FactorDescription("60.0", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Jules_Violle")},
              { "Carcel", new FactorDescription("9.74", FactorDescription.QualificationEnum.approximate, "https://en.wikipedia.org/wiki/Carcel")},
              { "AtomicMass", new FactorDescription("1.66053906660e-27", FactorDescription.QualificationEnum.approximate, "https://en.wikipedia.org/wiki/Atomic_mass")},
              { "Pound", new FactorDescription("0.45359237", FactorDescription.QualificationEnum.convention, "https://en.wikipedia.org/wiki/Pound_(mass)")},
              { "Ounce", new FactorDescription("(1.0 / 16.0) * Factors.Pound", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Ounce")},
              { "Stone", new FactorDescription("14.0 * Factors.Pound", FactorDescription.QualificationEnum.exact, "https://simple.wikipedia.org/wiki/Stone_(unit)")},
              { "TonUK", new FactorDescription("2240.0 * Factors.Pound", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Ton")},
              { "TonUS", new FactorDescription("2000.0 * Factors.Pound", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Ton")},
              { "EarthMass", new FactorDescription("5.9722e24", FactorDescription.QualificationEnum.approximate, "https://en.wikipedia.org/wiki/Earth_mass")},
              { "SolarMass", new FactorDescription("332946.0487 * Factors.EarthMass", FactorDescription.QualificationEnum.approximate, "https://en.wikipedia.org/wiki/Earth_mass")},
              { "Grain", new FactorDescription("(1.0 / 7000.0) * Factors.Pound", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Grain_(unit)")},
              { "Calorie", new FactorDescription("4.184", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Calorie")},
              { "Litre", new FactorDescription("0.001", FactorDescription.QualificationEnum.exact, "")},
              { "GallonUK", new FactorDescription("4.54609e-3", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Gallon")},
              { "GallonUS", new FactorDescription("231.0 * Factors.Inch * Factors.Inch * Factors.Inch", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Gallon")},
              { "Barrel", new FactorDescription("42.0 * Factors.GallonUS", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Barrel_(unit)")},
              { "BTU", new FactorDescription("1054.35", FactorDescription.QualificationEnum.approximate, "https://en.wikipedia.org/wiki/British_thermal_unit")},
              { "BTUGasPrice", new FactorDescription("1054.80", FactorDescription.QualificationEnum.approximate, "https://en.wikipedia.org/wiki/British_thermal_unit")},
              { "Poise", new FactorDescription("0.1", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Poise_(unit)")},
              { "G", new FactorDescription("9.80665", FactorDescription.QualificationEnum.standard, "https://en.wikipedia.org/wiki/Gravity_of_Earth")},
              { "WaterDensity4degC1Atm", new FactorDescription("999.9720", FactorDescription.QualificationEnum.approximate, "https://en.wikipedia.org/wiki/Relative_density")},
              { "MercuryDensity32degF", new FactorDescription("13595.065312221248", FactorDescription.QualificationEnum.approximate, "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")},
              { "MercuryDensity60degF", new FactorDescription("13556.805881080776", FactorDescription.QualificationEnum.approximate, "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")},
              { "SpecificGavity4degC", new FactorDescription("1.0 / Factors.WaterDensity4degC1Atm", FactorDescription.QualificationEnum.exact, "")},
              { "PPGUK", new FactorDescription("Factors.Pound / Factors.GallonUK", FactorDescription.QualificationEnum.exact, "")},
              { "PPGUS", new FactorDescription("Factors.Pound / Factors.GallonUS", FactorDescription.QualificationEnum.exact, "")},
              { "Knot", new FactorDescription("1.852 * Factors.Kilo / Factors.Hour", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Knot_(unit)")},
              { "PoundForce", new FactorDescription("Factors.Pound * Factors.G", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Pound_(force)")},
              { "Dyne", new FactorDescription("1e-5", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Dyne")},
              { "KilogramForce", new FactorDescription("1.0 * Factors.G", FactorDescription.QualificationEnum.standard, "https://en.wikipedia.org/wiki/Kilogram-force")},
              { "Bar", new FactorDescription("1e5", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Bar_(unit)")},
              { "PSI", new FactorDescription("Factors.PoundForce / (Factors.Inch * Factors.Inch)", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Pound_per_square_inch")},
              { "Atmosphere", new FactorDescription("101325.0", FactorDescription.QualificationEnum.standard, "https://en.wikipedia.org/wiki/Atmospheric_pressure")},
              { "Torr", new FactorDescription("(1.0 / 760.0) * Factors.Atmosphere", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Torr")},
              { "MillimetreMercury", new FactorDescription("133.322387415", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Millimetre_of_mercury")},
              { "InchMercury32degF", new FactorDescription("Factors.MercuryDensity32degF * Factors.G * Factors.Inch", FactorDescription.QualificationEnum.approximate, "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")},
              { "InchMercury60degF", new FactorDescription("Factors.MercuryDensity60degF * Factors.G * Factors.Inch", FactorDescription.QualificationEnum.approximate, "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")},
              { "MillimetreWater4degC", new FactorDescription("Factors.WaterDensity4degC1Atm * Factors.G * Factors.Milli", FactorDescription.QualificationEnum.approximate, "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")},
              { "InchWater4degC", new FactorDescription("Factors.WaterDensity4degC1Atm * Factors.G * Factors.Inch", FactorDescription.QualificationEnum.approximate, "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")},
              { "FootWater4degC", new FactorDescription("Factors.WaterDensity4degC1Atm * Factors.G * Factors.Foot", FactorDescription.QualificationEnum.approximate, "https://www.nist.gov/pml/special-publication-811/nist-guide-si-appendix-b-conversion-factors/nist-guide-si-appendix-b9")},
              { "Gauss", new FactorDescription("1e-4", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Gauss_(unit)")},
              { "Acre", new FactorDescription("Factors.SurveyorChain * Factors.Furlong", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Acre")},
              { "PlanckConstant", new FactorDescription("6.62607015e-34", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Planck_constant")},
              { "ElectronCharge", new FactorDescription("1.602176634e-19", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Elementary_charge")},
              { "AvogadroConstant", new FactorDescription("6.02214076e23", FactorDescription.QualificationEnum.exact, "https://www.bipm.org/en/si-base-units/mole")},
              { "Maxwell", new FactorDescription("1e-8", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Maxwell_(unit)")},
              { "Line", new FactorDescription("1e-8", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Maxwell_(unit)")},
              { "MagneticFluxQuantum", new FactorDescription("Factors.PlanckConstant / (2.0*Factors.ElectronCharge)", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Magnetic_flux_quantum")},
              { "Darcy", new FactorDescription("0.0000001 / Factors.Atmosphere", FactorDescription.QualificationEnum.exact, "https://en.wikipedia.org/wiki/Darcy_(unit)")}
            };
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("");
                writer.WriteLine("namespace OSDC.UnitConversion.Conversion");
                writer.WriteLine("{");
                writer.WriteLine("  public struct FactorDescription");
                writer.WriteLine("  {");
                writer.WriteLine("    public enum QualificationEnum { exact, approximate, standard, convention }");
                writer.WriteLine("    public string Definition { get; set; }");
                writer.WriteLine("    public QualificationEnum Qualification { get; set; }");
                writer.WriteLine("    public string Reference { get; set; }");
                writer.WriteLine("    public FactorDescription() { }");
                writer.WriteLine("    public FactorDescription(string definition, QualificationEnum qualif, string reference)");
                writer.WriteLine("    {");
                writer.WriteLine("       Definition = definition;");
                writer.WriteLine("       Qualification = qualif;");
                writer.WriteLine("       Reference = reference;");
                writer.WriteLine("    }");
                writer.WriteLine("  }");
                writer.WriteLine("  public static class Factors");
                writer.WriteLine("  {");
                writer.WriteLine("     public static Dictionary<string, FactorDescription> Descriptions = new Dictionary<string, FactorDescription>() {");
                bool first = true;
                foreach (var kpv in descriptions)
                {
                    writer.WriteLine(((!first)?",": "") + "{ \"" + kpv.Key + "\", new FactorDescription(\"" + kpv.Value.Definition + "\", FactorDescription.QualificationEnum." + kpv.Value.Qualification + " , \"" + ((kpv.Value.Reference == null) ? "" : kpv.Value.Reference) + "\")}");
                    first = false;
                }
                writer.WriteLine("      };");
                foreach (var kpv in descriptions)
                {
                    writer.WriteLine("    public static readonly double " + kpv.Key + " = " + kpv.Value.Definition + ";"); ;
                }
                writer.WriteLine("  }");
                writer.WriteLine("}");
            }
        }

        static void GenerateConstructors(string filename, List<BasePhysicalQuantity> quantities)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine();
                foreach (BasePhysicalQuantity quantity in quantities)
                {
                    Type typ = quantity.GetType();
                    if (typ != null && !string.IsNullOrEmpty(typ.Namespace) && typ.BaseType != null)
                    {
                        FieldInfo[] staticFields = typ.GetFields(BindingFlags.Static | BindingFlags.Public);
                        FieldInfo? unitChoiceDescriptions = null;
                        foreach (var field in staticFields)
                        {
                            if (field != null && "UnitChoiceDescriptions".Equals(field.Name) && field.FieldType == typeof(List<UnitChoice>))
                            {
                                unitChoiceDescriptions = field;
                                break;
                            }
                        }
                        if (unitChoiceDescriptions != null)
                        {
                            object? res = unitChoiceDescriptions.GetValue(null);
                            if (res != null && res is List<UnitChoice>)
                            {
                                List<UnitChoice> choices = (List<UnitChoice>)res;
                                string namesp = typ.Namespace;
                                string namecl = typ.Name;
                                string parentCl = typ.BaseType.Name;
                                writer.WriteLine("namespace " + namesp);
                                writer.WriteLine("{");
                                writer.WriteLine("  public partial class " + namecl + " : " + parentCl);
                                writer.WriteLine("  {");
                                writer.WriteLine("      protected override void InitializeUnitChoices()");
                                writer.WriteLine("      {");
                                writer.WriteLine("          UnitChoices = new List<UnitChoice>()");
                                writer.WriteLine("            {");
                                // find the SI Unit choice
                                UnitChoice? SIUnitChoice = null;
                                foreach (UnitChoice choice in choices)
                                {
                                    if (choice != null && choice.IsSI)
                                    {
                                        SIUnitChoice = choice;
                                        break;
                                    }
                                }
                                bool first = true;
                                foreach (UnitChoice choice in choices)
                                {
                                    writer.Write("              ");
                                    if (first)
                                    {
                                        writer.Write("  ");
                                    }
                                    else
                                    {
                                        writer.Write(", ");
                                    }
                                    first = false;
                                    writer.WriteLine("new UnitChoice");
                                    writer.WriteLine("                {");
                                    writer.WriteLine("                  UnitName = \"" + choice.UnitName + "\",");
                                    writer.WriteLine("                  UnitLabel = \"" + Process(choice.UnitLabel) + "\",");
                                    if (choice.Synonyms != null && choice.Synonyms.Count > 0)
                                    {
                                        string synonyms = string.Join(", ", choice.Synonyms
                                            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                                            .Select(value => "\"" + Process(value) + "\""));
                                        writer.WriteLine("                  Synonyms = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { " + synonyms + " },");
                                    }
                                    if (SIUnitChoice != null && !string.IsNullOrEmpty(SIUnitChoice.UnitName))
                                    {
                                        writer.WriteLine("                  SIUnitName = \"" + Process(SIUnitChoice.UnitName) + "\",");
                                    }
                                    writer.WriteLine("                  ID = new Guid(\"" + choice.ID.ToString() + "\"),");
                                    if (!string.IsNullOrEmpty(choice.ConversionFactorFromSIFormula))
                                    {
                                        writer.WriteLine("                  ConversionFactorFromSIFormula = \"" + choice.ConversionFactorFromSIFormula + "\",");
                                        writer.WriteLine("                  ConversionFactorFromSI = " + choice.ConversionFactorFromSIFormula + ",");
                                    }
                                    if (!string.IsNullOrEmpty(choice.ConversionBiasFromSIFormula))
                                    {
                                        writer.WriteLine("                  ConversionBiasFromSIFormula = \"" + choice.ConversionBiasFromSIFormula + "\",");
                                        writer.WriteLine("                  ConversionBiasFromSI = " + choice.ConversionBiasFromSIFormula + ",");
                                    }
                                    try
                                    {
                                        string desc = choice.GetConversionDescription();
                                        if (!string.IsNullOrEmpty(desc))
                                        {
                                            string[] lines = desc.Split(Environment.NewLine);
                                            if (lines != null && lines.Length > 0)
                                            {
                                                writer.WriteLine("                  ConversionDescription =");
                                                bool firstLine = true;
                                                foreach (var line in lines)
                                                {
                                                    if (!firstLine)
                                                    {
                                                        writer.WriteLine();
                                                        writer.Write(" + Environment.NewLine + ");
                                                    }
                                                    firstLine = false;
                                                    writer.Write("\"" + line + "\"");
                                                }
                                                writer.WriteLine(",");
                                            }
                                        }
                                    }
                                    catch (Exception exception)
                                    {
                                        throw new InvalidOperationException($"Could not describe conversion for '{typ.FullName}' / '{choice.UnitName}'.", exception);
                                    }
                                    if (choice.IsSI)
                                    {
                                        writer.WriteLine("                  IsSI = " + choice.IsSI.ToString().ToLower() + "");
                                    }
                                    writer.WriteLine("                }");
                                }
                                writer.WriteLine("            };");
                                writer.WriteLine("      }");
                                writer.WriteLine("  }");
                                writer.WriteLine("}");
                            }
                        }
                    }
                }
            }
        }
        static void GenerateEnumerations(string filename, Type typ, List<BasePhysicalQuantity> quantities)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine();
                if (typ != null && !string.IsNullOrEmpty(typ.Namespace) && typ.BaseType != null)
                {
                    string namesp = typ.Namespace;
                    string namecl = typ.Name;
                    string parentCl = typ.BaseType.Name;
                    writer.WriteLine("namespace " + namesp);
                    writer.WriteLine("{");
                    writer.WriteLine("  public partial class " + namecl + " : " + parentCl);
                    writer.WriteLine("  {");
                    writer.WriteLine("     public new enum QuantityEnum ");
                    writer.WriteLine("       {");
                    for (int i = 0; i < quantities.Count; i++)
                    {
                        BasePhysicalQuantity quantity = quantities[i];
                        string quantityName = quantity.Name;
                        string enumName = Convert(quantityName);
                        writer.WriteLine("         " + enumName + ((i < quantities.Count - 1) ? ", " : "") + " // " + quantityName);
                    }
                    writer.WriteLine("       }");
                    writer.WriteLine("    protected static new Dictionary<QuantityEnum, Guid> enumLookUp_ = new Dictionary<QuantityEnum, Guid>()");
                    writer.WriteLine("    {");
                    for (int i = 0; i < quantities.Count; i++)
                    {
                        BasePhysicalQuantity quantity = quantities[i];
                        string quantityName = quantity.Name;
                        string enumName = Convert(quantityName);
                        writer.WriteLine("         {QuantityEnum." + enumName + ", new Guid(\"" + quantity.ID.ToString() + "\")}" + ((i < quantities.Count - 1) ? ", " : "") + " // " + quantityName);
                    }
                    writer.WriteLine("    };");
                    writer.WriteLine("  }");
                    writer.WriteLine("}");
                    writer.WriteLine();
                }
                foreach (BasePhysicalQuantity quantity in quantities)
                {
                    typ = quantity.GetType();
                    if (typ != null && !string.IsNullOrEmpty(typ.Namespace) && typ.BaseType != null)
                    {
                        string namesp = typ.Namespace;
                        string namecl = typ.Name;
                        string parentCl = typ.BaseType.Name;
                        writer.WriteLine("namespace " + namesp);
                        writer.WriteLine("{");
                        writer.WriteLine("  public partial class " + namecl + " : " + parentCl);
                        writer.WriteLine("  {");
                        writer.WriteLine("    public new enum UnitChoicesEnum");
                        writer.WriteLine("      {");
                        if (quantity.UnitChoices != null)
                        {
                            for (int i = 0; i < quantity.UnitChoices.Count; i++)
                            {
                                UnitChoice choice = quantity.UnitChoices[i];
                                string choiceName = choice.UnitName;
                                string enumName = Convert(choiceName);
                                writer.WriteLine("         " + enumName + ((i < quantity.UnitChoices.Count - 1) ? ", " : "") + " // " + choiceName);
                            }
                        }
                        writer.WriteLine("      }");
                        writer.WriteLine("    protected new Dictionary<UnitChoicesEnum, Guid> enumLookUp_ = new Dictionary<UnitChoicesEnum, Guid>()");
                        writer.WriteLine("    {");
                        if (quantity.UnitChoices != null)
                        {
                            for (int i = 0; i < quantity.UnitChoices.Count; i++)
                            {
                                UnitChoice choice = quantity.UnitChoices[i];
                                string choiceName = choice.UnitName;
                                string enumName = Convert(choiceName);
                                writer.WriteLine("         {UnitChoicesEnum." + enumName + ", new Guid(\"" + choice.ID.ToString() + "\")}" + ((i < quantity.UnitChoices.Count - 1) ? ", " : "") + " // " + choiceName);
                            }
                        }
                        writer.WriteLine("    };");
                        writer.WriteLine("    public UnitChoice GetUnitChoice(UnitChoicesEnum choice)");
                        writer.WriteLine("    {");
                        writer.WriteLine("       UnitChoice c = null;");
                        writer.WriteLine("       Guid guid;");
                        writer.WriteLine("       if (enumLookUp_.TryGetValue(choice, out guid))");
                        writer.WriteLine("       {");
                        writer.WriteLine("         c = GetUnitChoice(guid);");
                        writer.WriteLine("       }");
                        writer.WriteLine("       return c;");
                        writer.WriteLine("    }");
                        writer.WriteLine("  }");
                        writer.WriteLine("}");
                    }
                }
            }
        }
        static string Process(string str)
        {
            if (!string.IsNullOrEmpty(str) && str.Contains("\""))
            {
                str = str.Replace("\"", "\\\"");
            }
            return str;
        }
        static string Convert(string name)
        {
            string[] tokens = name.Split(' ');
            string convertedName = "";
            foreach (string token in tokens)
            {
                if (!string.IsNullOrEmpty(token))
                {
                    string capitalizedToken = token.Substring(0, 1).ToUpper() + token.Substring(1);
                    capitalizedToken = capitalizedToken.Replace('\'', '_');
                    capitalizedToken = capitalizedToken.Replace('\\', '_');
                    capitalizedToken = capitalizedToken.Replace('/', '_');
                    capitalizedToken = capitalizedToken.Replace("(", "");
                    capitalizedToken = capitalizedToken.Replace(")", "");
                    capitalizedToken = capitalizedToken.Replace("-", "");
                    convertedName += capitalizedToken;
                }
            }
            return convertedName;
        }
    }
}
