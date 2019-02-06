﻿using Assets.Scripts.System;
using UnityEngine;

namespace Assets.Scripts.Entities
{
    public abstract class WorldEntity
    {
        private int _id;

        public int Id
        {
            get { return _id;}
            set
            {
                if (_id == value)
                {
                    return;
                }

                _id = value;
                EntityManager.Instance.RegisterId(this);
            }
        }

        public abstract bool Alive { get; }
        public int MaxAttackers { get; set; }
        public Transform Transform { get; private set; }
        public GameObject GameObject { get; private set; }

        public WorldEntity(GameObject gameObject)
        {
            GameObject = gameObject;
            Transform = gameObject.transform;
        }

        public virtual void ApplyDamage(DamageType damageType, Vector3 hitNormal, int damage, Car attacker)
        {
        }

        public virtual void Destroy()
        {
            Object.Destroy(GameObject);
        }
    }
}
