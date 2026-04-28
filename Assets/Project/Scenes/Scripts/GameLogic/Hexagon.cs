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

        // Tính toán hướng di chuyển để xác định trục lật (flip axis)
        Vector3 moveDir = (targetLocalPos - startLocalPos);
        moveDir.y = 0f;
        
        Vector3 flipAxis = moveDir.sqrMagnitude > 0.0001f
            ? Vector3.Cross(moveDir.normalized, Vector3.up).normalized
            : Vector3.right;

        // Lật 180 độ quanh trục vừa tính
        Quaternion flipRotation = Quaternion.AngleAxis(360f, flipAxis) * startRotation;

        // Tính khoảng cách để điều chỉnh độ cao bước nhảy (jump height) hài hòa
        float flatDist = Vector3.Distance(new Vector3(startLocalPos.x, 0, startLocalPos.z), new Vector3(targetLocalPos.x, 0, targetLocalPos.z));
        float jumpHeight = Mathf.Clamp(0.2f + flatDist * 0.25f, 0.2f, 0.6f);

        LeanTween.value(gameObject, 0f, 1f, duration)
            .setEase(LeanTweenType.easeOutQuad) // Tạo cảm giác hạ cánh êm hơn
            .setDelay(delay)
            .setOnUpdate((float t) =>
            {
                // Di chuyển tịnh tiến phẳng
                Vector3 flatPos = Vector3.Lerp(startLocalPos, targetLocalPos, t);
                
                // Hiệu ứng nhảy (Arc) - dùng t * (1-t) để tạo đường cong mượt nhất
                float arc = 4f * jumpHeight * t * (1f - t);
                
                transform.localPosition = new Vector3(flatPos.x, flatPos.y + arc, flatPos.z);
                
                // Xoay lật mượt mà
                transform.localRotation = Quaternion.Slerp(startRotation, flipRotation, t);
            })
            .setOnComplete(() =>
            {
                // Đảm bảo vị trí cuối cùng chính xác tuyệt đối
                transform.localPosition = targetLocalPos;
                // Giữ nguyên rotation lật 180 độ (vì hexagon đối xứng nên nó vẫn đẹp và không bị giật)
                onComplete?.Invoke();
            });
    }

}
