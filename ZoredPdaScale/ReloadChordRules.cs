namespace ZoredPdaScale
{
    internal sealed class ReloadChordRules
    {
        private bool armed, holding;
        private float started;
        internal bool Step(bool open, bool left, bool right, float now)
        {
            if (!left && !right) { armed = true; holding = false; return false; }
            if (!open || !left || !right) { if (holding) armed = false; holding = false; return false; }
            if (!armed) return false;
            if (!holding) { holding = true; started = now; return false; }
            if (now - started < 3f) return false;
            armed = false; holding = false; return true;
        }
    }
}
