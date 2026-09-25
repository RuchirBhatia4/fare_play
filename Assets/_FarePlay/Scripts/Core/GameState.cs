namespace FarePlay
{
    /// <summary>The phases of one run. GameManager owns the current state.</summary>
    public enum GameState
    {
        Overview,   // top-down view of the routes, bus locked, 30 s countdown (Space skips)
        Ready,      // chase camera, bus can drive in the depot, timer NOT running yet
        Driving,    // bus crossed a start line: timer running
        Won,        // reached the finish line in time
        Failed      // timer hit zero
    }
}
