using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;

namespace AutomaticAds.Managers;

public class TimerManager
{
    private readonly List<CounterStrikeSharp.API.Modules.Timers.Timer> _timers = new();
    private CounterStrikeSharp.API.Modules.Timers.Timer? _adTimer;
    private readonly BasePlugin _plugin;

    public TimerManager(BasePlugin plugin)
    {
        _plugin = plugin;
    }

    public CounterStrikeSharp.API.Modules.Timers.Timer AddTimer(float interval, Action callback, TimerFlags flags = TimerFlags.STOP_ON_MAPCHANGE)
    {
        var timer = _plugin.AddTimer(interval, callback, flags);
        _timers.Add(timer);
        return timer;
    }

    public void SetAdTimer(CounterStrikeSharp.API.Modules.Timers.Timer timer)
    {
        _adTimer?.Kill();
        _adTimer = timer;
    }

    public void KillAdTimer()
    {
        _adTimer?.Kill();
        _adTimer = null;
    }

    public void KillAllTimers()
    {
        _adTimer?.Kill();
        _adTimer = null;

        foreach (var timer in _timers)
        {
            timer.Kill();
        }
        _timers.Clear();
    }

    public int ActiveTimersCount => _timers.Count(t => t != null);
}