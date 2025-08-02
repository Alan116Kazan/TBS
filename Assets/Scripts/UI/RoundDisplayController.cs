using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Отвечает за отображение текущего номера раунда на UI.
/// Подписывается на событие смены раунда и обновляет текст.
/// </summary>
public class RoundDisplayController : MonoBehaviour
{
    [SerializeField] private Text roundText;

    private void Start()
    {
        UpdateRoundUI();
    }

    private void OnEnable()
    {
        GameEvents.OnRoundChanged += HandleRoundChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnRoundChanged -= HandleRoundChanged;
    }

    /// <summary>
    /// Обновляет отображение номера раунда при его изменении.
    /// </summary>
    /// <param name="round">Номер текущего раунда.</param>
    private void HandleRoundChanged(int round)
    {
        roundText.text = $"Раунд: {round}";
    }

    /// <summary>
    /// Инициализирует текст при старте на основе данных TurnManager.
    /// </summary>
    private void UpdateRoundUI()
    {
        if (TurnManager.Instance != null)
            roundText.text = $"Раунд: {TurnManager.Instance.CurrentRound}";
    }
}
