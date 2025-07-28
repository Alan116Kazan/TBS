using UnityEngine;

public class PlayerUnitSelectionData
{
    public int shortMoveLongRangeCount;
    public int longMoveShortRangeCount;

    public bool IsReady => shortMoveLongRangeCount + longMoveShortRangeCount > 0;
}
