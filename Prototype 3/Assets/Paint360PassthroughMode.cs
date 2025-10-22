using System.Collections.Generic;
using UnityEngine;

public class Paint360PassthroughMode : MonoBehaviour
{
    public enum Mode { Scene, Passthrough }

    [Header("Start Mode")]
    public Mode startMode = Mode.Scene;

    [Header("Scene roots to hide when Passthrough is ON")]
    public List<GameObject> sceneRoots = new List<GameObject>();

    // refs
    Transform centerEye;
    OVRPassthroughLayer passthroughLayer;

    public Mode Current { get; private set; }

    void Awake()
    {
        var rig = FindObjectOfType<OVRCameraRig>();
        centerEye = rig ? rig.centerEyeAnchor : null;
    }

    void Start()
    {
        SetMode(startMode);
    }

    public void ToggleMode()
    {
        SetMode(Current == Mode.Scene ? Mode.Passthrough : Mode.Scene);
    }

    public void SetMode(Mode mode)
    {
        Current = mode;

        bool enablePT = (mode == Mode.Passthrough);
        ToggleSceneRoots(!enablePT);
        TogglePassthrough(enablePT);

        Debug.Log("[Paint360Passthrough] Mode -> " + mode);
    }

    void ToggleSceneRoots(bool enable)
    {
        foreach (var go in sceneRoots)
            if (go) go.SetActive(enable);
    }

    void TogglePassthrough(bool enable)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (enable)
        {
            if (!passthroughLayer)
            {
                if (centerEye)
                {
                    passthroughLayer = centerEye.GetComponent<OVRPassthroughLayer>();
                    if (!passthroughLayer) passthroughLayer = centerEye.gameObject.AddComponent<OVRPassthroughLayer>();
                }
                else
                {
                    var found = FindObjectOfType<OVRPassthroughLayer>();
                    if (found) passthroughLayer = found;
                    else       passthroughLayer = new GameObject("OVRPassthroughLayer_Auto").AddComponent<OVRPassthroughLayer>();
                }
                // sensible defaults
                passthroughLayer.placement = OVRPassthroughLayer.Placement.Overlay;
                passthroughLayer.edgeRenderingEnabled = false;
            }

            passthroughLayer.hidden = false;
            if (OVRManager.instance) OVRManager.instance.isInsightPassthroughEnabled = true;
        }
        else
        {
            if (passthroughLayer) passthroughLayer.hidden = true;
            if (OVRManager.instance) OVRManager.instance.isInsightPassthroughEnabled = false;
        }
#else
        // Editor/PC: nothing to do (safe no-op)
#endif
    }
}

