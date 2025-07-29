using UnityEngine;

public class UnitSelectionVisuals : MonoBehaviour
{
    [SerializeField] private GameObject selectionCircle;
    [SerializeField] private GameObject attackTargetHighlight;
    [SerializeField] private AttackRangeVisualizer rangeVisualizer;

    private float _attackRange;

    private void Reset()
    {
        // јвтоматически подт€гиваем компоненты в инспекторе (если забыли назначить)
        if (rangeVisualizer == null)
            rangeVisualizer = GetComponent<AttackRangeVisualizer>() ?? GetComponentInChildren<AttackRangeVisualizer>();
    }

    public void Initialize(float attackRange)
    {
        this._attackRange = attackRange;
    }

    public void ShowSelection(bool selected, bool hasAttacked)
    {
        selectionCircle?.SetActive(selected);

        bool showRange = selected && !hasAttacked;

        if (rangeVisualizer != null)
        {
            rangeVisualizer.Show(showRange);
            if (showRange)
            {
                rangeVisualizer.SetRange(_attackRange);
                rangeVisualizer.Draw();
            }
        }
    }

    public void ShowAttackTargetHighlight(bool selected)
    {
        attackTargetHighlight?.SetActive(selected);
    }
}
