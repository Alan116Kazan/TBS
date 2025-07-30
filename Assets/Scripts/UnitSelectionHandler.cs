using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class UnitSelectionHandler : MonoBehaviour
{
    // Ссылка на главную камеру для лучей от мыши
    private Camera _mainCamera;

    // Текущий выбранный юнит игроком
    private UnitController _selectedUnit;

    // Юнит, выбранный как цель для атаки
    private UnitController _attackTarget;

    // Предсказанная точка для перемещения (для визуализации пути)
    private Vector3? _predictedTarget;

    // Время последнего правого клика мыши (для распознавания двойного клика)
    private float _lastRightClickTime;
    private const float _doubleClickThreshold = 0.3f; // Интервал для двойного клика в секундах

    // Для отладочного вывода — хранит последнее известное оставшееся расстояние движения выбранного юнита
    private float _lastReportedDistance = -1f;

    [SerializeField] private LineRenderer greenLineRenderer; // Линия зеленого цвета для пути, который юнит может пройти
    [SerializeField] private LineRenderer redLineRenderer;   // Линия красного цвета для недопустимого (слишком длинного) пути

    private void Start()
    {
        _mainCamera = Camera.main;  // Получаем основную камеру
        ClearPrediction();          // Сбрасываем любые визуализации пути
    }

    private void Update()
    {
        // Проверяем, что клиент подключён и готов
        if (NetworkManager.Singleton?.IsConnectedClient != true) return;

        ulong myId = NetworkManager.Singleton.LocalClientId;

        // Проверяем, ходит ли локальный игрок сейчас
        if (!TurnManager.Instance.IsPlayerTurn(myId)) return;

        // Обработка нажатий мыши
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();     // Левая кнопка — выбор
        else if (Input.GetMouseButtonDown(1) && _selectedUnit != null) HandleRightClick(); // Правая — действие (атака или движение)

        // Отладочный вывод оставшегося расстояния движения выбранного юнита (выводит в консоль при изменении)
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

    // Обработка левого клика — выбор юнита или сброс выделения
    private void HandleLeftClick()
    {
        if (!Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;

        // Если кликнули по юниту игрока
        if (hit.collider.TryGetComponent(out UnitController unit) &&
            unit.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            // Если юнит уже завершил ход (атаковал и не может двигаться)
            if (unit.HasAttacked && unit.RemainingMoveDistance <= 0f)
            {
                Debug.Log("Этот юнит уже завершил ход.");
                return;
            }

            // Если выбран новый юнит — обновляем выделение
            if (_selectedUnit != unit)
            {
                _selectedUnit?.SetSelected(false); // Снимаем выделение с предыдущего
                ClearAttackTarget();               // Сбрасываем цель атаки

                _selectedUnit = unit;
                _selectedUnit.SetSelected(true);  // Выделяем новый юнит
            }
        }
        else
        {
            // Клик не по своему юниту — снимаем выделение, сбрасываем цели и прогнозы
            _selectedUnit?.SetSelected(false);
            _selectedUnit = null;
            ClearPrediction();
            ClearAttackTarget();
        }
    }

    // Обработка правого клика — атака или перемещение
    private void HandleRightClick()
    {
        if (!Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;
        if (_selectedUnit == null) return;

        // Если юнит уже атаковал и кликнули по другому юниту — запретить повторную атаку
        if (_selectedUnit.HasAttacked && hit.collider.TryGetComponent<UnitController>(out UnitController clickedUnit))
        {
            if (clickedUnit.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("Этот юнит уже атаковал и не может атаковать снова.");
                return;
            }
        }

        // Обработка клика по юниту (свой или чужой)
        if (hit.collider.TryGetComponent<UnitController>(out UnitController clickedUnit2))
        {
            if (clickedUnit2.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                // Клик по своему юниту — сброс цели атаки
                ClearAttackTarget();
                return;
            }

            // Клик по вражескому юниту — проверяем радиус атаки
            if (!_selectedUnit.IsTargetInRange(clickedUnit2.transform.position))
            {
                Debug.Log("Цель вне радиуса атаки");
                ClearAttackTarget();
                return;
            }

            if (_selectedUnit.HasAttacked)
            {
                Debug.Log("Этот юнит уже атаковал.");
                return;
            }

            // Если цель ещё не выбрана или выбрана другая — выделяем её и ждем подтверждения
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
                // Повторный клик — подтверждаем атаку и вызываем серверный RPC
                _selectedUnit.TryAttackServerRpc(_attackTarget.transform.position);
                Debug.Log($"Атака по цели {_attackTarget.name}!");

                ClearAttackTarget();
                ClearPrediction();
            }
        }
        else
        {
            // Клик по пустому месту — перемещение юнита

            ClearAttackTarget();

            // Если новая точка движения отличается от предыдущей — обновляем визуализацию пути
            if (!_predictedTarget.HasValue || Vector3.Distance(_predictedTarget.Value, hit.point) > 0.5f)
            {
                _predictedTarget = hit.point;
                DrawPrediction(hit.point);
                _lastRightClickTime = Time.time;
                return;
            }

            // Проверяем двойной клик (два правых клика подряд по одной точке) — подтверждаем перемещение
            if (Time.time - _lastRightClickTime < _doubleClickThreshold)
            {
                _selectedUnit.TryMoveServerRpc(hit.point);
                ClearPrediction();
            }
            else
            {
                _lastRightClickTime = Time.time;
            }
        }
    }

    // Визуализация прогнозируемого пути перемещения
    private void DrawPrediction(Vector3 target)
    {
        if (!_selectedUnit || !greenLineRenderer || !redLineRenderer) return;

        NavMeshAgent agent = _selectedUnit.NavAgent;
        if (agent == null) return;

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(target, path)) return;

        Vector3[] corners = path.corners;
        float moveLimit = _selectedUnit.RemainingMoveDistance;

        float totalLength = 0f;
        int splitIndex = corners.Length;
        float overshoot = 0f;

        // Ищем точку, где путь превышает оставшееся расстояние движения
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

        // Зеленая часть пути — доступная для перемещения
        List<Vector3> greenPoints = new List<Vector3>();
        for (int i = 0; i < splitIndex; i++)
            greenPoints.Add(corners[i]);

        if (splitIndex < corners.Length)
        {
            Vector3 dir = (corners[splitIndex] - corners[splitIndex - 1]).normalized;
            greenPoints.Add(corners[splitIndex - 1] + dir * overshoot);
        }

        greenLineRenderer.positionCount = greenPoints.Count;
        greenLineRenderer.SetPositions(greenPoints.ToArray());

        // Красная часть пути — недоступная (слишком длинная)
        if (splitIndex < corners.Length)
        {
            List<Vector3> redPoints = new List<Vector3> { greenPoints[^1] };
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

    // Сброс визуализации пути
    private void ClearPrediction()
    {
        _predictedTarget = null;
        if (greenLineRenderer) greenLineRenderer.positionCount = 0;
        if (redLineRenderer) redLineRenderer.positionCount = 0;
    }

    // Сброс выделения цели атаки
    private void ClearAttackTarget()
    {
        if (_attackTarget != null)
        {
            _attackTarget.SetAttackTargetSelected(false);
            _attackTarget = null;
        }
    }
}
