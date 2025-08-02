/// <summary>
/// Данные выбора юнитов игроком перед началом матча.
/// </summary>
public class PlayerUnitSelectionData
{
    /// <summary>
    /// Количество выбранных медленных юнитов (с дальним радиусом атаки).
    /// </summary>
    public int SlowUnitCount;

    /// <summary>
    /// Количество выбранных быстрых юнитов (с меньшим радиусом атаки, но большей скоростью).
    /// </summary>
    public int FastUnitCount;

    /// <summary>
    /// Проверка готовности игрока к началу игры (выбран хотя бы один юнит).
    /// </summary>
    public bool IsReady => (SlowUnitCount + FastUnitCount) > 0;

    /// <summary>
    /// Конструктор с параметрами.
    /// </summary>
    public PlayerUnitSelectionData(int slowCount = 0, int fastCount = 0)
    {
        SlowUnitCount = slowCount;
        FastUnitCount = fastCount;
    }
    public int TotalUnits => SlowUnitCount + FastUnitCount;
}
