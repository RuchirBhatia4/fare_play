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
        public float TimeBonus;            // positive, only when Won; exact to the millisecond (9.876 s x 10 = 98.76)

        public float Total;                // never below 0; 0 when failed; has decimals from the time bonus
    }
}
