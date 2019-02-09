using Assets.Scripts;
using UnityEngine;

public class AnimatedTexture : IFixedUpdateable
{
    private const float FrameRate = 1f / 25f; // Haven't found a consistent value in XDF that resembles framerate.

    private Material[][] _materialGroups;
    private int[] _frameIndices;
    private float _currentTime;
    private Material[] _activeMaterials;
    private MeshRenderer _meshRenderer;

    public static AnimatedTexture AnimateObject(GameObject gameObject, Material[][] materialGroups, MeshRenderer meshRenderer)
    {
        return new AnimatedTexture(gameObject, materialGroups, meshRenderer);
    }

    private AnimatedTexture(GameObject gameObject, Material[][] materialGroups, MeshRenderer meshRenderer)
    {
        _materialGroups = materialGroups;
        _frameIndices = new int[_materialGroups.Length];
        _activeMaterials = new Material[_materialGroups.Length];
        _meshRenderer = meshRenderer;
        UpdateManager.Instance.AddFixedUpdateable(this);

        // Little hook helper so we don't have to worry about cleaning up.
        DestroyHook destroyHook = gameObject.AddComponent<DestroyHook>();
        destroyHook.OnDestroyed += Destroy;
    }

    private void Destroy()
    {
        UpdateManager.Instance.RemoveFixedUpdateable(this);
    }

    public void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        _currentTime += dt;

        if (_currentTime < FrameRate)
        {
            return;
        }

        for (int i = 0; i < _frameIndices.Length; ++i)
        {
            _frameIndices[i] = (_frameIndices[i] + 1) % _materialGroups[i].Length;
            _activeMaterials[i] = _materialGroups[i][_frameIndices[i]];
        }

        _currentTime -= FrameRate;
        _meshRenderer.materials = _activeMaterials;
    }
}
