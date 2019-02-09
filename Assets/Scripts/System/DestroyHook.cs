using UnityEngine;

public class DestroyHook : MonoBehaviour
{
    public delegate void DestroyHandler();

    public event DestroyHandler OnDestroyed;

    private void OnDestroy()
    {
        if (OnDestroyed != null)
        {
            OnDestroyed();
        }
    }
}
