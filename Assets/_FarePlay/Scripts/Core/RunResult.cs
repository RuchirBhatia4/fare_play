namespace FarePlay
{
    /// <summary>Everything the results screen needs. Built by ScoreManager when a run ends.</summary>
    public struct RunResult
    {
        public bool Won;
        public string Reason;              // "Route complete!" or "Out of time"
        public string RouteName;

        public int PassengersDelivered;
        public int PassengerPoints;        // positive

        public int RedLightViolations;
        public int RedLightPoints;         // negative

        public int Collisions;
        public int CollisionPoints;        // negative

        public float SecondsRemaining;
        public int TimeBonus;              // positive, only when Won

        public int Total;                  // never below 0; 0 when failed
    }
}
