using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Linq.Expressions;

namespace OSDC.UnitConversion.Conversion
{
    public class SemanticFact
    {
        public string? Subject { get; set; } = null;
        public string? Verb { get; set; } = null;
        public string? Object { get; set; } = null;
        /// <summary>
        /// default constructor
        /// </summary>
        public SemanticFact()
        {

        }
        /// <summary>
        /// Initialization constructor
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="verb"></param>
        /// <param name="object"></param>
        public SemanticFact(string subject, string verb, string @object)
        {
            Subject = subject;
            Verb = verb;
            Object = @object;
        }
    }
    public partial class BasePhysicalQuantity
    {
        private static List<BasePhysicalQuantity> availableBasePhysicalQuantities_ = null;
        private static Dictionary<Guid, BasePhysicalQuantity> basePhysicalQuantitiesByGuid_ = null;
        private static Dictionary<string, BasePhysicalQuantity> basePhysicalQuantitiesByName_ = null;

        protected Dictionary<string, UnitChoice> unitChoicesByName_ = new Dictionary<string, UnitChoice>(StringComparer.OrdinalIgnoreCase);
        protected Dictionary<Guid, UnitChoice> unitChoicesByGuid_ = new Dictionary<Guid, UnitChoice>();

        public static List<BasePhysicalQuantity> AvailableBasePhysicalQuantities
        {
            get
            {
                if (availableBasePhysicalQuantities_ == null)
                {
                    Initialize();
                }
                return availableBasePhysicalQuantities_;
            }
        }

        /// <summary>
        /// the name of the physical quantity
        /// </summary>
        public string Name { get; protected set; } = null;
        /// <summary>
        /// the guid for that physical quantity
        /// </summary>
        public Guid ID { get; protected set; } = Guid.Empty;
        /// <summary>
        /// Description using the Markdown style
        /// </summary>
        public string DescriptionMD { get; protected set; } = string.Empty;

        internal void ReplaceDescription(string description)
        {
            DescriptionMD = description;
        }
        /// <summary>
        /// usual names of the physical quantity
        /// </summary>
        public HashSet<string> UsualNames { get; protected set; }

        internal void AddUsualNames(IEnumerable<string> names)
        {
            UsualNames = UsualNames == null
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(UsualNames, StringComparer.OrdinalIgnoreCase);
            UsualNames.UnionWith(names.Where(name => !string.IsNullOrWhiteSpace(name)));
        }
        /// <summary>
        /// the SI unit name for this base unit
        /// </summary>
        public virtual string SIUnitName { get; }
        /// <summary>
        /// the SI unit symbol for this base unit
        /// </summary>
        public virtual string SIUnitLabelLatex { get; }
        /// <summary>
        /// the possible non SI unit choices for this base unit
        /// </summary>
        public List<UnitChoice> UnitChoices { get; protected set; } = null;
        /// <summary>
        /// the physical dimension for length, abbreviated L
        /// </summary>
        public virtual double LengthDimension { get; } = 0;
        /// <summary>
        /// the physical dimension for mass, abbreviated M
        /// </summary>
        public virtual double MassDimension { get; } = 0;
        /// <summary>
        /// the physical dimension for time, abbreviated T
        /// </summary>
        public virtual double TimeDimension { get; } = 0;
        /// <summary>
        /// the physical dimension for temperature, abbreviated K
        /// </summary>
        public virtual double TemperatureDimension { get; } = 0;
        /// <summary>
        /// the physical dimension for amount of substance, abbreviated N
        /// </summary>
        public virtual double AmountSubstanceDimension { get; } = 0;
        /// <summary>
        /// the physical dimension for electric current, abbreviated I
        /// </summary>
        public virtual double ElectricCurrentDimension { get; } = 0;
        /// <summary>
        /// the physicsal dimension for luminous intensity, abbreviated J
        /// </summary>
        public virtual double LuminousIntensityDimension { get; } = 0;
        /// <summary>
        /// the physical dimension for angles in a plane, abbreviated θ
        /// </summary>
        public virtual double PlaneAngleDimension { get; } = 0;
        /// <summary>
        /// the physical dimension for solid angle, abbreviated Ω
        /// </summary>
        public virtual double SolidAngleDimension { get; } = 0;
        /// the smallest absolute value of the quantity that makes any sense with regards to its usage
        /// ex: drilling depth 0.001m, pipe diameter 0.0001m
        public virtual double? MeaningfulPrecisionInSI { get; } = null;
        /// <summary>
        /// the symbol used for this base unit in physical quantity descriptions: L, T, M, ...
        /// </summary>
        public virtual string TypicalSymbol { get; } = null;
        /// <summary>
        /// A list of semantic facts that describes an example where the quantity is used.
        /// </summary>
        public virtual List<SemanticFact> SemanticExample { get; protected set; } = null;
        /// <summary>
        /// return the unit label for the SI Unit choice as defined in the unit choices.
        /// </summary>
        public virtual string SIUnitLabel
        {
            get
            {
                UnitChoice? SIChoice = null;
                if (UnitChoices != null)
                {
                    foreach (var UnitChoice in UnitChoices)
                    {
                        if (UnitChoice != null && UnitChoice.IsSI)
                        {
                            SIChoice = UnitChoice;
                            break;
                        }
                    }
                }
                if (SIChoice != null && !string.IsNullOrEmpty(SIChoice.UnitLabel))
                {
                    return SIChoice.UnitLabel;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        /// <summary>
        /// return the latex representation of the physical dimension in between square brackets and the SI Unit Label in between parentheses.
        /// </summary>
        public string PhysicalDimensionLatex
        {
            get
            {
                string latex = GetDimensionsLatex();
                if (latex == null)
                {
                    latex = string.Empty;
                }
                if (!string.IsNullOrEmpty(SIUnitLabelLatex))
                {
                    if (!string.IsNullOrEmpty(latex))
                    {
                        latex += ",";
                    }
                    latex += "(" + SIUnitLabelLatex + ")";
                }
                if (!string.IsNullOrEmpty(latex))
                {
                    return "$" + latex + "$";
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        //////////////////////
        /// Static methods ///
        //////////////////////

        private static void Initialize()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(BasePhysicalQuantity));
            if (assembly != null)
            {
                foreach (Type typ in assembly.GetTypes())
                {
                    if (typ.IsSubclassOf(typeof(BasePhysicalQuantity)))
                    {
                        MethodInfo method = null;
                        foreach (MethodInfo meth in typ.GetMethods())
                        {
                            if (meth.IsStatic &&
                                meth.Name.EndsWith("Instance") &&
                                meth.ReturnType.IsSubclassOf(typeof(BasePhysicalQuantity)))
                            {
                                method = meth;
                                break;
                            }
                        }
                        // call the method
                        if (method != null)
                        {
                            object obj = method.Invoke(null, null);
                            if (obj != null)
                            {
                                var res = (BasePhysicalQuantity)obj;
                                if (availableBasePhysicalQuantities_ == null)
                                {
                                    availableBasePhysicalQuantities_ = new List<BasePhysicalQuantity>();
                                }
                                availableBasePhysicalQuantities_.Add(res);
                                if (basePhysicalQuantitiesByGuid_ == null)
                                {
                                    basePhysicalQuantitiesByGuid_ = new Dictionary<Guid, BasePhysicalQuantity>();
                                }
                                if (res.ID != Guid.Empty && !basePhysicalQuantitiesByGuid_.ContainsKey(res.ID))
                                {
                                    basePhysicalQuantitiesByGuid_.Add(res.ID, res);
                                }
                                else
                                {
                                    throw new Exception("problem with ID of physical quantity");
                                }
                                if (basePhysicalQuantitiesByName_ == null)
                                {
                                    basePhysicalQuantitiesByName_ = new Dictionary<string, BasePhysicalQuantity>(StringComparer.OrdinalIgnoreCase);
                                }
                                string lookupName = NormalizeQuantityLookupName(res.Name);
                                if (!string.IsNullOrEmpty(lookupName) && !basePhysicalQuantitiesByName_.ContainsKey(lookupName))
                                {
                                    basePhysicalQuantitiesByName_.Add(lookupName, res);
                                }
                                else
                                {
                                    throw new Exception("problem with name of physical quantity");
                                }
                            }
                        }
                    }
                }
                RegisterUniqueSynonyms(availableBasePhysicalQuantities_, basePhysicalQuantitiesByName_);
            }
        }

        protected static void RegisterUniqueSynonyms(
            IEnumerable<BasePhysicalQuantity>? quantities,
            Dictionary<string, BasePhysicalQuantity>? quantityLookup)
        {
            if (quantities == null || quantityLookup == null) return;

            var synonyms = new Dictionary<string, List<BasePhysicalQuantity>>(StringComparer.OrdinalIgnoreCase);
            foreach (BasePhysicalQuantity quantity in quantities)
            {
                foreach (string synonym in quantity.UsualNames ?? [])
                {
                    if (string.IsNullOrWhiteSpace(synonym)) continue;
                    string lookupName = NormalizeQuantityLookupName(synonym);
                    if (string.IsNullOrEmpty(lookupName)) continue;
                    if (!synonyms.TryGetValue(lookupName, out List<BasePhysicalQuantity>? matches))
                    {
                        matches = new List<BasePhysicalQuantity>();
                        synonyms.Add(lookupName, matches);
                    }
                    matches.Add(quantity);
                }
            }

            foreach ((string lookupName, List<BasePhysicalQuantity> matches) in synonyms)
            {
                BasePhysicalQuantity[] distinctMatches = matches.DistinctBy(quantity => quantity.ID).ToArray();
                if (distinctMatches.Length == 1 && !quantityLookup.ContainsKey(lookupName))
                {
                    quantityLookup.Add(lookupName, distinctMatches[0]);
                }
            }
        }

        protected static string NormalizeQuantityLookupName(string? name)
        {
            return string.IsNullOrWhiteSpace(name)
                ? string.Empty
                : new string(name.Where(char.IsLetterOrDigit).ToArray());
        }

        protected virtual void InitializeUnitChoices()
        {
            if (UnitChoices == null || UnitChoices.Count == 0)
            {
                Type typ = this.GetType();
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
                        var assies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !string.IsNullOrEmpty(assembly.Location));
                        object? res = unitChoiceDescriptions.GetValue(null);
                        if (res != null && res is List<UnitChoice>)
                        {
                            UnitChoices = (List<UnitChoice>)res;
                            foreach (var unitChoice in UnitChoices)
                            {
                                if (unitChoice != null && !string.IsNullOrEmpty(unitChoice.ConversionFactorFromSIFormula))
                                {
                                    double dres = CSharpScript.EvaluateAsync<double>(unitChoice.ConversionFactorFromSIFormula, ScriptOptions.Default.WithReferences(assies).WithImports("OSDC.UnitConversion.Conversion", "System.Math")).GetAwaiter().GetResult();
                                    if (!double.IsNaN(dres) && !double.IsInfinity(dres))
                                    {
                                        unitChoice.ConversionFactorFromSI = dres;
                                    }
                                }
                                if (unitChoice != null && !string.IsNullOrEmpty(unitChoice.ConversionBiasFromSIFormula))
                                {
                                    double dres = CSharpScript.EvaluateAsync<double>(unitChoice.ConversionBiasFromSIFormula, ScriptOptions.Default.WithReferences(assies).WithImports("OSDC.UnitConversion.Conversion", "System.Math")).GetAwaiter().GetResult();
                                    if (!double.IsNaN(dres) && !double.IsInfinity(dres))
                                    {
                                        unitChoice.ConversionBiasFromSI = dres;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public string GetDimensionsLatex()
        {
            double precision = 1e-6;
            List<Tuple<double, string>> dims = new List<Tuple<double, string>>();
            if (Math.Abs(LengthDimension) > precision)
            {
                dims.Add(new Tuple<double, string>(LengthDimension, "L"));
            }
            if (Math.Abs(MassDimension) > precision)
            {
                dims.Add(new Tuple<double, string>(MassDimension, "M"));
            }
            if (Math.Abs(TimeDimension) > precision)
            {
                dims.Add(new Tuple<double, string>(TimeDimension, "T"));
            }
            if (Math.Abs(TemperatureDimension) > precision)
            {
                dims.Add(new Tuple<double, string>(TemperatureDimension, "K"));
            }
            if (Math.Abs(AmountSubstanceDimension) > precision)
            {
                dims.Add(new Tuple<double, string>(AmountSubstanceDimension, "N"));
            }
            if (Math.Abs(ElectricCurrentDimension) > precision)
            {
                dims.Add(new Tuple<double, string>(ElectricCurrentDimension, "I"));
            }
            if (Math.Abs(LuminousIntensityDimension) > precision)
            {
                dims.Add(new Tuple<double, string>(LuminousIntensityDimension, "J"));
            }
            if (Math.Abs(PlaneAngleDimension) > precision)
            {
                dims.Add(new Tuple<double, string>(PlaneAngleDimension, "{\\theta}"));
            }
            if (Math.Abs(SolidAngleDimension) > precision)
            {
                dims.Add(new Tuple<double, string>(SolidAngleDimension, "{\\Omega}"));
            }
            dims.Sort((x, y) => y.Item1.CompareTo(x.Item1));
            string dimensions = string.Empty;
            foreach (var dim in dims)
            {
                dimensions += dim.Item2;
                if (Math.Abs(dim.Item1-1) > precision)
                {
                    dimensions += "^{" + dim.Item1 + "}";
                }
                dimensions += "";
            }
            if (!string.IsNullOrEmpty(dimensions))
            {
                dimensions = "[" + dimensions + "]";
            }
            return dimensions;
        }

        public string GetDimensionsEnclosed()
        {
            string dimensions = GetDimensionsLatex();
            if (!string.IsNullOrEmpty(dimensions))
            {
                dimensions = "$" + dimensions + "$";
            }
            return dimensions;
        }
        public static BasePhysicalQuantity GetQuantity(Guid ID)
        {
            BasePhysicalQuantity quantity = null;
            if (basePhysicalQuantitiesByGuid_ == null)
            {
                Initialize();
            }
            basePhysicalQuantitiesByGuid_.TryGetValue(ID, out quantity);
            return quantity;
        }
        public static BasePhysicalQuantity GetQuantity(string name)
        {
            BasePhysicalQuantity quantity = null;
            if (basePhysicalQuantitiesByName_ == null)
            {
                Initialize();
            }
            basePhysicalQuantitiesByName_.TryGetValue(NormalizeQuantityLookupName(name), out quantity);
            return quantity;
        }
        public static BasePhysicalQuantity GetQuantity(BasePhysicalQuantity.QuantityEnum choice)
        {
            BasePhysicalQuantity quantity = null;
            Guid guid;
            if (enumLookUp_.TryGetValue(choice, out guid))
            {
                if (basePhysicalQuantitiesByGuid_ == null)
                {
                    Initialize();
                }
                basePhysicalQuantitiesByGuid_.TryGetValue(guid, out quantity);
            }
            return quantity;
        }
        /// <summary>
        /// Getting assemblies is much harder than you think, be really cautious:
        /// https://dotnetcoretutorials.com/2020/07/03/getting-assemblies-is-harder-than-you-think-in-c/
        /// </summary>
        /// <returns></returns>
        private static Assembly[] GetAllAssemblies()
        {
            var assemblies = System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll").Select(x => Assembly.Load(AssemblyName.GetAssemblyName(x)));
            var returnAssemblies = new List<Assembly>();
            var loadedAssemblies = new HashSet<string>();
            var assembliesToCheck = new Queue<Assembly>();

            assembliesToCheck.Enqueue(Assembly.GetEntryAssembly());

            while (assembliesToCheck.Any())
            {
                var assemblyToCheck = assembliesToCheck.Dequeue();

                foreach (var reference in assemblyToCheck.GetReferencedAssemblies())
                {
                    if (!loadedAssemblies.Contains(reference.FullName))
                    {
                        var assembly = Assembly.Load(reference);
                        assembliesToCheck.Enqueue(assembly);
                        loadedAssemblies.Add(reference.FullName);
                        returnAssemblies.Add(assembly);
                    }
                }
            }
            return returnAssemblies.ToArray();
        }

        //////////////////////////////
        /// Administration methods ///
        //////////////////////////////
        
        protected void Reset()
        {
            if (UnitChoices == null)
            {
                UnitChoices = new List<UnitChoice>();
            }
            UnitChoices.Clear();
            unitChoicesByName_.Clear();
            unitChoicesByGuid_.Clear();
        }
        protected void PostProcess()
        {
            PhysicalQuantityDescriptionCatalog.Supplement(this);
            QuantitySynonymCatalog.Supplement(this);
            if (UnitChoices != null)
            {
                var alternativeNames = new Dictionary<string, List<UnitChoice>>(StringComparer.OrdinalIgnoreCase);
                foreach (UnitChoice choice in UnitChoices)
                {
                    if (!unitChoicesByName_.ContainsKey(choice.UnitName))
                    {
                        unitChoicesByName_.Add(choice.UnitName, choice);
                    }
                    else
                    {
                        throw new Exception("duplicate unit choice name");
                    }
                    if (!unitChoicesByGuid_.ContainsKey(choice.ID))
                    {
                        unitChoicesByGuid_.Add(choice.ID, choice);
                    }
                    else
                    {
                        throw new Exception("duplicate unit choice Guid");
                    }

                    choice.SupplementSynonyms();
                    AddAlternativeName(alternativeNames, choice.UnitLabel, choice);
                    foreach (string synonym in choice.Synonyms)
                    {
                        AddAlternativeName(alternativeNames, synonym, choice);
                    }
                }

                foreach ((string name, List<UnitChoice> choices) in alternativeNames)
                {
                    UnitChoice[] distinctChoices = choices.DistinctBy(choice => choice.ID).ToArray();
                    if (distinctChoices.Length == 1 && !unitChoicesByName_.ContainsKey(name))
                    {
                        unitChoicesByName_.Add(name, distinctChoices[0]);
                    }
                }
            }
        }

        private static void AddAlternativeName(Dictionary<string, List<UnitChoice>> names, string? name, UnitChoice choice)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            if (!names.TryGetValue(name, out List<UnitChoice>? choices))
            {
                choices = new List<UnitChoice>();
                names.Add(name, choices);
            }
            choices.Add(choice);
        }

        ///////////////////////
        /// Utility methods ///
        ///////////////////////

        /// <summary>
        /// return an alphabetically sorted list of the unit choice names
        /// </summary>
        /// <returns></returns>
        public List<string> GetUnitChoiceNames()
        {
            if (UnitChoices != null)
            {
                List<string> choices = new List<string>(UnitChoices.Count);
                foreach (UnitChoice choice in UnitChoices)
                {
                    choices.Add(choice.UnitName);
                }
                choices.Sort();
                return choices;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// return a dictionary of the unit choice names and IDs
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Guid> GetUnitChoiceIDs()
        {
            if (UnitChoices != null)
            {
                Dictionary<string, Guid> dict = new Dictionary<string, Guid>();
                foreach (UnitChoice choice in UnitChoices)
                {
                    dict.Add(choice.UnitName, choice.ID);
                }
                return dict;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// return the unit choice correpsonding to guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public UnitChoice GetUnitChoice(Guid guid)
        {
            UnitChoice c = null;
            if (unitChoicesByGuid_ != null && unitChoicesByGuid_.ContainsKey(guid))
            {
                unitChoicesByGuid_.TryGetValue(guid, out c);
            }
            return c;
        }
        /// <summary>
        /// return the unit choice corresponding to unitChoiceName
        /// </summary>
        /// <param name="unitChoiceName"></param>
        /// <returns></returns>
        public UnitChoice GetUnitChoice(string unitChoiceName)
        {
            UnitChoice c = null;
            if (unitChoicesByName_ != null && unitChoicesByName_.ContainsKey(unitChoiceName))
            {
                unitChoicesByName_.TryGetValue(unitChoiceName, out c);
            }
            return c;
        }
        /// <summary>
        /// convert a SI value into the unit choice, given a UnitChoice name
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unitChoiceName"></param>
        /// <returns>the converted value typed as a double</returns>
        public double FromSI(double value, string unitChoiceName)
        {
            UnitChoice unitChoice;
            if (unitChoicesByName_.TryGetValue(unitChoiceName, out unitChoice))
            {
                return unitChoice.FromSI(value);
            }
            else
            {
                throw new Exception("unknown unit choice");
            }
        }
        /// <summary>
        /// convert a SI value into the unit choice, given a UnitChoice ID
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unitChoiceID"></param>
        /// <returns>the converted value typed as a double</returns>
        public double FromSI(double value, Guid unitChoiceID)
        {
            UnitChoice unitChoice;
            if (unitChoicesByGuid_.TryGetValue(unitChoiceID, out unitChoice))
            {
                return unitChoice.FromSI(value);
            }
            else
            {
                throw new Exception("unknown unit choice");
            }
        }
        /// <summary>
        /// convert a SI value into the unit choice, given a UnitChoice name
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unitChoiceName"></param>
        /// <returns>the converted value typed as a string</returns>
        public string FromSIString(double value, string unitChoiceName)
        {
            UnitChoice unitChoice;
            if (unitChoicesByName_.TryGetValue(unitChoiceName, out unitChoice))
            {
                if (MeaningfulPrecisionInSI != null)
                {
                    return unitChoice.FromSI(value, MeaningfulPrecisionInSI);
                } else
                {
                    return unitChoice.FromSI(value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                }
            }
            else
            {
                throw new Exception("unknown unit choice");
            }
        }
        /// <summary>
        /// convert a SI value into the unit choice, given a UnitChoice ID
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unitChoiceID"></param>
        /// <returns>the converted value typed as a string</returns>
        public string FromSIString(double value, Guid unitChoiceID)
        {
            UnitChoice unitChoice;
            if (unitChoicesByGuid_.TryGetValue(unitChoiceID, out unitChoice))
            {
                if (MeaningfulPrecisionInSI != null)
                {
                    return unitChoice.FromSI(value, MeaningfulPrecisionInSI);
                }
                else
                {
                    return unitChoice.FromSI(value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                }
            }
            else
            {
                throw new Exception("unknown unit choice");
            }
        }
        /// <summary>
        /// convert a value from the unit choice to SI unit, given a UnitChoice ID
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unitChoiceName"></param>
        /// <returns>the converted value typed as a double</returns>
        public double ToSI(double value, Guid unitChoiceID)
        {
            UnitChoice unitChoice;
            if (unitChoicesByGuid_.TryGetValue(unitChoiceID, out unitChoice))
            {
                return unitChoice.ToSI(value);
            }
            else
            {
                throw new Exception("unknown unit choice");
            }
        }
        /// <summary>
        /// convert a value from the unit choice to SI unit, given a UnitChoice name
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unitChoiceName"></param>
        /// <returns>the converted value typed as a double</returns>
        public double ToSI(double value, string unitChoiceName)
        {
            UnitChoice unitChoice;
            if (unitChoicesByName_.TryGetValue(unitChoiceName, out unitChoice))
            {
                return unitChoice.ToSI(value);
            }
            else
            {
                throw new Exception("unknown unit choice");
            }
        }
        /// <summary>
        /// convert a value from the unit choice to SI unit, given a UnitChoice ID
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unitChoiceID"></param>
        /// <returns>the converted value typed as a string</returns>
        public string ToSIString(double value, Guid unitChoiceID)
        {
            UnitChoice unitChoice;
            if (unitChoicesByGuid_.TryGetValue(unitChoiceID, out unitChoice))
            {
                if (MeaningfulPrecisionInSI != null)
                {
                    return unitChoice.ToSI(value, MeaningfulPrecisionInSI);
                }
                else
                {
                    return unitChoice.ToSI(value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                }
            }
            else
            {
                throw new Exception("unknown unit choice");
            }
        }
        /// <summary>
        /// convert a value from the unit choice to SI unit
        /// </summary>
        /// <param name="value"></param>
        /// <param name="unitChoiceName"></param>
        /// <returns>the converted value typed as a string</returns>
        public string ToSIString(double value, string unitChoiceName)
        {
            UnitChoice unitChoice;
            if (unitChoicesByName_.TryGetValue(unitChoiceName, out unitChoice))
            {
                if (MeaningfulPrecisionInSI != null)
                {
                    return unitChoice.ToSI(value, MeaningfulPrecisionInSI);
                }
                else
                {
                    return unitChoice.ToSI(value).ToString(CultureInfo.InvariantCulture.NumberFormat);
                }
            }
            else
            {
                throw new Exception("unknown unit choice");
            }
        }
        protected virtual List<SemanticFact> GetSemanticExample()
        {
            return GetSemanticExample(Name);
        }
        protected virtual List<SemanticFact> GetSemanticExample(string radical)
        {
            if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(radical)) 
            {
                List<SemanticFact> result = new List<SemanticFact>();
                if (MeaningfulPrecisionInSI != null)
                {
                    result.Add(new SemanticFact(radical + "_Signal", "BelongsToClass", "DrillingSignal"));
                    result.Add(new SemanticFact(radical + "_DataPoint", "BelongsToClass", "DrillingDataPoint"));
                    result.Add(new SemanticFact(radical + "_DataPoint", "HasValue", radical + "_Signal"));
                    result.Add(new SemanticFact(radical + "_DataPoint", "IsOfMeasurableQuantity", Name + "Quantity"));
                    UnitChoice? SIUnitChoice = null;
                    if (UnitChoices != null)
                    {
                        foreach (UnitChoice choice in UnitChoices)
                        {
                            if (choice.IsSI)
                            {
                                SIUnitChoice = choice;
                                break;
                            }
                        }
                    }
                    if (SIUnitChoice != null && !string.IsNullOrEmpty(SIUnitChoice.GetVariableName()))
                    {
                        result.Add(new SemanticFact(radical + "_Signal", "HasUnitOfMeasure", SIUnitChoice.GetVariableName()));
                    }
                }
                else
                {
                    result.Add(new SemanticFact(Name + "Quantity", "BelongsToClass", "Quantity"));
                    if (LengthDimension != 0)
                    {
                        result.Add(new SemanticFact(Name + "Quantity." + "L", "=", LengthDimension.ToString(CultureInfo.InvariantCulture)));
                    }
                    if (MassDimension != 0)
                    {
                        result.Add(new SemanticFact(Name + "Quantity." + "M", "=", MassDimension.ToString(CultureInfo.InvariantCulture)));
                    }
                    if (TimeDimension != 0)
                    {
                        result.Add(new SemanticFact(Name + "Quantity." + "T", "=", TimeDimension.ToString(CultureInfo.InvariantCulture)));
                    }
                    if (TemperatureDimension != 0)
                    {
                        result.Add(new SemanticFact(Name + "Quantity." + "ThT", "=", TemperatureDimension.ToString(CultureInfo.InvariantCulture)));
                    }
                    if (AmountSubstanceDimension != 0)
                    {
                        result.Add(new SemanticFact(Name + "Quantity." + "N", "=", AmountSubstanceDimension.ToString(CultureInfo.InvariantCulture)));
                    }
                    if (ElectricCurrentDimension != 0)
                    {
                        result.Add(new SemanticFact(Name + "Quantity." + "I", "=", ElectricCurrentDimension.ToString(CultureInfo.InvariantCulture)));
                    }
                    if (LuminousIntensityDimension != 0)
                    {
                        result.Add(new SemanticFact(Name + "Quantity." + "J", "=", LuminousIntensityDimension.ToString(CultureInfo.InvariantCulture)));
                    }
                    if (PlaneAngleDimension != 0)
                    {
                        result.Add(new SemanticFact(Name + "Quantity." + "Theta", "=", PlaneAngleDimension.ToString(CultureInfo.InvariantCulture)));
                    }
                    if (SolidAngleDimension != 0)
                    {
                        result.Add(new SemanticFact(Name + "Quantity." + "Omega", "=", SolidAngleDimension.ToString(CultureInfo.InvariantCulture)));
                    }
                    if (UnitChoices != null)
                    {
                        foreach (UnitChoice choice in UnitChoices)
                        {
                            if (choice != null && !string.IsNullOrEmpty(choice.GetVariableName()))
                            {
                                result.Add(new SemanticFact(choice.GetVariableName(), "BelongsToClass", "Unit"));
                                result.Add(new SemanticFact(choice.GetVariableName() + ".ConversionFactorA", "=", choice.ConversionBiasFromSI.ToString(CultureInfo.InvariantCulture)));
                                result.Add(new SemanticFact(choice.GetVariableName() + ".ConversionFactorB", "=", choice.ConversionFactorFromSI.ToString(CultureInfo.InvariantCulture)));
                                if (!string.IsNullOrEmpty(choice.UnitLabel))
                                {
                                    result.Add(new SemanticFact(choice.GetVariableName() + ".Symbol", "=", "\"" + choice.UnitLabel + "\""));
                                }
                                result.Add(new SemanticFact(choice.GetVariableName(), "IsUnitForQuantity", Name + "Quantity"));
                                if (choice.IsSI)
                                {
                                    result.Add(new SemanticFact(Name + "Quantity", "HasSIUnit", choice.GetVariableName()));
                                }
                            }
                        }
                    }
                }
                return result;
            }
            else
            {
                return null;
            }
        }
     }
}
