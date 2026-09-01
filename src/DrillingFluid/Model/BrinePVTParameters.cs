using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class BrinePVTParameters
    {
        
        /// <summary>
        /// Saline constant 0
        /// </summary>
        public GaussianDrillingProperty S0 { get; set; }
        /// <summary>
        /// Saline constant 1
        /// </summary>
        public GaussianDrillingProperty S1 { get; set; }
        /// <summary>
        /// Saline constant 2
        /// </summary>
        public GaussianDrillingProperty S2 { get; set; }
        /// <summary>
        /// Saline constant 3
        /// </summary>
        public GaussianDrillingProperty S3 { get; set; }
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty Bw { get; set; }
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty Cw { get; set; }
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty Dw { get; set; }
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty Ew { get; set; }
        /// <summary>
        /// PVT constant 
        /// </summary>
        public GaussianDrillingProperty Fw { get; set; }
        
        public BrinePVTParameters() : base()
        {
        }
    }
}
