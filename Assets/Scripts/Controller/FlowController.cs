using UnityEngine;

public class FlowController : Singleton<FlowController>
{
    protected override void Awake()
    {
        base.Awake();
        FirstInit();
    }
    private void FirstInit()
    {
        int currentMap = GlobalManager.Instance.GetCurrentMap();
        GenerateMapController.Instance.GenerateMap(currentMap);
        DrawLineController.Instance.GenerateNodeList();
    }
}
