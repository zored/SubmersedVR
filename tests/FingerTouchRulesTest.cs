using System;
using ZoredPdaScale;
class FingerTouchRulesTest
{
 static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
 static void Main()
 {
  var t = new FingerTouchRules();
  Check(t.Step(true, .004f, false) == TouchEvent.None, "opening already at screen cannot press");
  Check(t.Step(true, .004f, false) == TouchEvent.None, "stationary finger cannot press");
  Check(t.Step(true, .06f, false) == TouchEvent.None, "withdrawal arms sensor");
  Check(t.Step(true, .02f, false) == TouchEvent.None, "approaching without contact cannot press");
  Check(t.Step(true, .006f, false) == TouchEvent.Press, "crossing surface toward it starts contact");
  Check(t.Step(true, .006f, false) == TouchEvent.None && t.Held, "stationary contact stays held");
  Check(t.Step(true, .015f, false) == TouchEvent.None && t.Held, "small separation while sliding remains held");
  Check(t.Step(true, .03f, false) == TouchEvent.Release && !t.Held, "withdrawal releases");
  Check(t.Step(true, .006f, false) == TouchEvent.None, "must withdraw far enough before another touch");
  t.Step(true, .06f, false);
  Check(t.Step(true, -.02f, false) == TouchEvent.Press, "fast movement crossing the plane presses");
  Check(t.Step(false, 0f, false) == TouchEvent.Release, "losing target ends contact");
  Check(t.Step(true, .004f, false) == TouchEvent.None, "target reappearing near finger cannot press");
  t.Step(true, .4f, false);
  Check(t.Step(true, -.02f, false) == TouchEvent.None, "tracking jump cannot cause accidental press");
  t.Step(true, .06f, false);
  Check(t.Step(true, .006f, false) == TouchEvent.Press, "touch recovers after withdrawal");
  Check(t.Step(true, .006f, true) == TouchEvent.None && !t.Held, "controller takes ownership");
  Check(t.Step(true, .006f, false) == TouchEvent.None, "controller release cannot cause touch");
  t.Step(true, .06f, false); t.Step(true, .006f, false); t.Cancel();
  Check(!t.Held && t.Step(true, .004f, false) == TouchEvent.None, "cancel clears contact");
 }
}
