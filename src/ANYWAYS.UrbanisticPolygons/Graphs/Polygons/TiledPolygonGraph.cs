using System;
using System.Collections.Generic;
using System.Linq;
using ANYWAYS.UrbanisticPolygons.Landuse;
using OsmSharp.Tags;

namespace ANYWAYS.UrbanisticPolygons.Graphs.Polygons
{
    internal class TiledPolygonGraph
    {
        private readonly Graph<(int x, int y, uint tileId), PolygonGraphEdge, LanduseAttributes> _graph = new Graph<(int x, int y, uint tileId), PolygonGraphEdge, LanduseAttributes>();
        private readonly Dictionary<Guid, int> _vertexGuids = new Dictionary<Guid, int>();
        private readonly Dictionary<Guid, int> _edgeGuids = new Dictionary<Guid, int>();
        private readonly Dictionary<Guid, int> _faceGuids = new Dictionary<Guid, int>();
        private readonly HashSet<uint> _tiles = new HashSet<uint>();

        public TiledPolygonGraph(int zoom = 14, int resolution = 16384)
        {
            Zoom = zoom;
            Resolution = resolution;
        }

        public int Zoom { get; }

        public int Resolution { get; }

        public int VertexCount => _graph.VertexCount;

        public int FaceCount => _graph.FaceCount;

        public bool TryGetVertex(Guid vertexGuid, out int vertex)
        {
            return _vertexGuids.TryGetValue(vertexGuid, out vertex);
        }

        public void SetTileLoaded(uint tile)
        {
            _tiles.Add(tile);
        }

        public IEnumerable<uint> LoadedTiles()
        {
            return _tiles;
        }

        public bool HasTile(uint tile)
        {
            return _tiles.Contains(tile);
        }

        public int AddVertex((int x, int y, uint tileId) tiledLocation, Guid vertexGuid)
        {
            var vertex = _graph.AddVertex(tiledLocation);
            _vertexGuids[vertexGuid] = vertex;
            return vertex;
        }

        public (int x, int y, uint tileId) GetVertex(int vertex)
        {
            return _graph.GetVertex(vertex);
        }

        public bool TryGetEdge(Guid edgeGuid, out int edge)
        {
            return _edgeGuids.TryGetValue(edgeGuid, out edge);
        }

        public int AddEdge(int vertex1, int vertex2, Guid edgeGuid, IEnumerable<(int x, int y, uint tileId)>? shape = null,
            TagsCollectionBase tags = null)
        {
            shape ??= Enumerable.Empty<(int x, int y, uint tileId)>();
            tags ??= new TagsCollection();
            
            var id = _graph.AddEdge(vertex1, vertex2, new PolygonGraphEdge()
            {
                Shape = shape.ToArray(),
                Tags = tags
            });
            _edgeGuids.Add(edgeGuid, id);
            return id;
        }

        public void DeleteEdge(int edge)
        {
            _graph.DeleteEdge(edge);
        }

        public void ResetFaces()
        {
            _graph.ResetFaces();
        }
        
        public int AddFace(Guid? guid = null)
        {
            var id = _graph.AddFace(default);
            if (guid != null) _faceGuids[guid.Value] = id;
            return id;
        }

        public void SetFaceData(int face, LanduseAttributes data)
        {
            _graph.SetFaceData(face, data); 
        }

        public LanduseAttributes GetFaceData(int face)
        {
            return _graph.GetFaceData(face);
        }

        public bool TryGetFace(Guid guid, out int face)
        {
            return _faceGuids.TryGetValue(guid, out face);
        }

        public void SetFace(int edge, bool left, int face)
        {
            _graph.SetFace(edge, left, face);
        }
        
        public GraphEnumerator GetEnumerator()
        {
            return new GraphEnumerator(this);
        }
        
        public GraphFaceEnumerator GetFaceEnumerator()
        {
            return new GraphFaceEnumerator(this);
        }

        private struct PolygonGraphEdge
        {
            public (int x, int y, uint tileId)[] Shape { get; set; }
        
            public TagsCollectionBase Tags { get; set; }
        }

        public class GraphEnumerator
        {
            private readonly Graph<(int x, int y, uint tileId), PolygonGraphEdge, LanduseAttributes>.Enumerator _enumerator;

            public GraphEnumerator(TiledPolygonGraph graph)
            {
                Graph = graph;
                
                _enumerator = graph._graph.GetEdgeEnumerator();
            }

            public bool MoveTo(int vertex)
            {
                return _enumerator.MoveTo(vertex);
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public TiledPolygonGraph Graph { get; }

            public int Edge => _enumerator.Edge;

            public int Vertex1 => _enumerator.Vertex1;

            public int Vertex2 => _enumerator.Vertex2;

            public int FaceLeft => _enumerator.FaceLeft;

            public int FaceRight => _enumerator.FaceRight;

            public bool Forward => _enumerator.Forward;

            public (int x, int y, uint tileId)[] Shape => _enumerator.Data.Shape;

            public TagsCollectionBase Tags => _enumerator.Data.Tags;
        }
        
        public class GraphFaceEnumerator
        {
            private readonly Graph<(int x, int y, uint tileId), PolygonGraphEdge, LanduseAttributes>.FaceEnumerator _enumerator;

            public GraphFaceEnumerator(TiledPolygonGraph graph)
            {
                Graph = graph;
                
                _enumerator = graph._graph.GetFaceEnumerator();
            }

            public bool MoveTo(int face)
            {
                return _enumerator.MoveTo(face);
            }

            public bool MoveNext()
            {
                return _enumerator.MoveNext();
            }

            public TiledPolygonGraph Graph { get; }

            public int Edge => _enumerator.Edge;

            public int Vertex1 => _enumerator.Vertex1;

            public int Vertex2 => _enumerator.Vertex2;

            public bool IsLeft => _enumerator.IsLeft;

            public (int x, int y, uint tileId)[] Shape => _enumerator.Data.Shape;

            public TagsCollectionBase Tags => _enumerator.Data.Tags;
        }
    }
}