/// <summary>
/// Управляет выбором юнита.
/// </summary>
public class UnitSelector
{
    public UnitController SelectedUnit { get; private set; }

    public void Select(UnitController unit)
    {
        if (SelectedUnit == unit)
            return;

        Deselect();

        SelectedUnit = unit;
        SelectedUnit.SetSelected(true);
    }

    public void Deselect()
    {
        if (SelectedUnit != null)
        {
            SelectedUnit.SetSelected(false);
            SelectedUnit = null;
        }
    }
}
