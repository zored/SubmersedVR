using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace ZoredPdaScale
{
    // Renders only the right-hand mesh and a soft symbolic head into two small textures.
    internal sealed class PdaHandReflection : IDisposable
    {
        private readonly Camera capture;
        private readonly RenderTexture[] targets = new RenderTexture[2];
        private readonly CommandBuffer commands;
        private readonly Material handMaterial, headMaterial;
        private readonly Mesh headMesh;
        private readonly List<SkinnedMeshRenderer> hands = new List<SkinnedMeshRenderer>();
        private readonly FieldInfo mainCameraField = AccessTools.Field(typeof(SNCameraRoot), "mainCamera");
        private Transform hand;
        private Player owner;
        private bool disposed, logged;
        private float nextRender;
        internal PdaHandReflection(Shader mask)
        {
            var go = new GameObject("Zored PDA reflection camera");
            UnityEngine.Object.DontDestroyOnLoad(go);
            capture = go.AddComponent<Camera>(); capture.enabled = false;
            for (int i=0; i<2; i++) { targets[i] = new RenderTexture(256,256,16,RenderTextureFormat.ARGB32); targets[i].name="PDA reflection " + i; targets[i].Create(); }
            commands = new CommandBuffer(); commands.name = "PDA reflected hand and head";
            handMaterial = new Material(mask); handMaterial.color = new Color(.65f,.8f,.85f,.14f);
            headMaterial = new Material(mask); headMaterial.color = new Color(.25f,.35f,.4f,.09f);
            headMaterial.SetFloat("_HandRadius", 1f);
            headMesh = BuildHeadMesh();
        }
        private static Mesh BuildHeadMesh()
        {
            var mesh = new Mesh(); mesh.name="PDA reflection head silhouette";
            const int rows=8, cols=12;
            var vertices=new Vector3[(rows+1)*(cols+1)]; var indices=new int[rows*cols*6];
            for (int y=0;y<=rows;y++) { float a=Mathf.PI*y/rows; for (int x=0;x<=cols;x++) { float b=2f*Mathf.PI*x/cols; vertices[y*(cols+1)+x]=new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b)); } }
            int n=0; for (int y=0;y<rows;y++) for (int x=0;x<cols;x++) { int k=y*(cols+1)+x; indices[n++]=k; indices[n++]=k+cols+1; indices[n++]=k+1; indices[n++]=k+1; indices[n++]=k+cols+1; indices[n++]=k+cols+2; }
            mesh.vertices=vertices; mesh.triangles=indices; mesh.RecalculateBounds(); return mesh;
        }
        private bool FindHand(Player player)
        {
            if (owner == player && hand != null && hands.Count > 0) return true;
            hands.Clear(); owner=player; hand=null;
            var animator=player == null ? null : player.playerAnimator;
            if (animator == null) return false;
            const string prefix="export_skeleton/head_rig/neck/chest/clav_R/clav_R_aim/shoulder_R/";
            hand=animator.transform.Find(prefix+"hand_R");
            if (hand == null) hand=animator.transform.Find(prefix+"elbow_R/hand_R");
            if (hand == null) return false;
            foreach (var candidate in player.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (candidate == null || candidate.sharedMesh == null) continue;
                var name=candidate.name.ToLowerInvariant();
                if (name.Contains("hand") || name.Contains("glove")) hands.Add(candidate);
            }
            return hands.Count > 0;
        }
        internal void Render(MeshRenderer screen, Material glass)
        {
            if (disposed) return;
            try { RenderUnsafe(screen, glass); }
            catch (Exception e) { Debug.LogWarning("Zored PDA reflection disabled: " + e.Message); if (glass != null) glass.SetFloat("_ReflectionEnabled",0f); Dispose(); }
        }
        private void RenderUnsafe(MeshRenderer screen, Material glass)
        {
            if (disposed || screen == null || glass == null) return;
            if (Time.unscaledTime < nextRender) return;
            nextRender = Time.unscaledTime + .033f;
            var player=Player.main; var root=SNCameraRoot.main;
            var source=root == null || mainCameraField == null ? null : mainCameraField.GetValue(root) as Camera;
            if (source == null || !FindHand(player)) { glass.SetFloat("_ReflectionEnabled",0f); return; }
            var filter=screen.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) { glass.SetFloat("_ReflectionEnabled",0f); return; }
            var planePoint=screen.transform.TransformPoint(filter.sharedMesh.bounds.center);
            var normal=screen.transform.up.normalized;
            var reflection=Reflection(normal,planePoint);
            handMaterial.SetVector("_HandCenter",hand.position); handMaterial.SetFloat("_HandRadius",.16f);
            var headCenter=source.transform.position - source.transform.forward*.06f;
            headMaterial.SetVector("_HandCenter",headCenter);
            commands.Clear();
            foreach (var skin in hands) if (skin != null && skin.enabled && skin.gameObject.activeInHierarchy)
                for (int j=0;j<skin.sharedMesh.subMeshCount;j++) commands.DrawRenderer(skin,handMaterial,j);
            commands.DrawMesh(headMesh,Matrix4x4.TRS(headCenter,source.transform.rotation,new Vector3(.09f,.115f,.085f)),headMaterial);
            capture.CopyFrom(source); capture.enabled=false; capture.stereoTargetEye=StereoTargetEyeMask.None;
            capture.cullingMask=0; capture.clearFlags=CameraClearFlags.SolidColor; capture.backgroundColor=Color.clear;
            capture.RemoveAllCommandBuffers(); capture.AddCommandBuffer(CameraEvent.BeforeForwardOpaque,commands);
            bool old=GL.invertCulling;
            try
            {
                for (int eye=0;eye<2;eye++)
                {
                    Matrix4x4 view=source.worldToCameraMatrix, projection=source.projectionMatrix;
                    if (source.stereoEnabled) { var e=eye==0 ? Camera.StereoscopicEye.Left : Camera.StereoscopicEye.Right; view=source.GetStereoViewMatrix(e); projection=source.GetStereoProjectionMatrix(e); }
                    capture.worldToCameraMatrix=view*reflection; capture.projectionMatrix=projection; capture.targetTexture=targets[eye];
                    GL.invertCulling=!old; capture.Render(); GL.invertCulling=old;
                }
            }
            finally { GL.invertCulling=old; capture.targetTexture=null; capture.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque,commands); }
            glass.SetTexture("_ReflectionLeft",targets[0]); glass.SetTexture("_ReflectionRight",targets[1]); glass.SetFloat("_ReflectionEnabled",1f);
            if (!logged) { Debug.Log("Zored PDA reflection active: hand renderers=" + hands.Count + ", stereo=" + source.stereoEnabled); logged = true; }
        }
        internal static Matrix4x4 Reflection(Vector3 normal, Vector3 point)
        {
            normal.Normalize(); float d=-Vector3.Dot(normal,point); var p=new Vector4(normal.x,normal.y,normal.z,d); var m=Matrix4x4.identity;
            for(int row=0;row<3;row++) for(int col=0;col<4;col++) m[row,col]-=2f*p[row]*p[col]; return m;
        }
        public void Dispose()
        {
            if (disposed) return; disposed=true;
            if(capture!=null) UnityEngine.Object.Destroy(capture.gameObject);
            commands.Release(); foreach(var target in targets) if(target!=null) { target.Release(); UnityEngine.Object.Destroy(target); }
            UnityEngine.Object.Destroy(handMaterial); UnityEngine.Object.Destroy(headMaterial); UnityEngine.Object.Destroy(headMesh); hands.Clear();
        }
    }
}