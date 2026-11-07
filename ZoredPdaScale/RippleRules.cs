namespace ZoredPdaScale
{
    // Emit at contact and after a minimum movement and interval, never per frame.
    internal sealed class RippleRules
    {
        private float x, y, lastTime;
        private bool tracking;
        internal bool Update(bool press, bool held, float nextX, float nextY, float time)
        {
            if (!held) { tracking = false; return false; }
            float dx = nextX - x, dy = nextY - y;
            if (!press && tracking && (time - lastTime < .24f || dx * dx + dy * dy < .0004f)) return false;
            x = nextX; y = nextY; lastTime = time; tracking = true;
            return true;
        }
        internal void Clear() { tracking = false; }
    }
}