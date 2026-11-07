namespace ZoredPdaScale
{
    internal sealed class VisualContactRules
    {
        private bool previous, contact;
        private float previousDistance;
        internal bool Step(bool valid, float distance)
        {
            if (!valid) { previous = contact = false; return false; }
            if (distance > .025f) contact = false;
            if (!contact && previous && previousDistance >= 0f && distance < 0f && previousDistance - distance <= .20f) contact = true;
            previous = true; previousDistance = distance;
            return contact;
        }
    }
}
