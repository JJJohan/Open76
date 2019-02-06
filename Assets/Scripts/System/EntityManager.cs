using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Assets.Scripts.Entities;
using UnityEngine;

namespace Assets.Scripts.System
{
    public class EntityManager
    {
        public delegate T GameObjectConstructor<T>(GameObject gameObject);

        private static EntityManager _instance;
        public static EntityManager Instance
        {
            get { return _instance ?? (_instance = new EntityManager()); }
        }

        public List<WorldEntity> Entities { get; }

        private readonly Dictionary<int, WorldEntity> _entityLookup;
        private readonly Dictionary<GameObject, WorldEntity> _gameObjectLookup;

        private EntityManager()
        {
            Entities = new List<WorldEntity>();
            _entityLookup = new Dictionary<int, WorldEntity>();
            _gameObjectLookup = new Dictionary<GameObject, WorldEntity>();
        }
        
        public T CreateEntity<T>(GameObject gameObject) where T : WorldEntity
        {
            Type entityType = typeof(T);

            ConstructorInfo constructorInfo = entityType.GetConstructor(new [] {typeof(GameObject)});
            ParameterExpression paramExpr = Expression.Parameter(typeof(GameObject));
            NewExpression body = Expression.New(constructorInfo, paramExpr);

            Expression<GameObjectConstructor<T>> constructor = Expression.Lambda<GameObjectConstructor<T>>(body, paramExpr);
            GameObjectConstructor<T> constructorDelegate = constructor.Compile();
            T entity = constructorDelegate(gameObject);

            Entities.Add(entity);
            _gameObjectLookup.Add(gameObject, entity);
            return entity;
        }

        public void Destroy()
        {
            for (int i = Entities.Count - 1; i >= 0; --i)
            {
                Entities[i].Destroy();
            }

            _gameObjectLookup.Clear();
            _entityLookup.Clear();
            Entities.Clear();
            _instance = null;
        }

        public void RegisterId(WorldEntity entity)
        {
            _entityLookup.Add(entity.Id, entity);
        }

        public void RemoveEntity(WorldEntity entity)
        {
            if (_entityLookup.ContainsKey(entity.Id))
            {
                _entityLookup.Remove(entity.Id);
            }

            _gameObjectLookup.Remove(entity.GameObject);
            Entities.Remove(entity);
            entity.Destroy();
        }

        public WorldEntity GetEntity(int id)
        {
            if (!_entityLookup.TryGetValue(id, out WorldEntity entity))
            {
                return null;
            }

            return entity;
        }

        public WorldEntity GetEntity(GameObject gameObject)
        {
            if (!_gameObjectLookup.TryGetValue(gameObject, out WorldEntity entity))
            {
                return null;
            }

            return entity;
        }
    }
}
