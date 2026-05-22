using System.Collections;
using UnityEngine;

public class ScoreFillFxController : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] particleTemplates;
    [SerializeField] private bool autoCollectFromChildren = true;
    [SerializeField] private float destroyPadding = 0.15f;
    [SerializeField] private Vector3 localSpawnOffset = Vector3.zero;
    [SerializeField] private bool useTemplateLocalRotation = true;
    private bool warnedMissingTemplates;

    public bool HasTemplates
    {
        get
        {
            CollectTemplatesIfNeeded();
            return particleTemplates != null && particleTemplates.Length > 0;
        }
    }

    private void Awake()
    {
        CollectTemplatesIfNeeded();
    }

    private void OnValidate()
    {
        CollectTemplatesIfNeeded();
    }

    public void PlayBurst()
    {
        CollectTemplatesIfNeeded();
        if (particleTemplates == null || particleTemplates.Length == 0)
        {
            if (!warnedMissingTemplates)
            {
                Debug.LogWarning("ScoreFillFxController has no particle templates to spawn.");
                warnedMissingTemplates = true;
            }
            return;
        }

        for (int i = 0; i < particleTemplates.Length; i++)
        {
            ParticleSystem template = particleTemplates[i];
            if (template == null)
                continue;

            SpawnOneFx(template);
        }
    }

    public void PlayBurstSequence(int count, float interval, float startDelay = 0f)
    {
        if (count <= 0)
            return;

        StartCoroutine(PlaySequenceRoutine(count, interval, startDelay));
    }

    private IEnumerator PlaySequenceRoutine(int count, float interval, float startDelay)
    {
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < count; i++)
        {
            PlayBurst();

            if (i < count - 1 && interval > 0f)
                yield return new WaitForSeconds(interval);
        }
    }

    private void CollectTemplatesIfNeeded()
    {
        if (!autoCollectFromChildren)
            return;

        if (particleTemplates != null && particleTemplates.Length > 0)
            return;

        particleTemplates = GetComponentsInChildren<ParticleSystem>(true);
    }

    public ParticleSystem[] GetParticleTemplates()
    {
        CollectTemplatesIfNeeded();
        return particleTemplates;
    }

    public void SetParticleTemplates(ParticleSystem[] templates)
    {
        particleTemplates = templates;
    }

    private void SpawnOneFx(ParticleSystem template)
    {
        GameObject instance = Instantiate(template.gameObject);
        Transform instanceTransform = instance.transform;
        instanceTransform.SetParent(transform, false);
        instanceTransform.localPosition = localSpawnOffset;
        instanceTransform.localRotation = useTemplateLocalRotation ? template.transform.localRotation : Quaternion.identity;
        instanceTransform.localScale = template.transform.localScale;

        if (!instance.activeSelf)
            instance.SetActive(true);

        ParticleSystem[] systems = instance.GetComponentsInChildren<ParticleSystem>(true);
        float maxLifetime = 0f;
        for (int i = 0; i < systems.Length; i++)
        {
            ParticleSystem system = systems[i];
            if (system == null)
                continue;

            system.Clear(true);
            system.Play(true);
            maxLifetime = Mathf.Max(maxLifetime, EstimateLifetime(system));
        }

        float destroyAfter = maxLifetime + destroyPadding;
        if (destroyAfter <= 0f)
            destroyAfter = 1f;

        Destroy(instance, destroyAfter);
    }

    private static float EstimateLifetime(ParticleSystem system)
    {
        ParticleSystem.MainModule main = system.main;
        float duration = main.duration;
        float startLifetime = GetMaxCurveValue(main.startLifetime);
        return duration + startLifetime;
    }

    private static float GetMaxCurveValue(ParticleSystem.MinMaxCurve curve)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return curve.constant;
            case ParticleSystemCurveMode.TwoConstants:
                return curve.constantMax;
            case ParticleSystemCurveMode.Curve:
                return curve.curve != null ? curve.curve.Evaluate(1f) : 0f;
            case ParticleSystemCurveMode.TwoCurves:
                float minValue = curve.curveMin != null ? curve.curveMin.Evaluate(1f) : 0f;
                float maxValue = curve.curveMax != null ? curve.curveMax.Evaluate(1f) : 0f;
                return Mathf.Max(minValue, maxValue);
            default:
                return 0f;
        }
    }
}
