using OSDC.DotnetLibraries.Drilling.DrillingProperties;
using OSDC.DotnetLibraries.General.DataManagement;
using OSDC.DotnetLibraries.General.Statistics;
using NORCE.Drilling.GeothermalProperties.Model;

namespace NORCE.Drilling.GeothermalProperties.ModelTest
{
    public class Tests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
        }

        [Test]
        public void Test_Calculus()
        {
            Guid guid = Guid.NewGuid();
            MetaInfo metaInfo = new() { ID = guid };
            DateTimeOffset creationDate = DateTimeOffset.UtcNow;

            Guid guid2 = Guid.NewGuid();
            MetaInfo metaInfo2 = new() { ID = guid2 };
            DateTimeOffset creationDate2 = DateTimeOffset.UtcNow;
            ScalarDrillingProperty derivedData1Param = new() { DiracDistributionValue = new DiracDistribution() { Value = 2.0 } };
            Model.GeothermalProperties geothermalProperties = new()
            {
                MetaInfo = metaInfo2,
                Name = "My test GeothermalProperties name",
                Description = "My test GeothermalProperties for POST",
                CreationDate = creationDate,
                LastModificationDate = creationDate2,
                TableType = (TableType)0,
                GeothermalDataList = new List<GeothermalData>
					{
						new GeothermalData
						{
							RegionType = GeothermalPropertiesType.Air,
							Temperature = 293.15,
							TemperatureGradient = -3,
							VerticalDepth = 0,
						},
						new GeothermalData
						{
							RegionType = GeothermalPropertiesType.RockFormation,
							Temperature = 303.15,
							TemperatureGradient = 4,
							VerticalDepth = 20,
						}
					}		
            };
            Model.GeothermalPropertiesCompletionOrder geothermalPropertiesCompletionOrder = new()
            {
                MetaInfo = metaInfo,
                Name = "My test GeothermalPropertiesCompletionOrder",
                Description = "My test GeothermalPropertiesCompletionOrder",
                CreationDate = creationDate,
                LastModificationDate = creationDate,
                InterpolationStep = 10,
                ReferenceGeothermalProperties = geothermalProperties,
            };

  
            bool success = geothermalPropertiesCompletionOrder.Calculate();
            Assert.That(success);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }
    }
}