namespace J4JSoftware.GPSCommon;

public record OpenTopoMapDisplayLayer() : KeylessMapDisplayLayerBase( "https://tile.opentopomap.org/{z}/{x}/{y}.png",
                                                                      "OpenTopoMap",
                                                                      "© [OpenTopoMap](https://opentopomap.org/) © [OpenStreetMap contributors](http://www.openstreetmap.org/copyright)" );
