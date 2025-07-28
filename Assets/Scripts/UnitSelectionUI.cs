using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UnitSelectionUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button startButton;
    [SerializeField] private InputField shortRangeInput;
    [SerializeField] private InputField longRangeInput;

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
        int.TryParse(shortRangeInput.text, out int shortCount);
        int.TryParse(longRangeInput.text, out int longCount);

        var selection = new PlayerUnitSelectionData
        {
            longMoveShortRangeCount = shortCount,
            shortMoveLongRangeCount = longCount
        };

        UnitSelectionManager.Instance.SubmitSelection(selection);
        panel.SetActive(false);
    }
}
