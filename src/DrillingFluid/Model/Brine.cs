using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;

namespace NORCE.Drilling.DrillingFluid.Model
{
    /// <summary>
    /// a base class other classes may derive from
    /// </summary>
    public class Brine : BaseCompositionProperties
    {
        /// <summary>
        /// a MetaInfo for the MyBaseData
        /// </summary>
        public MetaInfo? MetaInfo { get; set; }

        /// <summary>
        /// name of the data
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// a description of the data
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// the date when the data was created
        /// </summary>
        public DateTimeOffset? CreationDate { get; set; }

        /// <summary>
        /// the date when the data was last modified
        /// </summary>
        public DateTimeOffset? LastModificationDate { get; set; }  
        /// <summary>
        /// PVT Parameters for single-salt brine 
        /// </summary>
        public BrinePVTParameters? PVTParameters { get; set; }
        /// <summary>
        /// Brine salt content 
        /// </summary>
        public List<SaltContent>? SaltContent { get; set; }
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public Brine() : base()
        {
        }
    }
}
