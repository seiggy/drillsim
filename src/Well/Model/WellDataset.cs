using System;
using System.Collections.Generic;

namespace OSDC.Drilling.Well.Model;

public enum WellDataClassification
{
    Observed,
    Derived,
    ModelEstimated,
    Synthetic
}

public class WellDataset
{
    public WellDatasetProvenance? Provenance { get; set; }
    public List<WellExternalIdentifier>? ExternalIdentifiers { get; set; }
    public WellBitemporalTimestamps? Temporal { get; set; }
    public List<WellLifecycleEvent>? LifecycleEvents { get; set; }
    public List<WellMonthlyProduction>? MonthlyProduction { get; set; }
}

public class WellDatasetProvenance
{
    public string? DatasetName { get; set; }
    public string? DatasetVersion { get; set; }
    public WellDataClassification? Classification { get; set; }
    public List<WellSourceArtifact>? SourceArtifacts { get; set; }
}

public class WellSourceArtifact
{
    public Guid ID { get; set; }
    public string? Url { get; set; }
    public string? License { get; set; }
    public string? Attribution { get; set; }
    public string? SHA256 { get; set; }
}

public class WellExternalIdentifier
{
    public string? Namespace { get; set; }
    public string? Value { get; set; }
}

public class WellBitemporalTimestamps
{
    public DateTimeOffset? ValidTimeStart { get; set; }
    public DateTimeOffset? ValidTimeEnd { get; set; }
    public DateTimeOffset? TransactionTimeStart { get; set; }
    public DateTimeOffset? TransactionTimeEnd { get; set; }
}

public class WellLifecycleEvent
{
    public string? EventType { get; set; }
    public DateTimeOffset? EffectiveAt { get; set; }
    public DateTimeOffset? FirstSeenAt { get; set; }
    public Guid? SourceArtifactID { get; set; }
}

public class WellProductionQuantity
{
    public double? Value { get; set; }
    public string? Unit { get; set; }
}

public class WellMonthlyProduction
{
    public int Year { get; set; }
    public int Month { get; set; }
    public WellProductionQuantity? Oil { get; set; }
    public WellProductionQuantity? Gas { get; set; }
    public WellProductionQuantity? Water { get; set; }
    public double? DaysOnProduction { get; set; }
    public bool IsAllocated { get; set; }
    public Guid? SourceArtifactID { get; set; }
    public WellDataClassification? Classification { get; set; }
}
