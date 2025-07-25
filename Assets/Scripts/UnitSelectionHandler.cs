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
        // ���������� ������ ���� �������� ������ ���������� ������
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
                // ��������� ������ �����
                if (selectedUnit != null)
                    selectedUnit.SetSelected(false);

                selectedUnit = unit;
                selectedUnit.SetSelected(true);

                Debug.Log("���� ������: " + unit.name);
                return;
            }
        }

        // ���� �� ����� ���� � �������� �� �� ����� ��� �� ������ �����
        if (selectedUnit != null)
        {
            selectedUnit.SetSelected(false);
            selectedUnit = null;
            Debug.Log("����� �������");
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
