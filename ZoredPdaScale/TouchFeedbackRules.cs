namespace ZoredPdaScale
{
    internal sealed class TouchFeedbackRules
    {
        private float pressUntil;
        internal int State(bool valid, float distance, bool pressed, float now)
        {
            if (!valid) { pressUntil = 0f; return 0; }
            if (pressed) pressUntil = now + .18f;
            if (now < pressUntil) return 2;
            return distance <= .07f ? 1 : 0;
        }
    }
}
