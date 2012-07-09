using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Tile
    {
        public ushort X;
        public ushort Y;
        public TerrainType BaseTerrainType;

        public Tile(ushort x, ushort y, TerrainType terrainType)
        {
            X = x;
            Y = y;
            BaseTerrainType = terrainType;
        }

        public IEnumerable<Tile> AdjacentTiles
        {
            get
            {
                if (_adjacentTiles == null)
                {
                    throw new Exception();
                }
                return _adjacentTiles;
            }
            set { _adjacentTiles = value; }
        }
        private IEnumerable<Tile> _adjacentTiles;

        public bool IsCoastal
        {
            get
            {
                if (_isCoastalDiscovered)
                    return _isCoastal;

                _isCoastalDiscovered = true;

                return _isCoastal = BaseTerrainType.HasFlag(Terrain.All_Land) && AdjacentTiles.Any(x => x.BaseTerrainType.HasFlag(Terrain.All_Water));
            }
        }
        bool _isCoastal;
        bool _isCoastalDiscovered;

        //public int StackLimit
        //{
        //    get
        //    {
        //        if (Structure != null)
        //            return stackLimit + 1;
        //        return stackLimit;
        //    }
        //}

        //private int stackLimit
        //{
        //    get { return TerrainType.StackLimit; }
        //}


        
    }
}
