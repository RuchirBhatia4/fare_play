using TMPro;
using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// LANE B. The end-of-run panel with the score breakdown and a Restart button.
    /// Issue: "Results screen: win/fail, score breakdown, restart"
    /// </summary>
    public class ResultsScreen : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text breakdownText;

        void Awake()
        {
            if (panel != null) panel.SetActive(false);
        }

        void OnEnable()  { GameEvents.RunEnded += Show; }
        void OnDisable() { GameEvents.RunEnded -= Show; }

        void Show(RunResult result)
        {
            // TODO: panel.SetActive(true);
            // TODO: titleText.text = result.Won ? "Route complete!" : result.Reason;
            // TODO: breakdownText, one line each:
            //       Passengers  8 x 100      +800
            //       Red lights  1            -50
            //       Collisions  2            -50
            //       Time bonus  12 s x 10    +120
            //       TOTAL                    820
            // Restart button: OnClick -> GameManager.Restart (drag the GameSystems object in).
        }
    }
}
