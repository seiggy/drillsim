using OSDC.DotnetLibraries.General.DataManagement;
using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using NORCE.Drilling.GeothermalProperties.Interpolation;
namespace NORCE.Drilling.GeothermalProperties.Model
{   


    public class GeothermalPropertiesCompletionOrder : GeothermalPropertiesCompletionOrderLight 
    {
        /// <summary>
        /// a MetaInfo for the GeothermalPropertiesCompletionOrder
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
        /// an input list of GeothermalProperties
        /// </summary>
        public GeothermalProperties? ReferenceGeothermalProperties { get; set; }
        /// <summary>
        /// an output list of GeothermalProperties
        /// </summary>
        public GeothermalProperties? CompletedGeothermalProperties { get; set; }
        /// <summary>
        /// The vertical depth step for interpolation
        /// </summary>
        public double InterpolationStep { get; set; }

        /// <summary>
        /// an output parameter, result of the Calculate() method
        /// </summary>
        public CompletionMethod CompletionMethod {get; set;} = CompletionMethod.LinearInterpolation;
        /// <summary>
        /// default constructor required for JSON serialization
        /// </summary>
        public GeothermalPropertiesCompletionOrder() : base()
        {
        }
        /// <summary>
        /// main calculation method of the GeothermalPropertiesCompletionOrder
        /// </summary>
        /// <returns></returns>
        private List<double> interpolatedTemperatures;
        private List<double> interpolatedDepths;
        private List<double> interpolatedGradients;
        
        public bool Calculate()
        {
            bool success = false;
            if (InterpolationStep <= 0)
            {
                return success;
            }
            if (ReferenceGeothermalProperties != null)
            {   
                //Create a base instance for the geothermal data
                CompletedGeothermalProperties = new GeothermalProperties
                {
                    MetaInfo = new MetaInfo 
                        {
                            ID = Guid.NewGuid(),
                            HttpHostName = "https://dev.digiwells.no/",
                            HttpHostBasePath = "GeothermalProperties/api/",
					        HttpEndPoint = "GeothermalPropertiesInterpolated/",
                        },
                    Name = ReferenceGeothermalProperties.Name + " (Interpolated)",
                    Description = ReferenceGeothermalProperties.Name + ": Interpolated with step :" + InterpolationStep.ToString(),                    
                    TableType = TableType.Interpolated,                    
                    GeothermalDataList = new List<GeothermalData>()
                };
                //If both initial information are null, then it fails
                if (ReferenceGeothermalProperties.GeothermalDataList[0].Temperature == null ||  
                    ReferenceGeothermalProperties.GeothermalDataList[0].VerticalDepth == null)
                {
                    return success;
                }
                List<LinearCurve> lines = new List<LinearCurve>
                    {
                        new LinearCurve
                        (
                            (double) ReferenceGeothermalProperties.GeothermalDataList[0].VerticalDepth!, 
                            (double) ReferenceGeothermalProperties.GeothermalDataList[0].Temperature!, 
                            ReferenceGeothermalProperties.GeothermalDataList[1].VerticalDepth, 
                            ReferenceGeothermalProperties.GeothermalDataList[1].Temperature,               
                            ReferenceGeothermalProperties.GeothermalDataList[1].TemperatureGradient
                        )
                    }; 
                List<GeothermalPropertiesType> regionTypes = new List<GeothermalPropertiesType>{GeothermalPropertiesType.Air};               
                for (int i = 1; i < ReferenceGeothermalProperties.GeothermalDataList.Count - 1; i++)
                {   
                    regionTypes.Add(
                        ReferenceGeothermalProperties.GeothermalDataList[i].RegionType
                    );
                    lines.Add(
                        new LinearCurve
                        (
                            lines[i-1],
                            ReferenceGeothermalProperties.GeothermalDataList[i+1].VerticalDepth, 
                            ReferenceGeothermalProperties.GeothermalDataList[i+1].Temperature,               
                            ReferenceGeothermalProperties.GeothermalDataList[i+1].TemperatureGradient
                        )
                    );                
                }
                double initialDepth = lines[0].pt0[0];
                double finalDepth = lines[lines.Count-1].pt1[0];
                int lineIndex = 0;
                int maxLine = lines.Count-1;
                for (double depth = initialDepth; depth < finalDepth + InterpolationStep; depth += InterpolationStep)
                {
                    // Check which line the first point belongs to 
                    lineIndex += (depth > lines[lineIndex].pt1[0]) ? 1:0;
                    lineIndex = Math.Min(lineIndex, maxLine);
                    double localTemperature = lines[lineIndex].f(depth);
                    double localGradient = lines[lineIndex].dYdX;
                    CompletedGeothermalProperties.GeothermalDataList.Add
                    (
                        new GeothermalData
                        {
                            RegionType = regionTypes[lineIndex],
                            VerticalDepth = depth,
                            Temperature = localTemperature,
                            TemperatureGradient = localGradient
                        }
                    );
                }
                success = true;
            }
            return success;
        }
        private int ReturnInstancesWithWaterRegion()
        {
            if (ReferenceGeothermalProperties == null)
                return 0;
            else if (ReferenceGeothermalProperties.GeothermalDataList != null)
                return ReferenceGeothermalProperties.GeothermalDataList.Where(t => t.RegionType == GeothermalPropertiesType.Water).ToList().Count;
            else 
                return 0;
        }
    }
}
