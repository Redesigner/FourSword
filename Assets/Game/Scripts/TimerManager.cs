using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct TimerHandle
{
    private readonly WeakReference<TimerEntry> _timer;

    public TimerHandle(TimerEntry timer)
    {
        _timer = new WeakReference<TimerEntry>(timer);
    }

    public bool IsActive()
    {
        if (_timer == null)
        {
            return false;
        }

        if (_timer.TryGetTarget(out var timer))
        {
            return timer.duration >= timer.currentTime;
        }

        return false;
    }

    public void Pause()
    {
        if (_timer == null)
        {
            return;
        }
        
        if (_timer.TryGetTarget(out var timer))
        {
            timer.paused = true;
        }
    }

    public void Unpause()
    {
        if (_timer.TryGetTarget(out var timer))
        {
            timer.paused = false;
        }
    }

    public void Reset()
    {
        if (_timer.TryGetTarget(out var timer))
        {
            timer.currentTime = 0.0f;
            timer.paused = false;
        }
    }
}

public class TimerEntry
{
    public readonly WeakReference<MonoBehaviour> owner;
    public readonly float duration;
    public float currentTime;
    public readonly Action callback;
    public bool paused;

    public TimerEntry(WeakReference<MonoBehaviour> owner, float duration, Action callback)
    {
        this.owner = owner;
        this.duration = duration;
        this.callback = callback;

        currentTime = 0.0f;
        paused = false;
    }
}

public class TimerManager : MonoBehaviour
{
    private static TimerManager _instance;

    // Lazily initialize our timermanager singleton
    public static TimerManager instance
    {
        get
        {
            if (_instance)
            {
                return _instance;
            }
            
            var container = new GameObject("TimerManager");
            
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            // Only called once per game
            _instance = container.AddComponent<TimerManager>();
            return _instance;
        }
    }
    
    private readonly List<TimerEntry> _timers = new();

    public void Update()
    {
        for(var i = _timers.Count - 1; i >= 0; --i)
        {
            var timer = _timers[i];
            if (timer.paused)
            {
                continue;
            }
            
            timer.currentTime += Time.deltaTime;

            if (!(timer.currentTime >= timer.duration))
            {
                continue;
            }
            
            if (timer.owner.TryGetTarget(out var owner))
            {
                // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
                // This invocation is only once per timer
                timer.callback.Invoke();
            }
            _timers.RemoveAt(i);
        }
    }

    public void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /**
     * <summary>Create a new timer</summary>
     * <param name="owner">Owning scipt of this timer. The timer's lifetime is tied to this object</param>
     * <param name="duration">Time, in seconds, before the timer is called</param>
     * <param name="callback">Function to be called when timer is over</param>
     */
    public TimerHandle CreateTimer(MonoBehaviour owner, float duration, Action callback)
    {
        var newTimer = new TimerEntry(new WeakReference<MonoBehaviour>(owner), duration, callback);
        _timers.Add(newTimer);
        return new TimerHandle(newTimer);
    }

    public void CreateOrResetTimer(ref TimerHandle handle, MonoBehaviour owner, float duration, Action callback)
    {
        if (handle.IsActive())
        {
            handle.Reset();
            return;
        }

        handle = CreateTimer(owner, duration, callback);
    }
}
