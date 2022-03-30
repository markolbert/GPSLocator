using System;

namespace J4JSoftware.GPSLocator;

public record SelectablePage
{
    public SelectablePage(
        Type? pageType,
        string pageTag,
        string label = ""
    )
    {
        PageType = pageType;
        PageTag = pageTag;
        Label = string.IsNullOrEmpty(label) ? pageTag : label;
    }

    public Type? PageType { get; init; } 
    public string PageTag { get; init; }
    public string Label { get; init; }
}
