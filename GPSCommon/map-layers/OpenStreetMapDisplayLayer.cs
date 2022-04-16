namespace J4JSoftware.GPSCommon;

public record OpenStreetMapDisplayLayer() : KeylessMapDisplayLayerBase( "https://tile.openstreetmap.org/{z}/{x}/{y}.png",
                                                                        "OpenStreetMap",
                                                                        "© [OpenStreetMap Contributors](http://www.openstreetmap.org/copyright)" );
