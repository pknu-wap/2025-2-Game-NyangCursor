using UnityEngine;

public class WaterBeamTextureScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 2f;
    private LineRenderer lr;
    private float offset = 0f;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (lr == null || lr.material == null)
            return;

        offset -= Time.deltaTime * scrollSpeed;
        if (offset < -1f)
            offset += 1f;

        lr.material.mainTextureOffset = new Vector2(offset, 0f);
    }
}
