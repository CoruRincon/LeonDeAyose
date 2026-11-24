using UnityEngine;
using UnityEngine.Events;
using Vuforia;

[RequireComponent(typeof(ObserverBehaviour))]
public class ScanTrigger : MonoBehaviour
{
    public UnityEvent onFound;
    public bool once = true;

    ObserverBehaviour observer;
    bool fired;

    void Awake()
    {
        observer = GetComponent<ObserverBehaviour>();
        observer.OnTargetStatusChanged += OnStatus;
    }

    void OnDestroy()
    {
        if (observer) observer.OnTargetStatusChanged -= OnStatus;
    }

    void OnStatus(ObserverBehaviour b, TargetStatus s)
    {
        if (s.Status != Status.TRACKED) return;   // ignore extended/limited
        if (once && fired) return;
        fired = true;
        onFound.Invoke();
    }
}