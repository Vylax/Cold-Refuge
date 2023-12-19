using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
    public GameObject particlePrefab;
    public int poolSize = 100;

    private Queue<GameObject> particleQueue = new Queue<GameObject>();

    private ParticleSystem particleSystem;

    public Material lowResMaterial;
    public float normalDirection;
    public ParticleSystem.MinMaxCurve particleLifetime;
    public int maxParticles;


    void Start()
    {
        particleSystem = this.GetComponent<ParticleSystem>();

        ParticleSystem.MainModule mainModule = particleSystem.main;
        //mainModule.maxParticles = maxParticles; // Adjust according to performance
        //mainModule.startLifetime = particleLifetime;

        ParticleSystemRenderer rendererModule = particleSystem.GetComponent<ParticleSystemRenderer>();
        //rendererModule.renderMode = ParticleSystemRenderMode.Billboard; // Use Quad
        //rendererModule.normalDirection = normalDirection; // Adjust based on your camera setup
        //rendererModule.material = lowResMaterial; // Use a low-res material

        ParticleSystem.CollisionModule collisionModule = particleSystem.collision;
        //collisionModule.enabled = false; // Disable if not needed


        //InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject particle = Instantiate(particlePrefab, Vector3.zero, Quaternion.identity);
            particle.SetActive(false);
            particleQueue.Enqueue(particle);
        }
    }

    public GameObject GetPooledParticle(Vector3 position)
    {
        if (particleQueue.Count == 0)
        {
            // Optionally, dynamically expand the pool if needed
            return null;
        }

        GameObject particle = particleQueue.Dequeue();
        particle.transform.position = position;
        particle.SetActive(true);

        return particle;
    }

    public void ReturnToPool(GameObject particle)
    {
        particle.SetActive(false);
        particleQueue.Enqueue(particle);
    }
}
