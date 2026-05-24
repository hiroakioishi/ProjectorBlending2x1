using UnityEngine;

[ExecuteAlways]
public class ScaleSetterByAspectRatioRes : MonoBehaviour
{
    [SerializeField]
    Camera _camRef = default;

    public int ResolutionW = 3840;
    public int ResolutionH = 1200;

    void Update()
    {
        ResolutionW = Mathf.Max(ResolutionW, 1);
        ResolutionH = Mathf.Max(ResolutionH, 1);

        if (_camRef != null)
        {
            transform.localScale = new Vector3(
                _camRef.orthographicSize * 2.0f * (ResolutionW / (float)ResolutionH),
                _camRef.orthographicSize * 2.0f,
                1.0f
            );
        }
    }
}
