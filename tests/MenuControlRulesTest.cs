using System;
using ZoredPdaScale;
class MenuControlRulesTest
{
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Main()
    {
        float ui, tablet;
        MenuControlRules.AdjustPdaSize(2f, 1.9f, true, out ui, out tablet);
        Check(Math.Abs(ui - 2.1f) < .0001f && Math.Abs(tablet - 1.995f) < .0001f, "increase keeps the selected relative size");
        MenuControlRules.AdjustPdaSize(ui, tablet, false, out ui, out tablet);
        Check(Math.Abs(ui - 2f) < .0001f && Math.Abs(tablet - 1.9f) < .0001f, "increase and decrease return to the same size");
        MenuControlRules.AdjustPdaSize(2.99f, 2.8f, true, out ui, out tablet);
        Check(ui <= 3.00001f && Math.Abs(ui / tablet - 2.99f / 2.8f) < .0001f, "upper boundary keeps the ratio");
        MenuControlRules.AdjustPdaSize(.51f, .7f, false, out ui, out tablet);
        Check(ui >= .49999f && Math.Abs(ui / tablet - .51f / .7f) < .0001f, "lower boundary keeps the ratio");
        Check(Math.Abs(MenuControlRules.Adjust(1f, 0.1f, .5f, 2f) - 1.1f) < .0001f, "increase");
        Check(MenuControlRules.Adjust(2f, .1f, .5f, 2f) == 2f, "maximum");
        Check(MenuControlRules.Adjust(.5f, -.1f, .5f, 2f) == .5f, "minimum");
        Check(MenuControlRules.MayRecover(true, false, true, 0f, false), "explicit VR action can recover in background");
        Check(!MenuControlRules.MayRecover(true, false, false, 0f, false), "keyboard recovery requires focus");
        Check(!MenuControlRules.MayRecover(true, true, true, 0f, true), "legitimate pause must remain frozen");
        Check(!MenuControlRules.MayRecover(false, true, true, 0f, false), "PDA must be in use");
        Check(!MenuControlRules.MayRecover(true, true, true, 1f, false), "running time is not a freeze");
        Check(!MenuControlRules.ShouldRetryCamera(9f, 10f), "do not retry early");
        Check(MenuControlRules.ShouldRetryCamera(10f, 10f), "retry after layer becomes available");
        Check(MenuControlRules.ShouldStartWatcher(true, false), "start configured watcher when absent");
        Check(!MenuControlRules.ShouldStartWatcher(false, false), "do not start disabled watcher");
        Check(!MenuControlRules.ShouldStartWatcher(true, true), "do not start duplicate watcher");
        Check(MenuControlRules.Steps(1f, 1f, .1f) == 0, "baseline displays zero");
        Check(MenuControlRules.Steps(1.2f, 1f, .1f) == 2, "positive adjustments shown");
        Check(MenuControlRules.Steps(.8f, 1f, .1f) == -2, "negative adjustments shown");
    }
}
