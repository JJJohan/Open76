using Assets.Scripts.Entities;
using Assets.Scripts.System;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

namespace Assets.Scripts
{
    public class Projectile : MonoBehaviour
    {
        public Car Owner { get; set; }
        public Gdf Gdf { get; set; }

        private const float MaxLifeTime = 10.0f;

        private float _lifeTime;

        private enum ImpactType
        {
            Ground,
            Building,
            Car
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (Gdf.BulletVelocity > 0.0f)
            {
                transform.Translate(Vector3.forward * Gdf.BulletVelocity * dt, Space.Self);
            }

            _lifeTime += dt;
            if (_lifeTime > MaxLifeTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            WorldEntity entity = other.GetComponentInParent<WorldEntity>();
            if (entity != null)
            {
                Transform entityTransform = entity.transform;
                if (entityTransform == Owner.transform)
                {
                    return;
                }

                if (entity is Car)
                {
                    CreateImpactEffect(ImpactType.Car);
                }
                else
                {
                    CreateImpactEffect(ImpactType.Building);
                }

                Vector3 hitNormal = (entityTransform.position - transform.position).normalized;
                entity.ApplyDamage(DamageType.Projectile, hitNormal, Gdf.Damage, Owner);
            }
            else
            {
                CreateImpactEffect(ImpactType.Ground);
            }

            // TODO: Spawn impact sprite.
            Destroy(gameObject);
        }

        private void CreateImpactEffect(ImpactType type)
        {
            string effectName;
            string soundName;

            switch (type)
            {
                case ImpactType.Building:
                    effectName = Gdf.ImpactEffectBuilding;
                    soundName = Gdf.ImpactSoundBuilding;
                    break;
                case ImpactType.Car:
                    effectName = Gdf.ImpactEffectCar;
                    soundName = Gdf.ImpactSoundCar;
                    break;
                default:
                    effectName = Gdf.ImpactEffectGround;
                    soundName = Gdf.ImpactSoundGround;
                    break;
            }

            Effect effect = CacheManager.Instance.ImportXdf(effectName, transform);
            effect.AutoDestroy = true;
            effect.Fire();

            AudioSource audioSource = CacheManager.Instance.GetAudioSource(effect.gameObject, soundName);
            audioSource.Play();
        }
    }
}
