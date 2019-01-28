using Assets.Scripts.Entities;
using Assets.Scripts.System;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

namespace Assets.Scripts
{
    public class Projectile : MonoBehaviour
    {
        private const float MaxLifeTime = 10.0f;

        private float _lifeTime;
        private Car _owner;
        private Gdf _gdf;
        private Transform _transform;
        private Vector3 _lastPos;

        private enum ImpactType
        {
            Ground,
            Building,
            Car
        }

        public void Initialise(Car owner, Gdf gdf)
        {
            _owner = owner;
            _gdf = gdf;
            gameObject.SetActive(true);
            _transform = transform;
            _lastPos = _transform.position;
        }
        
        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;

            _lifeTime += dt;
            if (_lifeTime > MaxLifeTime)
            {
                Destroy(gameObject);
            }

            _transform.Translate(Vector3.forward * _gdf.BulletVelocity * dt, Space.Self);

            Vector3 currentPos = _transform.position;
            float dist = Vector3.Distance(currentPos, _lastPos);
            if (dist > float.Epsilon)
            {
                if (Physics.Linecast(_lastPos, currentPos, out RaycastHit hitInfo))
                {
                    CollisionCheck(ref hitInfo);
                }
            }

            _lastPos = currentPos;
        }

        private void CollisionCheck(ref RaycastHit hitInfo)
        {
            Transform other = hitInfo.transform;
            WorldEntity entity = other.GetComponentInParent<WorldEntity>();
            
            if (entity != null)
            {
                Transform entityTransform = entity.transform;
                if (entityTransform == _owner.transform)
                {
                    return;
                }

                if (entity is Car)
                {
                    CreateImpactEffect(ImpactType.Car, hitInfo.point);
                }
                else
                {
                    CreateImpactEffect(ImpactType.Building, hitInfo.point);
                }

                entity.ApplyDamage(DamageType.Projectile, hitInfo.normal, _gdf.Damage, _owner);
            }
            else
            {
                CreateImpactEffect(ImpactType.Ground, hitInfo.point);
            }

            Destroy(gameObject);
        }

        private void CreateImpactEffect(ImpactType type, Vector3 point)
        {
            string effectName;
            string soundName;

            switch (type)
            {
                case ImpactType.Building:
                    effectName = _gdf.ImpactEffectBuilding;
                    soundName = _gdf.ImpactSoundBuilding;
                    break;
                case ImpactType.Car:
                    effectName = _gdf.ImpactEffectCar;
                    soundName = _gdf.ImpactSoundCar;
                    break;
                default:
                    effectName = _gdf.ImpactEffectGround;
                    soundName = _gdf.ImpactSoundGround;
                    break;
            }

            if (effectName == null)
            {
                return;
            }

            Effect effect = CacheManager.Instance.ImportXdf(effectName, null);
            if (effect == null)
            {
                return;
            }

            effect.transform.position = point;
            effect.AutoDestroy = true;
            effect.Fire();

            if (soundName == null)
            {
                return;
            }

            AudioSource audioSource = CacheManager.Instance.GetAudioSource(effect.gameObject, soundName);
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }
}