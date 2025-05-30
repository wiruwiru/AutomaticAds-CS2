using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;

namespace AutomaticAds.Managers;

public class TimerManager
{
    private readonly List<CounterStrikeSharp.API.Modules.Timers.Timer> _timers = new();
    private CounterStrikeSharp.API.Modules.Timers.Timer? _adTimer;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _specAdTimer;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _onDeadAdTimer;
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

    public void SetSpecAdTimer(CounterStrikeSharp.API.Modules.Timers.Timer timer)
    {
        _specAdTimer?.Kill();
        _specAdTimer = timer;
    }

    public void SetOnDeadAdTimer(CounterStrikeSharp.API.Modules.Timers.Timer timer)
    {
        _onDeadAdTimer?.Kill();
        _onDeadAdTimer = timer;
    }

    public void KillAdTimer()
    {
        _adTimer?.Kill();
        _adTimer = null;
    }

    public void KillSpecAdTimer()
    {
        _specAdTimer?.Kill();
        _specAdTimer = null;
    }

    public void KillOnDeadAdTimer()
    {
        _onDeadAdTimer?.Kill();
        _onDeadAdTimer = null;
    }

    public void KillAllTimers()
    {
        _adTimer?.Kill();
        _adTimer = null;

        _specAdTimer?.Kill();
        _specAdTimer = null;

        _onDeadAdTimer?.Kill();
        _onDeadAdTimer = null;

        foreach (var timer in _timers)
        {
            timer?.Kill();
        }
        _timers.Clear();
    }

    public int ActiveTimersCount => _timers.Count(t => t != null);
}