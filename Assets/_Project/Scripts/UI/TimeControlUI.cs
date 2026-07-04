using UnityEngine;
using UnityEngine.UI;

public class TimeControlUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button speedUpButton;
    [SerializeField] private Button speedDownButton;

    [Header("Icons")]
    [SerializeField] private Image pauseImage;
    [SerializeField] private Sprite pauseIcon;
    [SerializeField] private Sprite playIcon;

    [Header("Speed Bars")]
    [SerializeField] private Image[] speedBars;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;

    private SimulationManager simulationManager;

    void Start()
    {
        simulationManager = GameManager.SimulationManager;

        if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
        
        if (speedUpButton != null) speedUpButton.onClick.AddListener(SpeedUp);

        if (speedDownButton != null) speedDownButton.onClick.AddListener(SpeedDown);
    }

    void Update()
    {
        if (simulationManager == null) return;
        
        if (pauseImage != null)
        {
            pauseImage.sprite = simulationManager.Running ? playIcon : pauseIcon;
        }
        
        if (speedBars != null)
        {
            int currentSpeed = simulationManager.SimulationSpeed;

            for (int i = 0; i < speedBars.Length; i++)
            {
                if (speedBars[i] != null)
                {
                    speedBars[i].color = (i < currentSpeed) ? activeColor : inactiveColor;
                }
            }
        }
    }

    void TogglePause()
    {
        if (simulationManager == null) return;

        if (simulationManager.Running) simulationManager.PauseSimulation(); else simulationManager.ResumeSimulation();
    }

    void SpeedUp()
    {
        simulationManager?.IncreaseSimulationSpeed();
    }

    void SpeedDown()
    {
        simulationManager?.DecreaseSimulationSpeed();
    }
}