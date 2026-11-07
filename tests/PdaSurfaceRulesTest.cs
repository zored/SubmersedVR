using System;
using ZoredPdaScale;
class PdaSurfaceRulesTest {
 static void Check(bool v,string m) { if(!v) throw new Exception(m); }
 static void Main() {
  Check(PdaSurfaceRules.ProbeStartDistance(-.3f, true) == .1f, "held probe stays in front at depth");
  Check(PdaSurfaceRules.ProbeStartDistance(.04f, false) == .14f, "unheld probe follows finger");
  Check(PdaSurfaceRules.AcceptDepth(-.3f,true), "held deep press stays valid");
  Check(!PdaSurfaceRules.AcceptDepth(-.3f,false), "unheld deep finger cannot press");
  Check(PdaSurfaceRules.VisualCorrection(-.04f,true) == .04f, "visual finger stops at surface");
  Check(PdaSurfaceRules.VisualCorrection(.02f,true) == 0f, "withdrawal follows tracked finger");
  Check(PdaSurfaceRules.VisualCorrection(-.04f,false) == 0f, "no correction without touch");
 }
}
