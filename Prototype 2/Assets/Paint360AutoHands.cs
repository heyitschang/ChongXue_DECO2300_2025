using System.Linq;
using UnityEngine;

public class Paint360_AutoHands : MonoBehaviour
{
    [Header("Stroke Drawing (right index pinch)")]
    public Stroke strokePrefab;
    public float maxPointHz = 90f;
    [Range(0,1)] public float pinchStart = 0.6f;
    [Range(0,1)] public float pinchEnd   = 0.4f;

    [Header("Erase (right palm)")]
    public float palmEraseRadius = 0.10f;
    public float eraseGestureThreshold = 0.6f; 
    public bool requirePalmFacingHead = false;
    public float palmFacingDotMin = 0.35f;

    [Header("Colors (left-hand palette)")]
    public Color thumbColor  = Color.red;
    public Color indexColor  = Color.green;
    public Color middleColor = Color.blue;
    [Range(0,1)] public float palettePinchThreshold = 0.6f;

    [Header("Object Painting (right thumb+middle pinch)")]
    public float paintGestureThreshold = 0.6f;
    public float paintRayLength = 3.0f; 
    public LayerMask paintableMask;    

    [Header("Debug")]
    public bool debugLogs = false;
    public bool drawGizmos = true;

    OVRHand rightHand, leftHand;
    OVRSkeleton rightSkel, leftSkel;
    Transform rightIndexTip, rightMiddleTip;
    Transform leftWrist, leftIndex1, leftMiddle1, leftRing1, leftPinky1;
    Transform rightWrist, rightIndex1, rightMiddle1, rightRing1, rightPinky1;
    Transform centerEye;

    Stroke currentStroke;
    float nextPointTime;
    bool eraserActive;

    GameObject previewSphere;
    Material previewMat;
    LineRenderer pointerLine;

    void Awake()
    {
        var rig = FindObjectOfType<OVRCameraRig>();
        if (!rig) { Debug.LogError("[Paint360] OVRCameraRig not found."); return; }

        centerEye = rig.centerEyeAnchor;

        rightHand = rig.rightHandAnchor.GetComponentInChildren<OVRHand>(true);
        leftHand  = rig.leftHandAnchor.GetComponentInChildren<OVRHand>(true);

        rightSkel = rig.rightHandAnchor.GetComponentInChildren<OVRSkeleton>(true);
        leftSkel  = rig.leftHandAnchor.GetComponentInChildren<OVRSkeleton>(true);

        InvokeRepeating(nameof(TryBindBones), 0.05f, 0.1f);

        previewSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(previewSphere.GetComponent<Collider>());
        previewSphere.transform.localScale = Vector3.one * 0.05f;
        previewMat = new Material(Shader.Find("Unlit/Color"));
        previewSphere.GetComponent<Renderer>().material = previewMat;

        var pointerGO = new GameObject("PointerLine");
        pointerLine = pointerGO.AddComponent<LineRenderer>();
        pointerLine.startWidth = 0.002f;
        pointerLine.endWidth = 0.002f;
        pointerLine.material = new Material(Shader.Find("Unlit/Color"));
        pointerLine.material.color = Color.cyan;
        pointerLine.positionCount = 2;
        pointerLine.enabled = false;
    }

    void TryBindBones()
    {
        if (rightSkel && rightSkel.IsInitialized)
        {
            if (!rightIndexTip) rightIndexTip = GetBone(rightSkel, OVRSkeleton.BoneId.Hand_IndexTip);
            if (!rightMiddleTip) rightMiddleTip = GetBone(rightSkel, OVRSkeleton.BoneId.Hand_MiddleTip);

            if (!rightWrist)    rightWrist    = GetBone(rightSkel, OVRSkeleton.BoneId.Hand_WristRoot);
            if (!rightIndex1)   rightIndex1   = GetBone(rightSkel, OVRSkeleton.BoneId.Hand_Index1);
            if (!rightMiddle1)  rightMiddle1  = GetBone(rightSkel, OVRSkeleton.BoneId.Hand_Middle1);
            if (!rightRing1)    rightRing1    = GetBone(rightSkel, OVRSkeleton.BoneId.Hand_Ring1);
            if (!rightPinky1)   rightPinky1   = GetBone(rightSkel, OVRSkeleton.BoneId.Hand_Pinky1);
        }

        if (leftSkel && leftSkel.IsInitialized)
        {
            if (!leftWrist)   leftWrist   = GetBone(leftSkel, OVRSkeleton.BoneId.Hand_WristRoot);
            if (!leftIndex1)  leftIndex1  = GetBone(leftSkel, OVRSkeleton.BoneId.Hand_Index1);
            if (!leftMiddle1) leftMiddle1 = GetBone(leftSkel, OVRSkeleton.BoneId.Hand_Middle1);
            if (!leftRing1)   leftRing1   = GetBone(leftSkel, OVRSkeleton.BoneId.Hand_Ring1);
            if (!leftPinky1)  leftPinky1  = GetBone(leftSkel, OVRSkeleton.BoneId.Hand_Pinky1);
        }

        if (rightIndexTip && rightMiddleTip && rightWrist && leftWrist)
        {
            if (debugLogs) Debug.Log("[Paint360] Bones bound OK");
            CancelInvoke(nameof(TryBindBones));
        }
    }

    Transform GetBone(OVRSkeleton skel, OVRSkeleton.BoneId id)
    {
        var bone = skel.Bones?.FirstOrDefault(b => b.Id == id);
        return bone?.Transform;
    }

    void Update()
    {
        if (!rightHand || !leftHand || !rightIndexTip || !rightMiddleTip) return;

        CheckEraserGesture();

        if (eraserActive)
            HandleErasing();
        else if (HandleObjectPainting())
            { /* skip drawing if painting */ }
        else
            HandleDrawing();

        UpdatePreviewSphere();
        UpdatePointerLine();
    }

    void CheckEraserGesture()
    {
        float thumbL = leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb);
        float pinkyL = leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky);
        eraserActive = (thumbL > eraseGestureThreshold && pinkyL > eraseGestureThreshold);
        if (debugLogs) Debug.Log($"[Paint360] Eraser {(eraserActive ? "ON" : "OFF")}");
    }

    void HandleDrawing()
    {
        float pin = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        bool drawNow = pin >= pinchStart || (currentStroke && pin > pinchEnd);

        if (drawNow)
        {
            if (!currentStroke) currentStroke = Instantiate(strokePrefab);

            var lr = currentStroke.GetComponent<LineRenderer>();
            if (lr) lr.material.color = GetCurrentPaletteColor();

            if (Time.time >= nextPointTime)
            {
                currentStroke.AddPoint(rightIndexTip.position);
                nextPointTime = Time.time + 1f / Mathf.Max(30f, maxPointHz);
            }
        }
        else if (currentStroke) currentStroke = null;
    }

    bool HandleObjectPainting()
    {
        float thumbR  = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb);
        float middleR = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);

        bool paintNow = (thumbR > paintGestureThreshold && middleR > paintGestureThreshold);
        if (!paintNow) return false;

        Color paintColor = GetCurrentPaletteColor();

        if (Physics.Raycast(rightMiddleTip.position, rightMiddleTip.forward, out RaycastHit hit, paintRayLength, paintableMask, QueryTriggerInteraction.Ignore))
        {
            PaintRendererOn(hit.collider.gameObject, paintColor);
            if (debugLogs) Debug.Log($"[Paint360] Painted {hit.collider.name}");
            return true;
        }
        return false;
    }

    void PaintRendererOn(GameObject go, Color color)
    {
        var r = go.GetComponent<Renderer>();
        if (r != null) r.material.color = color;

        foreach (var cr in go.GetComponentsInChildren<Renderer>())
            cr.material.color = color;
    }

    Color GetCurrentPaletteColor()
    {
        if (!leftHand) return Color.black;

        float t = leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb);
        float i = leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float m = leftHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);

        var list = new System.Collections.Generic.List<Color>();
        if (t > palettePinchThreshold) list.Add(thumbColor);
        if (i > palettePinchThreshold) list.Add(indexColor);
        if (m > palettePinchThreshold) list.Add(middleColor);

        if (list.Count == 0) return Color.black;
        if (list.Count == 1) return list[0];

        float r = 0, g = 0, b = 0;
        foreach (var c in list) { r += c.r; g += c.g; b += c.b; }
        return new Color(r / list.Count, g / list.Count, b / list.Count, 1f);
    }

    void HandleErasing()
    {
        if (rightHand.HandConfidence != OVRHand.TrackingConfidence.High) return;

        Vector3 palmCenter = ComputeRightPalmCenter();

        foreach (var s in FindObjectsOfType<Stroke>())
            s.TryEraseAt(palmCenter, palmEraseRadius);

        if (debugLogs) Debug.Log($"[Paint360] Right palm erasing at {palmCenter}");
    }


    Vector3 ComputeRightPalmCenter()
    {
        int count = 0; Vector3 sum = Vector3.zero;
        if (rightWrist)   { sum += rightWrist.position;   count++; }
        if (rightIndex1)  { sum += rightIndex1.position;  count++; }
        if (rightMiddle1) { sum += rightMiddle1.position; count++; }
        if (rightRing1)   { sum += rightRing1.position;   count++; }
        if (rightPinky1)  { sum += rightPinky1.position;  count++; }
        return count > 0 ? (sum / count) : (rightWrist ? rightWrist.position : Vector3.zero);
    }

    Vector3 ComputeLeftPalmCenter()
    {
        int count = 0; Vector3 sum = Vector3.zero;
        if (leftWrist)   { sum += leftWrist.position;   count++; }
        if (leftIndex1)  { sum += leftIndex1.position;  count++; }
        if (leftMiddle1) { sum += leftMiddle1.position; count++; }
        if (leftRing1)   { sum += leftRing1.position;   count++; }
        if (leftPinky1)  { sum += leftPinky1.position;  count++; }
        return count > 0 ? (sum / count) : (leftWrist ? leftWrist.position : Vector3.zero);
    }

    void UpdatePreviewSphere()
    {
        if (!previewSphere || !leftWrist) return;

        Vector3 palmCenter = ComputeLeftPalmCenter();
        previewSphere.transform.position = palmCenter + Vector3.up * 0.05f;

        previewMat.color = eraserActive ? Color.magenta : GetCurrentPaletteColor();
    }

    void UpdatePointerLine()
    {
        if (!pointerLine || !rightMiddleTip) return;

        Vector3 start = rightMiddleTip.position;
        Vector3 dir = rightMiddleTip.forward;
        Vector3 end = start + dir * paintRayLength;

        pointerLine.enabled = true;
        pointerLine.SetPosition(0, start);
        pointerLine.SetPosition(1, end);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || !Application.isPlaying) return;

        if (eraserActive && rightWrist)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(ComputeRightPalmCenter(), palmEraseRadius);
        }
    }
}
