using System.Collections.Generic;
using System.Linq;

namespace ANYWAYS.UrbanisticPolygons.Graphs.Barrier.Faces
{
    internal static class TiledBarrierGraphExtensions
    {
        internal static IEnumerable<TiledBarrierGraph.BarrierGraphEnumerator> NextClockwise(this TiledBarrierGraph.BarrierGraphEnumerator enumerator)
        {
            var nextEnumerator = enumerator.Graph.GetEnumerator();
            nextEnumerator.MoveTo(enumerator.Vertex2);
            var graph = enumerator.Graph;

            // get a sorted list by angle clockwise relative to the selected edge.
            var v2NonLocation = enumerator.FirstNonVertex2();
            var v2Location = graph.GetVertex(enumerator.Vertex2);
            var sortedByAngle = new SortedDictionary<double, int>();
            while (nextEnumerator.MoveNext())
            {
                if (nextEnumerator.Edge == enumerator.Edge) continue;

                var nextNonLocation = nextEnumerator.FirstNonVertex1();
                var angle = GeoExtensions.Angle(v2NonLocation, v2Location, nextNonLocation);
                sortedByAngle[angle] = nextEnumerator.Vertex2;
            }
            
            // enumerate edges by the order determined above.
            foreach (var p in sortedByAngle)
            {
                nextEnumerator.MoveTo(enumerator.Vertex2);

                while (nextEnumerator.MoveNext())
                {
                    if (nextEnumerator.Vertex2 == p.Value) break;
                }

                yield return nextEnumerator;
            }
        }

        internal static (double longitude, double latitude) FirstNonVertex2(
            this TiledBarrierGraph.BarrierGraphEnumerator enumerator)
        {
            if (enumerator.Shape.Length == 0)
            {
                return enumerator.Graph.GetVertex(enumerator.Vertex1);
            }
            if (enumerator.Forward)
            {
                return enumerator.Shape[^1];
            }
            else
            {
                return enumerator.Shape[0];
            }
        }
        
        internal static (double longitude, double latitude) FirstNonVertex1(
            this TiledBarrierGraph.BarrierGraphEnumerator enumerator)
        {
            if (enumerator.Shape.Length == 0)
            {
                return enumerator.Graph.GetVertex(enumerator.Vertex2);
            }
            if (enumerator.Forward)
            {
                return enumerator.Shape[0];
            }
            else
            {
                return enumerator.Shape[^1];
            }
        }

        internal static TiledBarrierGraph.BarrierGraphEnumerator? NextRight(
            this TiledBarrierGraph.BarrierGraphEnumerator enumerator)
        {
            return enumerator.NextClockwise().FirstOrDefault();
        }

        internal static TiledBarrierGraph.BarrierGraphEnumerator? NextLeft(
            this TiledBarrierGraph.BarrierGraphEnumerator enumerator)
        {
            return enumerator.NextClockwise().LastOrDefault();
        }
    }
}