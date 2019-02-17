using System.Collections.Generic;
using Assets.Scripts.System;
using UnityEngine;

namespace Assets.Scripts.Camera
{
    public class CameraManager
    {
        private readonly Stack<UnityEngine.Camera> _cameraStack;
        private readonly GameObject _mainCameraObject;
        private bool _audioEnabled;
        private UnityEngine.Camera _transitionCamera;
        private float _transitionPercent;

        public UnityEngine.Camera MainCamera
        {
            get { return _mainCameraObject != null ? _mainCameraObject.GetComponent<UnityEngine.Camera>() : null; }
        }

        public UnityEngine.Camera ActiveCamera
        {
            get
            {
                if (_cameraStack.Count > 0)
                {
                    return _cameraStack.Peek();
                }

                return null;
            }
        }

        private static CameraManager _instance;

        public static CameraManager Instance
        {
            get { return _instance ?? (_instance = new CameraManager()); }
        }

        public bool AudioEnabled
        {
            get { return _audioEnabled; }
            set
            {
                if (_audioEnabled == value)
                {
                    return;
                }

                _audioEnabled = value;
                if (_cameraStack.Count > 0)
                {
                    UnityEngine.Camera camera = _cameraStack.Peek();
                    camera.GetComponent<AudioListener>().enabled = value;
                }
                else
                {
                    MainCamera.GetComponent<AudioListener>().enabled = value;
                }
            }
        }

        public bool IsMainCameraActive
        {
            get { return MainCamera == ActiveCamera; }
        }

        private CameraManager()
        {
            _cameraStack = new Stack<UnityEngine.Camera>();
            UnityEngine.Camera mainCamera = Object.FindObjectOfType<UnityEngine.Camera>();
            _mainCameraObject = mainCamera.gameObject;
            _cameraStack.Push(mainCamera);
            _audioEnabled = true;
        }
        
        public void PushCamera()
        {
            if (_cameraStack.Count > 0)
            {
                UnityEngine.Camera camera = _cameraStack.Peek();
                camera.enabled = false;
                camera.GetComponent<AudioListener>().enabled = false;
            }

            GameObject newCameraObject = new GameObject("Stack Camera " + _cameraStack.Count);
            UnityEngine.Camera newCamera = newCameraObject.AddComponent<UnityEngine.Camera>();
            newCameraObject.AddComponent<AudioListener>();
            _cameraStack.Push(newCamera);
        }

        public void Transition(float startHeight, float endHeight, FSMPath path, Transform watchTarget)
        {
            if (ActiveCamera != _transitionCamera)
            {
                _transitionCamera = ActiveCamera;
                _transitionPercent = 0f;
            }
            else
            {
                _transitionPercent += Time.deltaTime * 0.1f; // Hardcoded 10 second transition at the moment.
            }

            Vector3 startPos = path.GetWorldPosition(0);
            Vector3 endPos = path.GetWorldPosition(1);
            Vector3 targetPos = Vector3.Lerp(startPos, endPos, _transitionPercent);
            float targetHeight = Mathf.Lerp(startHeight, endHeight, _transitionPercent) * 0.01f;
            targetPos.y = Utils.GroundHeightAtPoint(targetPos.x, targetPos.z) + targetHeight;

            Transform camTransform = _transitionCamera.transform;
            camTransform.position = targetPos;
            camTransform.LookAt(watchTarget, Vector3.up);
        }

        public bool CamArrived()
        {
            return _transitionPercent >= 1.0f - float.Epsilon;
        }

        public void PopCamera()
        {
            if (_cameraStack.Count == 0)
            {
                return;
            }

            UnityEngine.Camera stackCamera = _cameraStack.Pop();
            Object.Destroy(stackCamera.gameObject);
            
            UnityEngine.Camera camera = _cameraStack.Peek();
            camera.enabled = true;
            camera.GetComponent<AudioListener>().enabled = _audioEnabled;
        }

        public void Destroy()
        {
            while (_cameraStack.Count > 0)
            {
                UnityEngine.Camera stackCamera = _cameraStack.Pop();
                if (stackCamera != null)
                {
                    Object.Destroy(stackCamera.gameObject);
                }
            }

            _instance = null;
        }
    }
}
