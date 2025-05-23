using UnityEngine;
using UnityEngine.UI;

public class DataViewManager : MonoBehaviour
{
    [SerializeField]
    private GameObject liveDataCanvas;

    [SerializeField]
    private GameObject historicalDataCanvas;

    [SerializeField]
    private Button liveDataButton;

    [SerializeField]
    private Button historicalDataButton;

    [SerializeField]
    private Color selectedColor = new(0.8f, 0.8f, 1f); // Light blue selected state

    [SerializeField]
    private Color normalColor = Color.white; // Normal state

    private void Start()
    {
        liveDataButton.onClick.AddListener(ShowLiveData);
        historicalDataButton.onClick.AddListener(ShowHistoricalData);

        // Default view
        ShowLiveData();
    }

    public void ShowLiveData()
    {
        liveDataCanvas.SetActive(true);
        historicalDataCanvas.SetActive(false);

        // Visual feedback
        liveDataButton.interactable = true;
        historicalDataButton.interactable = true;

        // Change colors to indicate selection
        liveDataButton.image.color = selectedColor;
        historicalDataButton.image.color = normalColor;
    }

    public void ShowHistoricalData()
    {
        liveDataCanvas.SetActive(false);
        historicalDataCanvas.SetActive(true);

        // Visual feedback
        liveDataButton.interactable = true;
        historicalDataButton.interactable = true;

        // Change colors to indicate selection
        liveDataButton.image.color = normalColor;
        historicalDataButton.image.color = selectedColor;
    }
}
