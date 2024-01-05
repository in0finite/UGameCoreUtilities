namespace UGameCore.Utilities
{
    /// <summary>
    /// Provides game time, including simulation time and real time (unaffected by simulation).
    /// Simulation time can differ from real time, for example, when using scaled time for slow-motion
    /// effects, or when replaying a match and then seeking through timeline.
    /// </summary>
    public interface IGameTimeProvider
    {
        double Time { get; }

        float DeltaTime { get; }

        double RealTime { get; }

        float RealDeltaTime { get; }
    }

    /// <summary>
    /// Provides game time using <see cref="UnityEngine.Time"/> class.
    /// </summary>
    public class UnityGameTimeProvider : IGameTimeProvider
    {
        public double Time => UnityEngine.Time.timeAsDouble;

        public float DeltaTime => UnityEngine.Time.deltaTime;

        public double RealTime => UnityEngine.Time.unscaledTimeAsDouble;

        public float RealDeltaTime => UnityEngine.Time.unscaledDeltaTime;
    }
}
