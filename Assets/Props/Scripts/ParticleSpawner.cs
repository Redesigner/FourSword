using UnityEngine;

namespace Props.Scripts
{
    public class ParticleSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject particlePrefab;
        [SerializeField] [Min(0.0f)] private float particleLifetime = 1.0f;

        public void Spawn()
        {
            if (!particlePrefab)
            {
                return;
            }
            
            var particleObject = Instantiate(particlePrefab);
            particleObject.transform.position = transform.position;
            TimerManager.instance.CreateTimer(particleObject, particleLifetime, () =>
            {
                Destroy(particleObject);
            });
        }
    }
}