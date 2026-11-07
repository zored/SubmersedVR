namespace ZoredPdaScale
{
    internal enum TouchEvent { None, Press, Release }
    // Distances are measured along the front-facing normal of the PDA screen.
    internal sealed class FingerTouchRules
    {
        private const float Contact = .008f;
        private const float Release = .025f;
        private const float Rearm = .035f;
        private const float MaxStep = .20f;
        private bool armed, previousValid;
        private float previousDistance;
        internal bool Held { get; private set; }
        internal void Cancel() { Held = false; armed = false; previousValid = false; }
        internal TouchEvent Step(bool valid, float distance, bool physical)
        {
            if (physical) { Cancel(); return TouchEvent.None; }
            if (Held)
            {
                if (!valid || distance >= Release)
                {
                    Held = false;
                    armed = valid && distance >= Rearm;
                    previousValid = valid;
                    previousDistance = distance;
                    return TouchEvent.Release;
                }
                previousDistance = distance;
                previousValid = true;
                return TouchEvent.None;
            }
            if (!valid) { armed = false; previousValid = false; return TouchEvent.None; }
            if (distance >= Rearm) armed = true;
            float movement = previousDistance - distance;
            bool crossing = armed && previousValid && previousDistance > Contact && distance <= Contact &&
                            distance >= -.08f && movement > .0001f && movement <= MaxStep;
            if (armed && previousValid && movement > MaxStep) armed = false;
            previousDistance = distance;
            previousValid = true;
            if (!crossing) return TouchEvent.None;
            armed = false;
            Held = true;
            return TouchEvent.Press;
        }
    }
}
