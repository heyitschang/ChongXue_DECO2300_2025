using System.Linq;
using UnityEngine;

public class Paint360_AutoHands : MonoBehaviour
{
    [Header("Stroke Drawing (right index pinch)")]
    public Stroke strokePrefab;
    public float maxPointHz = 90f;
    [Range(0,1)] public float pinchStart = 0.6f;
    [Range(0,1)] public float pinchEnd   = 0.4f;

    [Header("Erase (open right palm)")]
    public float palmEraseRadius = 0.10f;
    public float eraseHoldDuration = 0.4f;

    [Header("Colors (left-hand palette)")]
    public Color thumbColor  = Color.red;   // Left Thumb = R
    public Color indexColor  = Color.green; // Left Index = G
    public Color middleColor = Color.blue;  // Left Middle = B
    [Range(0,1)] public float palettePinchThreshold = 0.6f;

    [Header("Object Painting (right thumb+middle pinch)")]
    public float paintGestureThreshold = 0.6f;
    public float paintRayLength = 3.0f;
    public LayerMask paintableMask;

    [Header("Debug")]
    public bool debugLogs = false;
    public bool drawGizmos = true;
    public bool showPinchMarker = true;
    public bool showTipMarkers = false;

    [Header("HUD")]
    public Paint360HUD hud;   // assign in Inspector or created at runtime

    // — Internals —
    OVRHand rightHand, leftHand;
    OVRSkeleton rightSkel, leftSkel;

    // Right-hand tips actually used
    Transform rightThumbTip;
    Transform rightIndexTip;
    Transform rightMiddleTip; // ray for object paint

    // Extra joints for palm center calc
    Transform leftWrist, leftIndex1, leftMiddle1, leftRing1, leftPinky1;
    Transform rightWrist, rightIndex1, rightMiddle1, rightRing1, rightPinky1;

    Transform centerEye;

    // Auto-resolve state (right hand) for pinch origin
    bool tipsLocked = false;
    int  settleFrames = 12;
    int  settleCount  = 0;
    Transform resolvedThumbTip, resolvedIndexTip;

    // Eraser state
    float eraseActivationTime = 0f;
    bool eraseActiveBuffered = false;
    bool eraserActive = false;

    Stroke currentStroke;
    float nextPointTime;

    GameObject previewSphere;
    Material previewMat;
    LineRenderer pointerLine;

    // Debug markers (right-hand)
    GameObject pinchMarker, thumbMarker, indexMarker, middleMarker;

    // HUD cache
    Color  _lastShownColor = new Color(0,0,0,0);
    string _lastShownTool  = "";

    void Awake()
    {
        var rig = FindObjectOfType<OVRCameraRig>();
        if (!rig) { Debug.LogError("[Paint360] OVRCameraRig not found."); return; }

        centerEye = rig.centerEyeAnchor;
        rightHand = rig.rightHandAnchor.GetComponentInChildren<OVRHand>(true);
        leftHand  = rig.leftHandAnchor.GetComponentInChildren<OVRHand>(true);
        rightSkel = rig.rightHandAnchor.GetComponentInChildren<OVRSkeleton>(true);
        leftSkel  = rig.leftHandAnchor.GetComponentInChildren<OVRSkeleton>(true);

        // HUD ensure
        if (!hud)
        {
            var go = new GameObject("Paint360HUD");
            hud = go.AddComponent<Paint360HUD>();
        }
        hud.Init(centerEye);
        hud.Ping("Ready", Color.clear);

        InvokeRepeating(nameof(TryBindBonesPassive), 0.05f, 0.1f);

        // Preview sphere (palette indicator ONLY)
        previewSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(previewSphere.GetComponent<Collider>());
        previewSphere.transform.localScale = Vector3.one * 0.05f;
        previewMat = new Material(Shader.Find("Unlit/Color"));
        previewSphere.GetComponent<Renderer>().material = previewMat;

        // Pointer line (object paint ray)
        var pointerGO = new GameObject("PointerLine");
        pointerLine = pointerGO.AddComponent<LineRenderer>();
        pointerLine.startWidth = 0.002f;
        pointerLine.endWidth   = 0.002f;
        pointerLine.material   = new Material(Shader.Find("Unlit/Color"));
        pointerLine.material.color = Color.cyan;
        pointerLine.positionCount = 2;
        pointerLine.enabled = false;

        // Debug markers (right)
        if (showPinchMarker) pinchMarker = MakeMarker(0.012f, Color.yellow);
        if (showTipMarkers)
        {
            thumbMarker  = MakeMarker(0.010f, Color.red);
            indexMarker  = MakeMarker(0.010f, Color.green);
            middleMarker = MakeMarker(0.010f, Color.cyan);
        }
    }

    GameObject MakeMarker(float size, Color c)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(go.GetComponent<Collider>());
        go.transform.localScale = Vector3.one * size;
        var mr = go.GetComponent<Renderer>();
        mr.sharedMaterial = new Material(Shader.Find("Unlit/Color"));
        mr.sharedMaterial.color = c;
        return go;
    }

    // Bind helpful joints without forcing tips yet
    void TryBindBonesPassive()
    {
        if (rightSkel && rightSkel.IsInitialized)
        {
            rightMiddleTip = rightMiddleTip ?? GetBoneTransform(rightSkel, OVRSkeleton.BoneId.Hand_MiddleTip);

            if (!rightWrist)   rightWrist   = GetBoneTransform(rightSkel, OVRSkeleton.BoneId.Hand_WristRoot);
            if (!rightIndex1)  rightIndex1  = GetBoneTransform(rightSkel, OVRSkeleton.BoneId.Hand_Index1);
            if (!rightMiddle1) rightMiddle1 = GetBoneTransform(rightSkel, OVRSkeleton.BoneId.Hand_Middle1);
            if (!rightRing1)   rightRing1   = GetBoneTransform(rightSkel, OVRSkeleton.BoneId.Hand_Ring1);
            if (!rightPinky1)  rightPinky1  = GetBoneTransform(rightSkel, OVRSkeleton.BoneId.Hand_Pinky1);
        }

        if (leftSkel && leftSkel.IsInitialized)
        {
            if (!leftWrist)   leftWrist   = GetBoneTransform(leftSkel, OVRSkeleton.BoneId.Hand_WristRoot);
            if (!leftIndex1)  leftIndex1  = GetBoneTransform(leftSkel, OVRSkeleton.BoneId.Hand_Index1);
            if (!leftMiddle1) leftMiddle1 = GetBoneTransform(leftSkel, OVRSkeleton.BoneId.Hand_Middle1);
            if (!leftRing1)   leftRing1   = GetBoneTransform(leftSkel, OVRSkeleton.BoneId.Hand_Ring1);
            if (!leftPinky1)  leftPinky1  = GetBoneTransform(leftSkel, OVRSkeleton.BoneId.Hand_Pinky1);
        }
    }

    // --- Bone helpers (OVRBone-based) ---
    Transform GetBoneTransform(OVRSkeleton skel, OVRSkeleton.BoneId id)
    {
        if (skel?.Bones == null) return null;
        var bone = skel.Bones.FirstOrDefault(b => b.Id == id);
        return bone != null ? bone.Transform : null;
    }

    bool LooksLike(OVRBone b, string token)
    {
        var idStr = b.Id.ToString().ToLower();
        var nm    = b.Transform ? b.Transform.name.ToLower() : "";
        token = token.ToLower();
        return idStr.Contains(token) || nm.Contains(token);
    }

    void AutoResolveTips()
    {
        if (tipsLocked || rightSkel?.Bones == null || rightHand == null) return;

        float pin = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        if (pin < pinchStart) { settleCount = 0; return; }

        var bones = rightSkel.Bones.Where(b => b.Transform != null).ToList();

        var thumbCands = bones.Where(b => LooksLike(b,"thumb") && (LooksLike(b,"tip") || LooksLike(b,"distal"))).ToList();
        var indexCands = bones.Where(b => LooksLike(b,"index") && (LooksLike(b,"tip") || LooksLike(b,"distal"))).ToList();
        if (thumbCands.Count == 0) thumbCands = bones.Where(b => LooksLike(b,"thumb")).ToList();
        if (indexCands.Count == 0) indexCands = bones.Where(b => LooksLike(b,"index")).ToList();
        if (thumbCands.Count == 0 || indexCands.Count == 0) return;

        float best = float.MaxValue;
        Transform bestThumb = null, bestIndex = null;
        foreach (var t in thumbCands)
        foreach (var i in indexCands)
        {
            float d = (t.Transform.position - i.Transform.position).sqrMagnitude;
            if (d < best) { best = d; bestThumb = t.Transform; bestIndex = i.Transform; }
        }

        resolvedThumbTip = bestThumb;
        resolvedIndexTip = bestIndex;

        settleCount++;
        if (settleCount >= settleFrames && resolvedThumbTip && resolvedIndexTip)
        {
            rightThumbTip = resolvedThumbTip;
            rightIndexTip = resolvedIndexTip;
            tipsLocked = true;
            if (debugLogs)
                Debug.Log($"[Paint360] Auto-locked tips: thumb={rightThumbTip.name}, index={rightIndexTip.name}");
        }
    }

    void Update()
    {
        if (!rightHand || !leftHand || rightSkel == null) return;

        AutoResolveTips();

        // Debug markers (right)
        if (showTipMarkers)
        {
            if (thumbMarker  && rightThumbTip)  thumbMarker.transform.position  = rightThumbTip.position;
            if (indexMarker  && rightIndexTip)  indexMarker.transform.position  = rightIndexTip.position;
            if (middleMarker && rightMiddleTip) middleMarker.transform.position = rightMiddleTip.position;
        }
        if (showPinchMarker && pinchMarker) pinchMarker.transform.position = ComputeRightPinchPoint();

        // Eraser detection (angle-independent)
        CheckEraserGesture();

        if (eraserActive)
            HandleErasing();
        else if (HandleObjectPainting())
        {
            // skip drawing if painting
        }
        else
        {
            HandleDrawing();
        }

        UpdatePreviewSphere();
        UpdatePointerLine();

        // Show HUD when palette color changes while idle
        var paletteCol = GetCurrentPaletteColor();
        bool hasPalette = paletteCol != Color.black; // black means "no selection"
        if (hasPalette && (ColorDistance(paletteCol, _lastShownColor) > 0.02f) && !eraserActive && currentStroke == null)
        {
            ShowHUD("Color", paletteCol);
        }
    }

    // --- Easier, angle-independent eraser detection ---
    void CheckEraserGesture()
    {
        if (rightHand == null) { eraserActive = false; return; }

        float openness =
            1f - ((rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb) +
                   rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index) +
                   rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) +
                   rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring) +
                   rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky)) / 5f);

        bool highConf = rightHand.HandConfidence == OVRHand.TrackingConfidence.High;
        bool handOpen = openness > 0.6f && highConf;

        if (handOpen)
        {
            eraseActivationTime += Time.deltaTime;
            if (eraseActivationTime > eraseHoldDuration)
                eraseActiveBuffered = true;
        }
        else
        {
            eraseActivationTime = 0f;
            eraseActiveBuffered = false;
        }

        eraserActive = eraseActiveBuffered;
    }

    // --- Handle erasing ---
    void HandleErasing()
    {
        if (rightHand == null) return;

        ShowHUD("Erase", new Color(0,0,0,0)); // hide swatch

        Vector3 palmCenter = ComputeRightPalmCenter();
        float effectiveRadius = palmEraseRadius * 1.1f;

        foreach (var s in FindObjectsOfType<Stroke>())
            s.TryEraseAt(palmCenter, effectiveRadius);

        if (debugLogs)
            Debug.Log($"[Paint360] Erasing at {palmCenter} (radius {effectiveRadius:F2})");
    }

    void HandleDrawing()
    {
        if (!rightThumbTip || !rightIndexTip) return;

        float pin = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        bool drawNow = pin >= pinchStart || (currentStroke && pin > pinchEnd);

        if (drawNow)
        {
            if (!currentStroke)
            {
                currentStroke = Instantiate(strokePrefab);
                ShowHUD("Draw", GetCurrentPaletteColor());
            }

            var lr = currentStroke.GetComponent<LineRenderer>();
            if (lr) lr.material.color = GetCurrentPaletteColor();

            if (Time.time >= nextPointTime)
            {
                currentStroke.AddPoint(ComputeRightPinchPoint());
                nextPointTime = Time.time + 1f / Mathf.Max(30f, maxPointHz);
            }
        }
        else if (currentStroke)
        {
            currentStroke = null;
        }
    }

    bool HandleObjectPainting()
    {
        if (!rightMiddleTip) return false;

        float thumbR  = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb);
        float middleR = rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);

        bool paintNow = (thumbR > paintGestureThreshold && middleR > paintGestureThreshold);
        if (!paintNow) return false;

        Color paintColor = GetCurrentPaletteColor();

        if (Physics.Raycast(rightMiddleTip.position, rightMiddleTip.forward, out RaycastHit hit, paintRayLength, paintableMask, QueryTriggerInteraction.Ignore))
        {
            PaintRendererOn(hit.collider.gameObject, paintColor);
            ShowHUD("Paint", paintColor);

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

    // —— ORIGINAL SIMPLE MIXING: threshold each of T/I/M; average selected colors ——
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

        if (list.Count == 0) return Color.black;   // no selection
        if (list.Count == 1) return list[0];

        float r = 0, g = 0, b = 0;
        foreach (var c in list) { r += c.r; g += c.g; b += c.b; }
        return new Color(r / list.Count, g / list.Count, b / list.Count, 1f);
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

    Vector3 ComputeRightPinchPoint()
    {
        if (rightThumbTip && rightIndexTip)
            return (rightThumbTip.position + rightIndexTip.position) * 0.5f;

        if (rightIndexTip)  return rightIndexTip.position;
        if (rightMiddleTip) return rightMiddleTip.position;
        return transform.position;
    }

    void UpdatePreviewSphere()
    {
        if (!previewSphere || !leftWrist) return;

        Vector3 palmCenter = ComputeLeftPalmCenter();
        previewSphere.transform.position = palmCenter + Vector3.up * 0.05f;

        // Always show the current palette color (no magenta for eraser here)
        previewMat.color = GetCurrentPaletteColor();
    }

    void UpdatePointerLine()
    {
        if (!pointerLine || !rightMiddleTip) return;

        Vector3 start = rightMiddleTip.position;
        Vector3 dir   = rightMiddleTip.forward;
        Vector3 end   = start + dir * paintRayLength;

        pointerLine.enabled = true;
        pointerLine.SetPosition(0, start);
        pointerLine.SetPosition(1, end);
    }

    // —— HUD helpers —— 
    void ShowHUD(string tool, Color color)
    {
        if (!hud) return;

        if (tool != _lastShownTool || ColorDistance(color, _lastShownColor) > 0.01f)
        {
            hud.Ping(tool, color.a > 0 ? color : new Color(0,0,0,0));
            _lastShownTool  = tool;
            _lastShownColor = color;
        }
    }

    float ColorDistance(Color a, Color b)
    {
        float dr = a.r - b.r, dg = a.g - b.g, db = a.b - b.b;
        return Mathf.Sqrt(dr*dr + dg*dg + db*db);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || !Application.isPlaying) return;

        if (eraserActive && rightWrist)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(ComputeRightPalmCenter(), palmEraseRadius * 1.1f);
        }
    }
}
