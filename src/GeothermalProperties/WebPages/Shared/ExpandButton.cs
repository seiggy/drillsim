using MudBlazor;

namespace NORCE.Drilling.GeothermalProperties.WebPages.Shared;

public class ExpandButton
{
    public readonly string PanelOpenText = "Hide";
    public readonly string PanelClosedText = "Show";
    public readonly string PanelOpenIcon = Icons.Material.Filled.ExpandLess;
    public readonly string PanelClosedIcon = Icons.Material.Filled.ExpandMore;
    public bool isExpanded = true;
}
