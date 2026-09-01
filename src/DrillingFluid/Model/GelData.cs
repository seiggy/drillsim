using OSDC.DotnetLibraries.Drilling.DrillingProperties;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class GelData
    {
        /// <summary>
        /// Gel duration time
        /// </summary>
        public GaussianDrillingProperty GelDuration { get; set; }
        /// <summary>
        /// Gel strength for this duration
        /// </summary>
        public GaussianDrillingProperty GelStrength { get; set; }
        /// <summary>
        /// Temperature of the measurement
        /// </summary>
        public GaussianDrillingProperty Temperature { get; set; }
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public GelData() : base()
        {
        }
    }
}
