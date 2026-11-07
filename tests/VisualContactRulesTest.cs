using System;
using ZoredPdaScale;
class VisualContactRulesTest {
 static void Check(bool v,string m) { if(!v) throw new Exception(m); }
 static void Main() {
  var s = new VisualContactRules();
  Check(!s.Step(true,.04f),"approaching");
  Check(s.Step(true,-.01f),"crossing glass stops finger");
  Check(s.Step(true,-.30f),"deep press stays constrained");
  Check(s.Step(true,.01f),"shallow withdrawal preserves contact");
  Check(!s.Step(true,.04f),"withdrawal releases");
  Check(!s.Step(true,-.3f),"tracking jump is ignored");
  Check(!s.Step(false,0f),"leaving glass resets");
  Check(!s.Step(true,-.01f),"opening behind glass cannot latch");
 }
}
