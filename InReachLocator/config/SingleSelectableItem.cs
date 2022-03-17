using System;

namespace J4JSoftware.InReach;

public record SingleSelectableItem
{
    public SingleSelectableItem(
        Type? itemType,
        string value,
        string label = ""
    )
    {
        Item = itemType;
        Value = value;
        Label = string.IsNullOrEmpty(label) ? value : label;
    }

    public Type? Item { get; init; } 
    public string Value { get; init; }
    public string Label { get; init; }
}
