using OSDC.UnitConversion.Conversion;
using OSDC.UnitConversion.Conversion.DrillingEngineering;

namespace ConversionUnitSystemDrillingEngineeringTest
{
    public class PhysicalQuantityDescriptionUnitTest
    {
        [Test]
        public void ExpandedCatalogDescriptionsContainDefinitionDimensionAndSiUnit()
        {
            Assert.That(AbsorbedDoseQuantity.Instance.DescriptionMD,
                Does.Contain("energy imparted by ionising radiation")
                    .And.Contain("physical dimension")
                    .And.Contain("coherent SI unit"));

            Assert.That(VolumePerAngleQuantity.Instance.DescriptionMD,
                Does.Contain("plane-angle variation")
                    .And.Contain("rotational geometry"));

            Assert.That(AbsorbedDoseDrillingQuantity.Instance.DescriptionMD,
                Does.Contain("energy imparted by ionising radiation")
                    .And.Contain("drilling specialization")
                    .And.Contain("meaningful precision"));
        }
    }
}
