using Unity.Netcode;
using UnityEngine;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private UnitStatusUI unitStatusUI;

    [SerializeField] private LineRenderer greenLineRenderer;
    [SerializeField] private LineRenderer redLineRenderer;

    private Camera _camera;
    private UnitSelector _selector;
    private AttackHandler _attackHandler;
    private MovePredictionDrawer _predictor;

    private Vector3? _prediction;
    private float _lastClickTime;
    private const float DoubleClickThreshold = 0.3f;

    private void Awake()
    {
        _selector = new UnitSelector();
        _attackHandler = new AttackHandler();
        _predictor = new MovePredictionDrawer(greenLineRenderer, redLineRenderer);
    }

    private void Start() => _camera = Camera.main;

    private void OnEnable() => GameEvents.OnTurnStarted += OnTurnStarted;

    private void OnDisable() => GameEvents.OnTurnStarted -= OnTurnStarted;

    private void OnTurnStarted(ulong id)
    {
        if (id != NetworkManager.Singleton.LocalClientId)
        {
            _selector.Deselect();
            _attackHandler.ClearTarget();
            _predictor.Clear();
            _prediction = null;
        }
        else if (_prediction.HasValue && _selector.SelectedUnit != null)
        {
            _predictor.Draw(_selector.SelectedUnit, _prediction.Value);
        }
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsConnectedClient) return;

        if (!TurnManager.Instance.IsPlayerTurn(NetworkManager.Singleton.LocalClientId)) return;

        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        else if (Input.GetMouseButtonDown(1) && _selector.SelectedUnit != null) HandleRightClick();
    }

    private void HandleLeftClick()
    {
        if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit)) return;

        if (hit.collider.TryGetComponent(out UnitController unit) &&
            unit.OwnerId == NetworkManager.Singleton.LocalClientId)
        {
            if (unit.HasAttacked && unit.RemainingMoveDistance <= 0f) return;

            _selector.Select(unit);
            unitStatusUI.SetUnit(unit); // <-- обновляем UI
        }
        else
        {
            _selector.Deselect();
            _attackHandler.ClearTarget();
            _predictor.Clear();
            _prediction = null;
            unitStatusUI.Clear(); // <-- очищаем UI
        }
    }

    private void HandleRightClick()
    {
        if (!Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hit)) return;

        var selected = _selector.SelectedUnit;
        if (hit.collider.TryGetComponent(out UnitController target))
        {
            if (target.OwnerId == NetworkManager.Singleton.LocalClientId)
            {
                _attackHandler.ClearTarget();
                return;
            }

            _attackHandler.HandleAttack(selected, target);
            _prediction = null;
            return;
        }

        _attackHandler.ClearTarget();

        if (!_prediction.HasValue || Vector3.Distance(_prediction.Value, hit.point) > 0.5f)
        {
            _prediction = hit.point;
            _predictor.Draw(selected, hit.point);
            _lastClickTime = Time.time;
        }
        else if (Time.time - _lastClickTime < DoubleClickThreshold)
        {
            selected.TryMove(hit.point);
            _predictor.Clear();
            _prediction = null;
        }
        else
        {
            _lastClickTime = Time.time;
        }
    }
}
