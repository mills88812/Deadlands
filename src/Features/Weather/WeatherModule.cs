namespace Deadlands;

/// <summary>
/// Global WeatherModule responsible for selecting weather to happen in the Deadlands
/// </summary>
public class WeatherModule
{
    public bool STASIS = true;

    public int cyclesSinceLastRain;
    public float globalSandstormProgress;

    public WeatherModule()
    {
    }

    public void Update()
    {
        if (STASIS)
        {
            return;
        }
    }

    public bool IsActive()
    {
        return !STASIS;
    }
}