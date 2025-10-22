using System.Linq;
using UnityEngine;

public class Paint360Hands_OVR : MonoBehaviour
{
    [Header("Hands (from Camera Rig)")]
    public OVRHand rightHand;
    public OVRSkeleton rightSkeleton;
    public OVRHand leftHand;
    public OVRSkeleton leftSkeleton;

    [Header("Stroke")]
    public Stroke strokePrefab;
    public float maxPointHz = 90f;
    public float pinchStart = 0.6f; 
    public float pinchEnd   = 0.4f; 

    [Header("Erase (left palm)")]
    public float palmEraseRadius = 0.05f;    
    public bool requirePalmFacingHead = true;
    public float palmFacingDotMin = 0.35f;   

    Stroke currentStroke;
    float nextPointTime;
    Transform rightIndexTip, leftPalm, centerEye;

    void Start()
    {
        var rig = FindObjectOfType<OVRCameraRig>();
        centerEye = rig ? rig.centerEyeAnchor : Camera.main?.transform;

        TryBindJoints();
        InvokeRepeating(nameof(TryBindJoints), 0.1f, 0.1f);
    }

    void TryBindJoints()
    {
        if (rightSkeleton && rightSkeleton.IsInitialized && rightIndexTip == null)
            rightIndexTip = GetBone(rightSkeleton, OVRSkeleton.BoneId.Hand_IndexTip);

        if (leftSkeleton && leftSkeleton.IsInitialized && leftPalm == null)
            leftPalm = GetBone(leftSkeleton, OVRSkeleton.BoneId.Hand_WristRoot);

        if (rightIndexTip && leftPalm) CancelInvoke(nameof(TryBindJoints));
    }

    Transform GetBone(OVRSkeleton skel, OVRSkeleton.BoneId id)
    {
        var bone = skel.Bones?.FirstOrDefault(b => b.Id == id);
        return bone?.Transform;
    }

    void Update()
    {
        if (!rightHand || !leftHand || rightIndexTip == null || leftPalm == null) return;

        HandleDrawing();
        HandleErasing();
    }

    void HandleDrawing()
    {
        float pin = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        bool drawNow = pin >= pinchStart || (currentStroke && pin > pinchEnd);

        if (drawNow)
        {
            if (!currentStroke)
                currentStroke = Instantiate(strokePrefab);

            if (Time.time >= nextPointTime)
            {
                currentStroke.AddPoint(rightIndexTip.position);
                nextPointTime = Time.time + 1f / Mathf.Max(30f, maxPointHz);
            }
        }
        else if (currentStroke)
        {
            currentStroke = null; 
        }
    }

    void HandleErasing()
    {
        if (leftHand.HandConfidence != OVRHand.TrackingConfidence.High) return;

        if (requirePalmFacingHead && centerEye)
        {
            Vector3 palmNormal = -leftPalm.forward;
            Vector3 toHead = (centerEye.position - leftPalm.position).normalized;
            if (Vector3.Dot(palmNormal, toHead) < palmFacingDotMin) return;
        }

        Vector3 p = leftPalm.position;
        foreach (var s in FindObjectsOfType<Stroke>())
            s.TryEraseAt(p, palmEraseRadius);
    }
}
