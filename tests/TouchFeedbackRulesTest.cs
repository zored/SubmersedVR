using System;
using ZoredPdaScale;
class TouchFeedbackRulesTest
{
 static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
 static void Main() {
  var f = new TouchFeedbackRules();
  Check(f.State(true, .2f, false, 1f) == 0, "far target is normal");
  Check(f.State(true, .04f, false, 1f) == 1, "near target gives warning");
  Check(f.State(true, .01f, true, 2f) == 2, "touch gives press feedback");
  Check(f.State(true, .01f, false, 2.1f) == 2, "press flash remains visible");
  Check(f.State(true, .04f, false, 2.3f) == 1, "flash returns to proximity");
  Check(f.State(false, 0f, false, 2.31f) == 0, "closed or lost target restores normal color");
 }
}
