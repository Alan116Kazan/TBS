/// <summary>
/// ������ ������ ������ ������� ����� ������� �����.
/// </summary>
public class PlayerUnitSelectionData
{
    /// <summary>
    /// ���������� ��������� ��������� ������ (� ������� �������� �����).
    /// </summary>
    public int SlowUnitCount;

    /// <summary>
    /// ���������� ��������� ������� ������ (� ������� �������� �����, �� ������� ���������).
    /// </summary>
    public int FastUnitCount;

    /// <summary>
    /// �������� ���������� ������ � ������ ���� (������ ���� �� ���� ����).
    /// </summary>
    public bool IsReady => (SlowUnitCount + FastUnitCount) > 0;

    /// <summary>
    /// ����������� � �����������.
    /// </summary>
    public PlayerUnitSelectionData(int slowCount = 0, int fastCount = 0)
    {
        SlowUnitCount = slowCount;
        FastUnitCount = fastCount;
    }
    public int TotalUnits => SlowUnitCount + FastUnitCount;
}
