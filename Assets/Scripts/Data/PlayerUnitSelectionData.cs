using UnityEngine;

public class PlayerUnitSelectionData
{
    public int SlowUnitCount;
    public int FastUnitCount;

    public bool IsReady => SlowUnitCount + FastUnitCount > 0;
}
