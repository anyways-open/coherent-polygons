using System;
using System.Collections.Generic;
using Reminiscence;
using Reminiscence.Arrays;

namespace ANYWAYS.UrbanisticPolygons.Tools.OSMCacheBuilder
{

    internal class TilesMap
    {
        private readonly TileMap _wayToFirstTile = new TileMap();
        private readonly MemoryArray<uint> _linkedTileList = new MemoryArray<uint>(0);
        private const uint TileMask = (uint) ((long)1 << 31);

        private uint _nextPointer = 0;

        public TilesMap()
        {
            
        }

        private TilesMap(TileMap wayToFirstTile, MemoryArray<uint> linkedTileList,
            uint nextPointer)
        {
            _wayToFirstTile = wayToFirstTile;
            _linkedTileList = linkedTileList;
            _nextPointer = nextPointer;
        }

        public void Add(long id, IEnumerable<uint> tiles)
        {
            using var enumerator = tiles.GetEnumerator();
            if (!enumerator.MoveNext()) return;

            _wayToFirstTile.EnsureMinimumSize(id);
            _wayToFirstTile[id] = enumerator.Current + TileMask;

            if (!enumerator.MoveNext()) return;
            
            // there is a second entry, add to linked list.
            var pointer = _nextPointer;
            _linkedTileList.EnsureMinimumSize(pointer * 2 + 2);
            _nextPointer++;
                
            // add previous data first.
            var previous = _wayToFirstTile[id] - TileMask;
            _linkedTileList[pointer * 2 + 0] = previous;
            _linkedTileList[pointer * 2 + 1] = uint.MaxValue; // no previous!
                
                
            // add current.
            var next = _nextPointer;
            _nextPointer++;
            _linkedTileList.EnsureMinimumSize(next * 2 + 2);
            _linkedTileList[next * 2 + 0] = enumerator.Current;
            _linkedTileList[next * 2 + 1] = pointer;

            // add tile 3 and so on.
            while (enumerator.MoveNext())
            {
                pointer = next;
                next = _nextPointer;
                _nextPointer++;
                _linkedTileList.EnsureMinimumSize(next * 2 + 2);
                _linkedTileList[next * 2 + 0] = enumerator.Current;
                _linkedTileList[next * 2 + 1] = pointer;
            }
                
            // update the first tile array to indicate data in the linked list.
            if (next >= TileMask) throw new Exception("This index cannot handle this much data.");
            _wayToFirstTile[id] = next;
        }

        public bool Has(long id)
        {
            if (_wayToFirstTile.Length <= id) return false;
            
            return _wayToFirstTile[id] != 0;
        }
        
        public IEnumerable<uint> Get(long id)
        {
            var idOrPointer = _wayToFirstTile[id];
            if (idOrPointer == 0) yield break;
            if (idOrPointer > TileMask)
            {
                yield return (idOrPointer - TileMask);
                yield break;
            }

            while (idOrPointer < uint.MaxValue)
            {
                var tile = _linkedTileList[idOrPointer * 2 + 0];
                yield return (tile);
                idOrPointer = _linkedTileList[idOrPointer * 2 + 1];
            }
        }
    }
}