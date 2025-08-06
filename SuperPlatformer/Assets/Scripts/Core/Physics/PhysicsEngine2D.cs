using UnityEngine;

namespace Core.Physics
{
    /// &lt;summary&gt;
    /// Stub for custom tile-based physics engine. To be implemented in a future milestone.
    /// &lt;/summary&gt;
    public class PhysicsEngine2D
    {
        /// &lt;summary&gt;
        /// Returns true if the given bounds are grounded (touching a tile from below).
        /// &lt;/summary&gt;
        public static bool CheckGrounded(Bounds bounds)
        {
            // TODO: Implement tile collision detection
            return false;
        }

        /// &lt;summary&gt;
        /// Resolves collisions and returns a new position vector.
        /// &lt;/summary&gt;
        public static Vector2 ResolveCollisions(Vector2 position, Vector2 velocity, Bounds bounds)
        {
            // TODO: Implement tile collision response
            return position + velocity * Time.fixedDeltaTime;
        }
    }
}