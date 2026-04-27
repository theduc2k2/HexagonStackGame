using UnityEngine;

public class Hexagon : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private new Renderer renderer;
    [SerializeField] private new Collider collider;

    public HexStack HexStack { get; private set; }

    public Color color
    {
        get
        {
            EnsureReferences();
            return renderer != null ? renderer.material.color : Color.white;
        }
        set
        {
            EnsureReferences();
            if (renderer == null)
                return;
            renderer.material.color = value;
        }
    }

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnValidate()
    {
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (renderer == null)
            renderer = GetComponentInChildren<Renderer>(true);

        if (collider == null)
            collider = GetComponentInChildren<Collider>(true);
    }

    public void Configure(HexStack hexStack)
    {
        HexStack = hexStack;
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    public void DisableCollider()
    {
        EnsureReferences();
        if (collider != null)
            collider.enabled = false;
    }

    public void Vanish(float delay)
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.zero, 0.2f)
            .setEase(LeanTweenType.easeInBack)
            .setDelay(delay)
            .setOnComplete(() => Destroy(gameObject));
    }

    public void MoveToLocal(Vector3 targetLocalPos, System.Action onComplete = null, float delay = 0f, float duration = 0.34f)
    {
        LeanTween.cancel(gameObject);
        Vector3 startLocalPos = transform.localPosition;
        Quaternion startRotation = transform.localRotation;

        Vector3 moveDir = (targetLocalPos - startLocalPos);
        moveDir.y = 0f;
        Vector3 flipAxis = moveDir.sqrMagnitude > 0.0001f
            ? Vector3.Cross(moveDir.normalized, Vector3.up).normalized
            : Vector3.right;

        Quaternion flipRotation = Quaternion.AngleAxis(180f, flipAxis) * startRotation;

        float distance = Vector3.Distance(startLocalPos, targetLocalPos);
        float jumpHeight = Mathf.Clamp(0.18f + distance * 0.3f, 0.18f, 0.55f);

        LeanTween.value(gameObject, 0f, 1f, duration)
            .setEase(LeanTweenType.easeOutCubic)
            .setDelay(delay)
            .setOnUpdate((float t) =>
            {
                Vector3 flat = Vector3.Lerp(startLocalPos, targetLocalPos, t);
                float arc = Mathf.Sin(t * Mathf.PI) * jumpHeight;
                transform.localPosition = flat + Vector3.up * arc;
                transform.localRotation = Quaternion.Slerp(startRotation, flipRotation, t);
            })
            .setOnComplete(() =>
            {
                transform.localPosition = targetLocalPos;
                transform.localRotation = startRotation;
                onComplete?.Invoke();
            });
    }
}
