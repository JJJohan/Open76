﻿using Assets.Scripts.System;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public class Building : WorldEntity
    {
        private int _health;
        private Sdf _sdf;
        private GameObject _wreckedObject;

        public override bool Alive
        {
            get { return _health > 0; }
        }

        public override int Health
        {
            get { return _health; }
        }

        public Building(GameObject gameObject) : base(gameObject)
        {
        }

        public void Initialise( Sdf sdf, GameObject wreckedObject)
        {
            _sdf = sdf;
            _health = (int)sdf.Health;
            _wreckedObject = wreckedObject;
        }

        public override void ApplyDamage(DamageType damageType, Vector3 hitNormal, int damage, Car attacker)
        {
            bool alive = Alive;
            _health -= damage;

            if (alive && _health <= 0)
            {
                if (_sdf.Xdf != null)
                {
                    // TODO: Perform effect.
                }

                if (!string.IsNullOrEmpty(_sdf.DestroySoundName))
                {
                    AudioSource source = CacheManager.Instance.GetAudioSource(GameObject, _sdf.DestroySoundName);
                    if (source != null)
                    {
                        source.Play();
                    }
                }

                if (_wreckedObject != null)
                {
                    foreach (Transform child in Transform)
                    {
                        child.gameObject.SetActive(false);
                    }

                    _wreckedObject.SetActive(true);
                }
            }
        }
    }
}
