using HyperPuzzleEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HyperPuzzleEngine
{
    public class RotatingBlade : MonoBehaviour
{
    public bool spawnParticleOnDestroy = true;
    public bool increaseCollectedPiecesCounterOnDestroy = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("MovableCube"))
        {
            if (spawnParticleOnDestroy)
            {
                GameObject spawnedParticle = Instantiate(Resources.Load("MovableCubeDestroyParticle") as GameObject, transform);
                spawnedParticle.transform.parent = null;
                spawnedParticle.transform.position = other.transform.position;
                spawnedParticle.GetComponent<ParticleSystemRenderer>().material = other.GetComponent<MeshRenderer>().material;
                Destroy(spawnedParticle, 2f);
            }

            if (increaseCollectedPiecesCounterOnDestroy)
                GetComponentInParent<CollectedStacksCounter>().IncreaseCollectedPieces(1, other.gameObject.GetInstanceID());

            GetComponentInParent<SoundsManagerForTemplate>().PlaySound_Block_DestroyedByObstacle();

            ReduceLockedBlockCounter();

            Destroy(other.gameObject);
        }
    }

    private void ReduceLockedBlockCounter()
    {
        foreach (LockBlock lockedBlock in GetComponentInParent<ShowcaseParent>().GetComponentsInChildren<LockBlock>())
            lockedBlock.ReduceLockedCounter();
    }
}
}