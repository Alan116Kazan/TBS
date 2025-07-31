using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Отвечает за выбор юнитов игроком и обработку команд на перемещение и атаку.
/// Поддерживает прогнозирование пути с визуализацией и подтверждение атак/движений.
/// </summary>
public class UnitSelectionHandler : MonoBehaviour
{
    private Camera _mainCamera;              // Главная камера для лучей от мыши
    private UnitController _selectedUnit;    // Текущий выбранный юнит
    private UnitController _attackTarget;    // Текущая выбранная цель для атаки
    private Vector3? _predictedTarget;       // Прогнозируемая позиция движения
    private float _lastRightClickTime;       // Время последнего правого клика (для двойного клика)
    private const float _doubleClickThreshold = 0.3f; // Максимальное время между кликами для двойного
    private float _lastReportedDistance = -1f; // Для логирования изменений оставшегося расстояния

    [SerializeField] private LineRenderer greenLineRenderer; // Линия для допустимого пути
    [SerializeField] private LineRenderer redLineRenderer;   // Линия для части пути вне досягаемости

    private void Start()
    {
        _mainCamera = Camera.main;
        ClearPrediction();
    }

    private void Update()
    {
        // Проверяем, что клиент подключён
        if (!NetworkManager.Singleton?.IsConnectedClient ?? true) return;

        // Проверяем, что сейчас ход данного игрока
        ulong myId = NetworkManager.Singleton.LocalClientId;
        if (!TurnManager.Instance.IsPlayerTurn(myId)) return;

        // Обработка кликов мыши
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        else if (Input.GetMouseButtonDown(1) && _selectedUnit != null) HandleRightClick();

        // Логируем изменения оставшегося расстояния движения выбранного юнита
        if (_selectedUnit)
        {
            float currentDistance = _selectedUnit.RemainingMoveDistance;
            if (Mathf.Abs(currentDistance - _lastReportedDistance) > 0.01f)
            {
                Debug.Log($"Оставшееся расстояние: {currentDistance:F2} м");
                _lastReportedDistance = currentDistance;
            }
        }
        else
        {
            _lastReportedDistance = -1f;
        }
    }

    /// <summary>
    /// Обработка левого клика мыши — выбор юнита или снятие выделения.
    /// </summary>
    private void HandleLeftClick()
    {
        if (!Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;

        if (hit.collider.TryGetComponent(out UnitController unit) &&
            unit.OwnerId == NetworkManager.Singleton.LocalClientId)
        {
            // Проверка, завершил ли юнит ход (атаковал и не может двигаться)
            if (unit.HasAttacked && unit.RemainingMoveDistance <= 0f)
            {
                Debug.Log("Этот юнит уже завершил ход.");
                return;
            }

            // Если выбран другой юнит — переключаем выделение
            if (_selectedUnit != unit)
            {
                _selectedUnit?.SetSelected(false);
                ClearAttackTarget();

                _selectedUnit = unit;
                _selectedUnit.SetSelected(true);
            }
        }
        else
        {
            // Клик по пустому пространству или не своему юниту — снимаем выделение
            _selectedUnit?.SetSelected(false);
            _selectedUnit = null;
            ClearPrediction();
            ClearAttackTarget();
        }
    }

    /// <summary>
    /// Обработка правого клика мыши — атака или движение.
    /// </summary>
    private void HandleRightClick()
    {
        if (!Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;

        // Проверяем, если юнит уже атаковал, не даём атаковать повторно
        if (_selectedUnit.HasAttacked && hit.collider.TryGetComponent<UnitController>(out UnitController clickedUnit) &&
            clickedUnit.OwnerId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log("Этот юнит уже атаковал и не может атаковать снова.");
            return;
        }

        if (hit.collider.TryGetComponent<UnitController>(out UnitController clickedUnit2))
        {
            // Клик по своему юниту — отмена выбора цели атаки
            if (clickedUnit2.OwnerId == NetworkManager.Singleton.LocalClientId)
            {
                ClearAttackTarget();
                return;
            }

            // Цель вне радиуса атаки — отмена выбора
            if (!_selectedUnit.IsTargetInRange(clickedUnit2.transform.position))
            {
                Debug.Log("Цель вне радиуса атаки");
                ClearAttackTarget();
                return;
            }

            // Юнит уже атаковал — нельзя атаковать повторно
            if (_selectedUnit.HasAttacked)
            {
                Debug.Log("Этот юнит уже атаковал.");
                return;
            }

            // Выбираем цель для атаки (с подтверждением)
            if (_attackTarget != clickedUnit2)
            {
                ClearAttackTarget();
                _attackTarget = clickedUnit2;
                _attackTarget.SetAttackTargetSelected(true);
                Debug.Log("Цель выбрана для атаки. Повторите клик для подтверждения.");
                return;
            }
            else
            {
                // Подтверждаем атаку
                _selectedUnit.TryAttack(_attackTarget.transform.position);
                Debug.Log($"Атака по цели {_attackTarget.name}!");
                ClearAttackTarget();
                ClearPrediction();
            }
        }
        else
        {
            // Если клик не по юниту — попытка движения
            ClearAttackTarget();

            // Если новая позиция для движения сильно отличается от предыдущей, показываем предсказание пути
            if (!_predictedTarget.HasValue || Vector3.Distance(_predictedTarget.Value, hit.point) > 0.5f)
            {
                _predictedTarget = hit.point;
                DrawPrediction(hit.point);
                _lastRightClickTime = Time.time;
                return;
            }

            // Если двойной клик по той же позиции — подтверждаем движение
            if (Time.time - _lastRightClickTime < _doubleClickThreshold)
            {
                _selectedUnit.TryMove(hit.point);
                ClearPrediction();
            }
            else
            {
                _lastRightClickTime = Time.time;
            }
        }
    }

    /// <summary>
    /// Отрисовка прогнозируемого пути движения с разделением на доступный (зелёный) и недоступный (красный) участки.
    /// </summary>
    private void DrawPrediction(Vector3 target)
    {
        if (_selectedUnit == null || !greenLineRenderer || !redLineRenderer) return;

        NavMeshAgent agent = _selectedUnit.NavAgent;
        if (agent == null) return;

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(target, path)) return;

        Vector3[] corners = path.corners;
        float moveLimit = _selectedUnit.RemainingMoveDistance;

        float totalLength = 0f;
        int splitIndex = corners.Length;
        float overshoot = 0f;

        // Вычисляем, где путь "обрезается" по оставшемуся расстоянию движения
        for (int i = 1; i < corners.Length; i++)
        {
            float segment = Vector3.Distance(corners[i - 1], corners[i]);
            if (totalLength + segment >= moveLimit)
            {
                splitIndex = i;
                overshoot = moveLimit - totalLength;
                break;
            }
            totalLength += segment;
        }

        // Формируем точки зелёной линии (до доступной точки)
        List<Vector3> greenPoints = new();
        for (int i = 0; i < splitIndex; i++) greenPoints.Add(corners[i]);

        if (splitIndex < corners.Length)
        {
            Vector3 dir = (corners[splitIndex] - corners[splitIndex - 1]).normalized;
            greenPoints.Add(corners[splitIndex - 1] + dir * overshoot);
        }

        greenLineRenderer.positionCount = greenPoints.Count;
        greenLineRenderer.SetPositions(greenPoints.ToArray());

        // Формируем точки красной линии (после доступной точки)
        if (splitIndex < corners.Length)
        {
            List<Vector3> redPoints = new() { greenPoints[^1] };
            for (int i = splitIndex; i < corners.Length; i++)
                redPoints.Add(corners[i]);

            redLineRenderer.positionCount = redPoints.Count;
            redLineRenderer.SetPositions(redPoints.ToArray());
        }
        else
        {
            redLineRenderer.positionCount = 0;
        }
    }

    /// <summary>
    /// Очистка прогноза пути и визуализации.
    /// </summary>
    private void ClearPrediction()
    {
        _predictedTarget = null;
        if (greenLineRenderer) greenLineRenderer.positionCount = 0;
        if (redLineRenderer) redLineRenderer.positionCount = 0;
    }

    /// <summary>
    /// Очистка выбранной цели атаки и снятие выделения.
    /// </summary>
    private void ClearAttackTarget()
    {
        if (_attackTarget != null)
        {
            _attackTarget.SetAttackTargetSelected(false);
            _attackTarget = null;
        }
    }
}
