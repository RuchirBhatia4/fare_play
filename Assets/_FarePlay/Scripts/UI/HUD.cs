using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FarePlay
{
    /// <summary>
    /// LANE B. Screen-space canvas on the GameSystems prefab. Reads the other systems every frame.
    /// Issues: "HUD v1: timer, score, passengers" and "HUD: always-visible signal indicator"
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] RaceTimer timer;
        [SerializeField] ScoreManager score;

        [Header("Texts")]
        [SerializeField] TMP_Text timerText;
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text passengersText;
        [SerializeField] TMP_Text routeText;
        [SerializeField] TMP_Text overviewText;

        [Header("Signal indicator")]
        [Tooltip("A circle Image tinted green / yellow / red.")]
        [SerializeField] Image signalLamp;
        [Tooltip("For example \"RED 6s\".")]
        [SerializeField] TMP_Text signalText;

        [Header("Fuel (stretch)")]
        [Tooltip("Image with Image Type = Filled. fillAmount 0..1.")]
        [SerializeField] Image fuelFill;

        void Update()
        {
            // TODO timer: timerText.text = timer.Formatted(); turn it red under 15 s.
            // TODO score and passengers: score.LiveScore and score.PassengersOnBoard.
            // TODO overview: while GameManager.Instance.State == GameState.Overview, show
            //      "Driving starts in 23s (Space to skip)" plus the controls. Hide it otherwise.
            // TODO signal: Route route = GameManager.Instance.ActiveRoute;
            //      TrafficLight next = route != null ? route.NextLight : null;
            //      null -> "No signals ahead"; otherwise tint signalLamp by next.State and show
            //      Mathf.CeilToInt(next.SecondsUntilChange). Keep it visible the whole run.
        }
    }
}
