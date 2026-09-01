using OSDC.DotnetLibraries.Drilling.DrillingProperties;
namespace NORCE.Drilling.DrillingFluid.Model
{
    public class BaseCompositionProperties
    {
        /// <summary>
        /// Density of the mass
        /// </summary>
        public GaussianDrillingProperty MassDensity { get; set; }
        /// <summary>
        /// Mass of the solid part
        /// </summary>
        public GaussianDrillingProperty MassFraction { get; set; }
        /// <summary>
        /// Specific heat capacity of the solid part
        /// </summary>
        public SpecificHeatCapacity SpecificHeatCapacity { get; set; }
        /// <summary>
        /// Thermal conductivity of the solid part
        /// </summary>
        public ThermalConductivity ThermalConductivity { get; set; }
        
        public BaseCompositionProperties() { }
    }
}