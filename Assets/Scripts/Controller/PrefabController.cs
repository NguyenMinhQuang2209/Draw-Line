using UnityEngine;

public class PrefabController : Singleton<PrefabController>
{
    [field: SerializeField] public PathPoint pathPoint { get; private set; }
    [field: SerializeField] public LineRenderer lineRender { get; private set; }
    [field: SerializeField] public Transform lineRenderWrap { get; private set; }
}
