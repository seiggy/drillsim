using OSDC.DotnetLibraries.General.DataManagement;
using System;
using System.Collections.Generic;

namespace NORCE.Drilling.DrillingFluid.Model
{
    public class DrillingFluid
    {
        /// <summary>
        /// a MetaInfo for the DrillingFluid
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
        /// the ID of the fluid description used
        /// </summary>
        public DrillingFluidDescription DrillingFluidDescription { get; set; }
        /// <summary>
        /// the ID of the drillstring used for this section
        /// </summary>
        public Guid DrillStringID { get; set; }
        
        /// <summary>
        /// an output parameter, result of the Calculate() method
        /// </summary>
        public double? OutputParam { get; set; }

        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public DrillingFluid() : base()
        {
        }

        /// <summary>
        /// main calculation method of the DrillingFluid
        /// </summary>
        /// <returns></returns>
        public bool Calculate()
        {
            bool success = false;
            OutputParam = null;
            if (DrillStringID != null && DrillingFluidDescription != null)
            {
                success = true;
            } 
            return success;
        }
    }
}
