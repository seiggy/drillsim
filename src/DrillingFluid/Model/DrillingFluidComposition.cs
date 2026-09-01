using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class DrillingFluidComposition
    {
        /// <summary>
        /// an input list of Brine data
        /// </summary>
        public Brine BrineProperies { get; set; }
        /// <summary>
        /// an input list of base oil data
        /// </summary>
        public BaseOil BaseOilProperies { get; set; }
        public GaussianDrillingProperty WaterOilRatio { get; set; }
        public BaseCompositionProperties HighGravitySolid { get; set; }
        public BaseCompositionProperties LowGravitySolid { get; set; }
        public List<LostCirculationMaterial> LostCirculationMaterials { get; set; } 
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public DrillingFluidComposition() : base()
        {
        }
    }
}
