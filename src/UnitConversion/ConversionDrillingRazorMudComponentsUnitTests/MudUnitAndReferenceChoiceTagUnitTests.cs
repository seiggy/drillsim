using Bunit;
using Xunit;
using OSDC.UnitConversion.DrillingRazorMudComponents;
using MudBlazor;
using MudBlazor.Services;

namespace ConversionDrillingRazorMudComponentsUnitTests
{
    public class MudUnitAndReferenceChoiceTagUnitTests : TestContext
    {
        public MudUnitAndReferenceChoiceTagUnitTests() : base()
        {
            Services.AddMudServices();
        }

        [Fact]
        public void Test1()
        {
            var obj = RenderComponent<MudUnitAndReferenceChoiceTag>(parameters => parameters
            .Add(p => p.HttpHost, "https://dev.digiwells.no/")
            .Add(p => p.HttpBasePath, "UnitConversion/api/")
            .Add(p => p.HttpController, "UnitSystem/")
            .Add(p => p.UnitSystemName, "SI"));

            obj.WaitForState(() => obj.Instance.InitializedOnce, timeout: TimeSpan.FromSeconds(5));
            Assert.NotNull(obj.Instance);
            double val = obj.Instance.FromSI(2.0*Math.PI, OSDC.UnitConversion.Conversion.DrillingEngineering.DrillingPhysicalQuantity.QuantityEnum.AngularVelocityDrilling);
            //Assert.Equal(2.0*Math.PI, val);
            obj.Instance.UnitSystemName = "Metric";
            var label = obj.Instance.GetUnitLabel(OSDC.UnitConversion.Conversion.DrillingEngineering.DrillingPhysicalQuantity.QuantityEnum.AngularVelocityDrilling);
            val = obj.Instance.FromSI(2.0 * Math.PI, OSDC.UnitConversion.Conversion.DrillingEngineering.DrillingPhysicalQuantity.QuantityEnum.AngularVelocityDrilling);
            //Assert.Equal(60.0, val);
        }
    }
}