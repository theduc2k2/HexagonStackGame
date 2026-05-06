using UnityEngine;

public class Hexagon : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private new Renderer renderer;
    [SerializeField] private new Collider collider;

    private static MaterialPropertyBlock propBlock;
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    private readonly HexagonAnimationPlayer animationPlayer = new HexagonAnimationPlayer();
    private Color currentColor;

    public HexStack HexStack { get; private set; }

    public Color color
    {
        get => currentColor;
        set
        {
            currentColor = value;
            ApplyColor(value);
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

    public void Configure(HexStack hexStack)
    {
        HexStack = hexStack;
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    public void PrepareForReuse()
    {
        EnsureReferences();
        transform.localScale = Vector3.one;

        if (collider != null)
            collider.enabled = true;
    }

    public void DisableCollider()
    {
        EnsureReferences();
        if (collider != null)
            collider.enabled = false;
    }

    public void Vanish(float delay)
    {
        animationPlayer.Vanish(this, delay, () => HexagonPool.Instance.ReturnHexagon(this));
    }

    public void VanishToScore(float delay, Vector3 scoreWorldPos)
    {
        animationPlayer.VanishToScore(this, delay, scoreWorldPos, () => HexagonPool.Instance.ReturnHexagon(this));
    }

    public void MoveToLocal(Vector3 targetLocalPos, System.Action onComplete = null, float delay = 0f, float duration = 0.34f)
    {
        animationPlayer.MoveToLocal(this, targetLocalPos, onComplete, delay, duration);
    }

    private void ApplyColor(Color colorValue)
    {
        EnsureReferences();
        if (renderer == null)
            return;

        propBlock ??= new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(ColorId, colorValue);
        propBlock.SetColor(BaseColorId, colorValue);
        renderer.SetPropertyBlock(propBlock);
    }

    private void EnsureReferences()
    {
        if (renderer == null)
            renderer = GetComponentInChildren<Renderer>(true);

        if (collider == null)
            collider = GetComponentInChildren<Collider>(true);
    }
}
