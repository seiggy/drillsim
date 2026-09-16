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
            JSInterop.Mode = JSRuntimeMode.Loose;
            RenderComponent<MudPopoverProvider>();
        }

        [Fact]
        public void DefaultUnitSystemsConvertAngularVelocityWithoutAnExternalService()
        {
            var obj = RenderComponent<MudUnitAndReferenceChoiceTag>(parameters => parameters
            .Add(p => p.UnitSystemName, "SI"));

            obj.WaitForState(() => obj.Instance.InitializedOnce, timeout: TimeSpan.FromSeconds(5));
            const OSDC.UnitConversion.Conversion.DrillingEngineering.DrillingPhysicalQuantity.QuantityEnum quantity =
                OSDC.UnitConversion.Conversion.DrillingEngineering.DrillingPhysicalQuantity.QuantityEnum.AngularVelocityDrilling;
            Assert.Equal(2.0 * Math.PI, obj.Instance.FromSI(2.0 * Math.PI, quantity), precision: 8);

            obj.SetParametersAndRender(parameters => parameters.Add(p => p.UnitSystemName, "Metric"));
            Assert.False(string.IsNullOrWhiteSpace(obj.Instance.GetUnitLabel(quantity)));
            Assert.Equal(60.0, obj.Instance.FromSI(2.0 * Math.PI, quantity), precision: 8);
        }
    }
}