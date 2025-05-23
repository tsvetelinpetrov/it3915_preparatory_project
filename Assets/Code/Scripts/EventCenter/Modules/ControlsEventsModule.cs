public class ControlsEventsModule
{
    // Defining the delegates for the events
    public delegate void GenericControlsEvent();
    public delegate void ControlsChangedEvent(Controls controls);
    public delegate void ControlsRefreshingStatusChangedEvent(bool refreshing);

    // Defining the events
    public event GenericControlsEvent OnTurnOnLights;
    public event GenericControlsEvent OnTurnOffLights;
    public event GenericControlsEvent OnTurnOnUpperFan;
    public event GenericControlsEvent OnTurnOffUpperFan;
    public event GenericControlsEvent OnTurnOnLowerFan;
    public event GenericControlsEvent OnTurnOffLowerFan;
    public event GenericControlsEvent OnValveOpen;
    public event GenericControlsEvent OnValveClose;
    public event ControlsChangedEvent OnControlsChanged;
    public event ControlsRefreshingStatusChangedEvent OnControlsRefreshingStatusChange;

    /// <summary>
    /// Invoking this event will turn on the lights in the scene. It will also change the lights status in GlobalSettings to true.
    /// </summary>
    public void TurnOnLights()
    {
        OnTurnOnLights?.Invoke();
    }

    /// <summary>
    /// Invoking this event will turn off the lights in the scene. It will also change the lights status in GlobalSettings to false.
    /// </summary>
    public void TurnOffLights()
    {
        OnTurnOffLights?.Invoke();
    }

    public void TurnOnUpperFan()
    {
        OnTurnOnUpperFan?.Invoke();
    }

    public void TurnOffUpperFan()
    {
        OnTurnOffUpperFan?.Invoke();
    }

    public void TurnOnLowerFan()
    {
        OnTurnOnLowerFan?.Invoke();
    }

    public void TurnOffLowerFan()
    {
        OnTurnOffLowerFan?.Invoke();
    }

    public void OpenValve()
    {
        OnValveOpen?.Invoke();
    }

    public void CloseValve()
    {
        OnValveClose?.Invoke();
    }

    public void ChangeControls(Controls controls)
    {
        OnControlsChanged?.Invoke(controls);
    }

    public void ChangeRefreshingStatus(bool refreshing)
    {
        OnControlsRefreshingStatusChange?.Invoke(refreshing);
    }
}
