using UnityEngine;

public class ToolManager : MonoBehaviour
{
    public static ToolManager Instance;

    private ITool currentTool;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Activate a tool and deactivate the previous one
    public void ActivateTool(ITool tool)
    {
        if (currentTool != null) currentTool.Deactivate();
        currentTool = tool;
        currentTool.Activate();
    }

    public void DeactivateCurrentTool()
    {
        if (currentTool != null) currentTool.Deactivate();
        currentTool = null;
    }
}
