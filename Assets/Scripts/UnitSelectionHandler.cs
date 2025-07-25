using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class UnitSelectionHandler : MonoBehaviour
{
    private Camera mainCamera;
    private UnitController selectedUnit;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Управление должно быть доступно только локальному игроку
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleLeftClick();
        }
        else if (Input.GetMouseButtonDown(1) && selectedUnit != null)
        {
            HandleRightClick();
        }
    }

    private void HandleLeftClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var unit = hit.collider.GetComponent<UnitController>();
            if (unit != null && unit.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                // Выделение нового юнита
                if (selectedUnit != null)
                    selectedUnit.SetSelected(false);

                selectedUnit = unit;
                selectedUnit.SetSelected(true);

                Debug.Log("Юнит выбран: " + unit.name);
                return;
            }
        }

        // Если мы дошли сюда — кликнули не по юниту или по чужому юниту
        if (selectedUnit != null)
        {
            selectedUnit.SetSelected(false);
            selectedUnit = null;
            Debug.Log("Выбор сброшен");
        }
    }



    private void HandleRightClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            selectedUnit.SetDestinationServerRpc(hit.point);
        }
    }
}
