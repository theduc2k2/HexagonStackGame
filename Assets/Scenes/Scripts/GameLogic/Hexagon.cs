using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Accessibility;
using DentedPixel;


public class Hexagon : MonoBehaviour
{
    [Header(" Elements")]
    [SerializeField] private new Renderer renderer;
    [SerializeField] private new Collider collider;
    public HexStack HexStack { get; private set; }
    public Color color

    {
        get => renderer.material.color;
        set => renderer.material.color = value;
    }
    public void Configure(HexStack hexStack)
    {
        HexStack = hexStack;
    }
    public void SetParent(Transform parent){
        transform.SetParent(parent);
    }
    public void DisableCollider() => collider.enabled = false;
    public void Vanish(float delay)
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.zero, .2f).setEase(LeanTweenType.easeInBack)
        .setDelay(delay)
        .setOnComplete(()=>Destroy(gameObject));
    }
    public void MoveToLocal(Vector3 targetLocalPos, System.Action onComplete = null)
    {
        LeanTween.cancel(gameObject); // Hủy mọi animation đang chạy trên đối tượng này

        float delay = transform.GetSiblingIndex() * 0.01f;

        // Di chuyển đối tượng
        LeanTween.moveLocal(gameObject, targetLocalPos, 0.5f)
            .setEase(LeanTweenType.easeInOutSine)
            .setDelay(delay)
            .setOnComplete(() =>
            {
                onComplete?.Invoke(); // Gọi lại khi hoàn thành nếu có
            });

        // Xoay đối tượng theo hướng di chuyển
        Vector3 direction = (targetLocalPos - transform.localPosition).normalized;
        direction.y = 0.2f; // Tăng nhẹ trục Y để tạo hiệu ứng nghiêng
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);

        LeanTween.rotateAround(gameObject, rotationAxis, 180f, 0.4f)
            .setDelay(delay);
    }
}
