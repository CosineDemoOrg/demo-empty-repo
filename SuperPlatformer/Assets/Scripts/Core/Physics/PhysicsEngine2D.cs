using UnityEngine;

namespace SuperPlatformer.Core.Physics
{
    /// &lt;summary&gt;
    /// Deterministic tile-based physics and collision.
    /// &lt;/summary&gt;
    public static class PhysicsEngine2D
    {
        private const float EPSILON = 0.0001f;
        private static readonly Vector2[] sweepDirs = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

        public struct MoveResult
        {
            public Vector2 allowedDelta;
            public bool hitLeft, hitRight, hitCeiling, hitGround;
        }

        /// &lt;summary&gt;
        /// True if any tile directly below playerBounds is solid.
        /// &lt;/summary&gt;
        public static bool CheckGrounded(Bounds playerBounds)
        {
            var world = TilemapWorld.Active;
            if (world == null) return false;

            float minX = playerBounds.min.x + EPSILON;
            float maxX = playerBounds.max.x - EPSILON;
            float y = playerBounds.min.y - EPSILON;
            int ty = Mathf.FloorToInt(y / world.tileSize);

            int tx0 = Mathf.FloorToInt(minX / world.tileSize);
            int tx1 = Mathf.FloorToInt(maxX / world.tileSize);

            for (int tx = tx0; tx &lt;= tx1; ++tx)
            {
                if (world.IsSolidAt(tx, ty))
                    return true;
            }
            return false;
        }

        /// &lt;summary&gt;
        /// Swept AABB tile collision. Returns allowed move and collision flags.
        /// &lt;/summary&gt;
        public static MoveResult Move(Bounds playerBounds, Vector2 desiredMove)
        {
            var world = TilemapWorld.Active;
            MoveResult result = new MoveResult();
            if (world == null)
            {
                result.allowedDelta = desiredMove;
                return result;
            }
            Vector2 pos = playerBounds.min;
            Vector2 size = new Vector2(playerBounds.size.x, playerBounds.size.y);

            Vector2 remaining = desiredMove;
            Vector2 allowed = Vector2.zero;
            int maxSteps = 4; // X, Y; repeat if cornered
            for (int i = 0; i &lt; maxSteps; ++i)
            {
                if (remaining.sqrMagnitude &lt; EPSILON * EPSILON)
                    break;

                Vector2 step = (i % 2 == 0) ? new Vector2(remaining.x, 0) : new Vector2(0, remaining.y);
                if (step == Vector2.zero)
                    continue;

                // Expand AABB in step direction
                Bounds bbox = new Bounds();
                bbox.SetMinMax(pos, pos + size);
                bbox.center += (Vector3)(step * 0.5f);
                bbox.size += new Vector3(Mathf.Abs(step.x), Mathf.Abs(step.y), 0f);

                // Compute swept area tiles
                int tx0 = Mathf.FloorToInt(bbox.min.x / world.tileSize);
                int tx1 = Mathf.FloorToInt((bbox.max.x - EPSILON) / world.tileSize);
                int ty0 = Mathf.FloorToInt(bbox.min.y / world.tileSize);
                int ty1 = Mathf.FloorToInt((bbox.max.y - EPSILON) / world.tileSize);

                float actualMove = step.x != 0 ? step.x : step.y;
                float maxMove = actualMove;

                // For each tile in swept area, check collision and compute allowed move
                for (int tx = tx0; tx &lt;= tx1; ++tx)
                {
                    for (int ty = ty0; ty &lt;= ty1; ++ty)
                    {
                        if (!world.IsSolidAt(tx, ty)) continue;
                        Bounds tile = world.TileBounds(tx, ty);
                        if (!AABBOverlaps(bbox, tile)) continue;

                        // Determine how much we can move before collision
                        if (step.x &gt; 0) // Moving right
                            maxMove = Mathf.Min(maxMove, tile.min.x - (pos.x + size.x));
                        else if (step.x &lt; 0) // Moving left
                            maxMove = Mathf.Max(maxMove, tile.max.x - pos.x);
                        else if (step.y &gt; 0) // Up
                            maxMove = Mathf.Min(maxMove, tile.min.y - (pos.y + size.y));
                        else if (step.y &lt; 0) // Down
                            maxMove = Mathf.Max(maxMove, tile.max.y - pos.y);
                    }
                }

                Vector2 allowedStep = Vector2.zero;
                if (step.x != 0)
                {
                    allowedStep.x = Mathf.Abs(maxMove) &lt; Mathf.Abs(step.x) ? maxMove : step.x;
                    if (step.x &gt; 0 &amp;&amp; allowedStep.x != step.x) result.hitRight = true;
                    if (step.x &lt; 0 &amp;&amp; allowedStep.x != step.x) result.hitLeft = true;
                }
                else if (step.y != 0)
                {
                    allowedStep.y = Mathf.Abs(maxMove) &lt; Mathf.Abs(step.y) ? maxMove : step.y;
                    if (step.y &gt; 0 &amp;&amp; allowedStep.y != step.y) result.hitCeiling = true;
                    if (step.y &lt; 0 &amp;&amp; allowedStep.y != step.y) result.hitGround = true;
                }

                pos += allowedStep;
                allowed += allowedStep;
                remaining -= allowedStep;
                if (allowedStep != step)
                {
                    // Hit something, zero out that axis for future steps (corner slide)
                    if (step.x != 0) remaining.x = 0;
                    if (step.y != 0) remaining.y = 0;
                }
            }
            result.allowedDelta = allowed;
            return result;
        }

        private static bool AABBOverlaps(Bounds a, Bounds b)
        {
            return a.min.x &lt; b.max.x &amp;&amp; a.max.x &gt; b.min.x &amp;&amp;
                   a.min.y &lt; b.max.y &amp;&amp; a.max.y &gt; b.min.y;
        }
    }
}