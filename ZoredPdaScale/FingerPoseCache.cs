namespace ZoredPdaScale
{
    internal sealed class FingerPoseCache<T>
    {
        private T position;
        private bool valid;
        internal void Save(T value) { position = value; valid = true; }
        internal bool TryGet(out T value) { value = position; return valid; }
        internal void Clear() { position = default(T); valid = false; }
    }
}
