using UnityEngine;

public class DragDropController : MonoBehaviour
{
    private const float DragStartThreshold = 0.08f;

    private BranchController branch;
    private Camera mainCamera;
    private Vector3 originalWorldPosition;
    private Vector3 pointerDownWorldPosition;
    private Vector3 dragOffset;
    private bool isPointerDown;
    private bool isDragging;

    public void Initialize(BranchController branchController)
    {
        branch = branchController;
    }

    private void Awake()
    {
        if (branch == null)
        {
            branch = GetComponent<BranchController>();
        }

        mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        if (!CanInteract())
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        originalWorldPosition = transform.position;
        pointerDownWorldPosition = GetMouseWorldPosition();
        dragOffset = transform.position - pointerDownWorldPosition;
        isPointerDown = true;
        isDragging = false;
    }

    private void OnMouseDrag()
    {
        if (!isPointerDown)
        {
            return;
        }

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        if (CanStartBranchDrag() && !isDragging && Vector3.Distance(mouseWorldPosition, pointerDownWorldPosition) >= DragStartThreshold)
        {
            isDragging = true;
            branch.HidePreview();
        }

        if (isDragging)
        {
            transform.position = mouseWorldPosition + dragOffset;
        }
    }

    private void OnMouseUp()
    {
        if (!isPointerDown)
        {
            return;
        }

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        if (isDragging)
        {
            BranchController targetBranch = FindTargetBranchUnderMouse();
            if (targetBranch != null)
            {
                ApplyFusionToTarget(targetBranch);
            }

            transform.position = originalWorldPosition;
        }
        else if (branch.IsWorldPointInsidePreviewFlower(mouseWorldPosition))
        {
            branch.TryStartGrowthFromPreview();
        }
        else if (branch.IsWorldPointInsideMatureFlower(mouseWorldPosition))
        {
            branch.TryHarvestMatureFlower();
        }

        isPointerDown = false;
        isDragging = false;
    }

    private bool CanInteract()
    {
        return branch != null && branch.Data != null && branch.Data.State != BranchState.Growing;
    }

    private bool CanStartBranchDrag()
    {
        return branch != null && branch.Data != null && branch.Data.State == BranchState.Idle;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            return transform.position;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = transform.position.z;
        return worldPosition;
    }

    private BranchController FindTargetBranchUnderMouse()
    {
        Vector2 mousePoint = GetMouseWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePoint);

        foreach (Collider2D hit in hits)
        {
            BranchController hitBranch = hit.GetComponent<BranchController>();
            if (hitBranch != null && hitBranch != branch && hitBranch.Data.State == BranchState.Idle)
            {
                return hitBranch;
            }
        }

        return null;
    }

    private void ApplyFusionToTarget(BranchController targetBranch)
    {
        FlowerColor fusedColor = ColorFusionSystem.Fuse(branch.CurrentColor, targetBranch.CurrentColor);
        targetBranch.SetCurrentColor(fusedColor);
    }
}
