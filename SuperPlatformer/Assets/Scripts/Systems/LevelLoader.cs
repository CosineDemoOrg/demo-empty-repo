using System.IO;
using UnityEngine;
using SuperPlatformer.Core.Physics;

namespace SuperPlatformer.Systems
{
    /// &lt;summary&gt;
    /// Loads a CSV level, populates TilemapWorld, spawns placeholder tile sprites.
    /// &lt;/summary&gt;
    public static class LevelLoader
    {
        private static Transform _tileParent;

        public static void Load(string levelName)
        {
            if (_tileParent != null)
                Object.DestroyImmediate(_tileParent.gameObject);

            // Load CSV from Resources/Levels/
            TextAsset csv = Resources.Load&lt;TextAsset&gt;($"Levels/{levelName}");
            if (csv == null)
            {
                Debug.LogError($"LevelLoader: missing level '{levelName}'");
                return;
            }

            // Parse CSV
            string[] lines = csv.text.Replace("\r\n", "\n").Split('\n');
            int height = lines.Length;
            int width = 0;
            for (int i = 0; i &lt; height; ++i)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) { height = i; break; }
                if (width == 0) width = lines[i].Split(',').Length;
            }
            int[,] tiles = new int[width, height];
            for (int y = 0; y &lt; height; ++y)
            {
                var cells = lines[y].Split(',');
                for (int x = 0; x &lt; width; ++x)
                {
                    int v; int.TryParse(cells[x], out v);
                    tiles[x, height - y - 1] = v; // y=0 = bottom row
                }
            }
            float tileSize = 1f;
            TilemapWorld.Active = new TilemapWorld(tiles, tileSize);

            // Visual: instantiate placeholder sprites for each solid tile
            _tileParent = new GameObject("TilemapVisual").transform;
            _tileParent.position = Vector3.zero;

            Sprite tileSprite = GenerateWhiteSquareSprite();
            for (int x = 0; x &lt; width; ++x)
                for (int y = 0; y &lt; height; ++y)
                    if (tiles[x, y] == 1)
                    {
                        var go = new GameObject($"Tile_{x}_{y}");
                        go.transform.parent = _tileParent;
                        go.transform.position = new Vector3(x * tileSize + tileSize / 2, y * tileSize + tileSize / 2, 0);
                        var sr = go.AddComponent&lt;SpriteRenderer&gt;();
                        sr.sprite = tileSprite;
                        sr.color = Color.white;
                        sr.sortingOrder = 0;
                        go.transform.localScale = Vector3.one * tileSize;
                    }
        }

        private static Sprite GenerateWhiteSquareSprite()
        {
            // 16x16 white texture, reused
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[16 * 16];
            for (int i = 0; i &lt; pixels.Length; ++i) pixels[i] = new Color32(255, 255, 255, 255);
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}