﻿using System.Collections;
using Assets.Scripts.Camera;
using Assets.Scripts.CarSystems;
using Assets.Scripts.Entities;
using Assets.Scripts.System;
using Assets.Scripts.System.Fileparsers;
using UnityEngine;

namespace Assets.Scripts
{
    public class SceneRoot : MonoBehaviour
    {
        public string GamePath;
        public string MissionFile;
        public string VcfToLoad;

        private Game _game;
        private AudioSource _audioSource1;
        private AudioSource _audioSource2;
        private AudioSource _lastAudioSource;

        public static SceneRoot Instance { get; private set; }

        private IEnumerator Start()
        {
            Instance = this;
            _game = Game.Instance;

            _audioSource1 = gameObject.AddComponent<AudioSource>();
            _audioSource1.spatialize = false;
            _audioSource1.volume = 0.5f;
            
            _audioSource2 = gameObject.AddComponent<AudioSource>();
            _audioSource2.spatialize = false;
            _audioSource2.volume = 0.5f;

#if UNITY_EDITOR
            gameObject.AddComponent<SceneViewAudioHelper>();
#endif

            if (!string.IsNullOrEmpty(_game.LevelName))
            {
                MissionFile = _game.LevelName;
            }

            if (string.IsNullOrEmpty(_game.GamePath))
            {
                _game.GamePath = GamePath;
            }

            yield return LevelLoader.Instance.LoadLevel(MissionFile);
            
            if (MissionFile.ToLower().StartsWith("m"))
            {
                CacheManager cacheManager = CacheManager.Instance;
                GameObject importedVcf = cacheManager.ImportVcf(VcfToLoad, true, out Vdf unused);
                importedVcf.AddComponent<CarInput>();
                importedVcf.AddComponent<Car>();

                GameObject spawnPoint = GameObject.FindGameObjectsWithTag("Spawn")[0];
                importedVcf.transform.position = spawnPoint.transform.position;
                importedVcf.transform.rotation = spawnPoint.transform.rotation;

                CameraManager.Instance.MainCamera.GetComponent<SmoothFollow>().Target = importedVcf.transform;
            }
        }

        public void PlayUiSound(string soundName)
        {
            AudioClip audioClip = CacheManager.Instance.GetAudioClip(soundName);
            if (audioClip == null)
            {
                return;
            }

            AudioSource targetSource = (_lastAudioSource == _audioSource1) ? _audioSource2 : _audioSource1;
            _lastAudioSource = targetSource;
            targetSource.clip = audioClip;
            targetSource.Play();
        }

        private void OnDestroy()
        {
            Instance = null;
        }
    }
}