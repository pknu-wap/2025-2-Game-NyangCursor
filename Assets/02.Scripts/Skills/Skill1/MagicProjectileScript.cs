using UnityEngine;
using System.Collections;

namespace MagicArsenal
{
    public class MagicProjectileScript : MonoBehaviour
    {
        public GameObject impactParticle;
        public GameObject projectileParticle;
        public GameObject muzzleParticle;
        public GameObject[] trailParticles;

        private Transform myTransform;

        void Start()
        {
            myTransform = transform;

            // Projectile Particle 생성
            if (projectileParticle)
            {
                projectileParticle = Instantiate(projectileParticle, myTransform.position, myTransform.rotation, myTransform);
            }

            // Muzzle Particle 생성
            if (muzzleParticle)
            {
                muzzleParticle = Instantiate(muzzleParticle, myTransform.position, myTransform.rotation, myTransform);
                Destroy(muzzleParticle, 1.5f);
            }

            // Trail 분리 및 유지 (원한다면 여기서 추가 로직 가능)
            foreach (GameObject trail in trailParticles)
            {
                if (trail != null)
                {
                    Transform curTrail = myTransform.Find(projectileParticle.name + "/" + trail.name);
                    if (curTrail != null)
                    {
                        curTrail.parent = null;
                        Destroy(curTrail.gameObject, 3f);
                    }
                }
            }
        }
    }
}
