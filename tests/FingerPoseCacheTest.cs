using System;
using ZoredPdaScale;

class FingerPoseCacheTest
{
    static int Main()
    {
        var cache = new FingerPoseCache<int>();
        int value;
        if (cache.TryGet(out value)) return 1;
        cache.Save(42);
        if (!cache.TryGet(out value) || value != 42) { Console.WriteLine("Saved fingertip pose was not retained for the next input update"); return 2; }
        cache.Clear();
        if (cache.TryGet(out value)) { Console.WriteLine("Old pose survived reset"); return 3; }
        return 0;
    }
}
