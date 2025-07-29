using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UnitSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button startButton;
    [SerializeField] private InputField FastUnitInput;
    [SerializeField] private InputField SlowUnitInput;

    private void Start()
    {
        panel.SetActive(false);
        startButton.onClick.AddListener(OnSubmitSelection);
    }

    public void Show()
    {
        panel.SetActive(true);
    }

    private void OnSubmitSelection()
    {
        int.TryParse(FastUnitInput.text, out int shortCount);
        int.TryParse(SlowUnitInput.text, out int longCount);

        var selection = new PlayerUnitSelectionData
        {
            FastUnitCount = shortCount,
            SlowUnitCount = longCount
        };

        UnitSelectionManager.Instance.SubmitSelection(selection);
        panel.SetActive(false);
    }
}
