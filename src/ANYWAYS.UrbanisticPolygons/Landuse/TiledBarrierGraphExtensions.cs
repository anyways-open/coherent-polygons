using System;
using System.Collections.Generic;
using System.Linq;
using ANYWAYS.UrbanisticPolygons.Graphs.Barrier;
using ANYWAYS.UrbanisticPolygons.Graphs.Barrier.Faces;
using ANYWAYS.UrbanisticPolygons.Guids;
using ANYWAYS.UrbanisticPolygons.Tiles;
using NetTopologySuite.Geometries;

namespace ANYWAYS.UrbanisticPolygons.Landuse
{
    internal static class TiledBarrierGraphExtensions
    {
        public static void AssignLanduse(
            this TiledBarrierGraph tiledBarrierGraph, uint tile,
            Func<((double longitude, double latitude) topLeft, (double longitude, double latitude) bottomRight), IEnumerable<(Polygon polygon, string type)>> getLanduse)
        {
            var tileBox = TileStatic.Box(tiledBarrierGraph.Zoom, tile);
            for (var f = 1; f < tiledBarrierGraph.FaceCount; f++)
            {
                // determine if face overlaps with tile.
                var box = tiledBarrierGraph.FaceToClockwiseCoordinates(f).ToBox();
                if (box == null) continue;
                if (!box.Value.Overlaps(tileBox)) continue; // face is not in tile.
                
                // build face polygon.
                var facePolygon = tiledBarrierGraph.ToPolygon(f);
                if (facePolygon == null) continue; // face is not a polygon.
                
                // build the attributes.
                var attributes = new LanduseAttributes();
                
                // get all the polygons for all tiles for the current face.
                var landuse = getLanduse(box.Value);
                foreach (var (polygon, type) in landuse)
                {
                    var percentage = 0.0;
                    if (polygon.Overlaps(facePolygon))
                    {
                        // landuse completely overlaps the polygon, add it as 100%
                        percentage = 1;
                    }
                    else
                    {
                        try
                        {
                            var intersection = facePolygon.Intersection(polygon);
                            if (intersection == null || intersection.IsEmpty) continue;

                            if (intersection is Polygon intersectionPolygon)
                            {
                                percentage = intersectionPolygon.Area / facePolygon.Area;
                            }
                        }
                        catch (Exception e)
                        {
                            // TODO: non-noded intersections, due to invalid polygons?
                        }
                    }
                    
                    // update attributes.
                    attributes = attributes.Set(type, percentage);
                }
                
                // set face data.
                tiledBarrierGraph.SetFaceData(f, attributes);
            }
        }
    }
}