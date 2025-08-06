using System;
using UnityEngine;

namespace SuperPlatformer.Core.Physics
{
    /// &lt;summary&gt;
    /// Holds a 2D tile grid; 0 = empty, 1 = solid. Provides collision helpers.
    /// &lt;/summary&gt;
    public class TilemapWorld
    {
        public readonly int[,] tiles;
        public readonly int width, height;
        public readonly float tileSize;

        public static TilemapWorld Active;

        public TilemapWorld(int[,] tiles, float tileSize = 1f)
        {
            this.tiles = tiles;
            width = tiles.GetLength(0);
            height = tiles.GetLength(1);
            this.tileSize = tileSize;
        }

        /// &lt;summary&gt; Returns true if tile at (tx,ty) is solid. Out-of-bounds is treated as solid. &lt;/summary&gt;
        public bool IsSolidAt(int tx, int ty)
        {
            if (tx &lt; 0 || ty &lt; 0 || tx &gt;= width || ty &gt;= height)
                return true;
            return tiles[tx, ty] == 1;
        }

        /// &lt;summary&gt; Returns world-space bounds of a tile at (tx,ty). &lt;/summary&gt;
        public Bounds TileBounds(int tx, int ty)
        {
            Vector3 min = new Vector3(tx * tileSize, ty * tileSize, 0f);
            Vector3 size = new Vector3(tileSize, tileSize, 0f);
            return new Bounds(min + size * 0.5f, size);
        }
    }
}