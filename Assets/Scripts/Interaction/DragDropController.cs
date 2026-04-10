using UnityEngine;

public class DragDropController : MonoBehaviour
{
    private const float DragStartThreshold = 0.08f;

    private BranchController branch;
    private Camera mainCamera;
    private Vector3 pointerDownWorldPosition;
    private bool isPointerDown;
    private bool isDragging;
    private GameObject scionDragGhost;
    private SpriteRenderer scionDragRenderer;
    private BranchController currentPreviewTarget;

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

        pointerDownWorldPosition = GetMouseWorldPosition();
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
            branch.BeginSourceRecovery();
            EnsureScionGhost();
            UpdateScionGhostPosition(mouseWorldPosition);
            scionDragGhost.SetActive(true);
        }

        if (isDragging)
        {
            UpdateScionGhostPosition(mouseWorldPosition);
            UpdatePreviewTarget();
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
        }
        else if (branch.IsWorldPointInsidePreviewFlower(mouseWorldPosition))
        {
            branch.TryStartGrowthFromPreview();
        }
        else if (branch.IsWorldPointInsideMatureFlower(mouseWorldPosition))
        {
            branch.TryStartChargeHarvestFromMatureFlower();
        }

        ClearPreviewTarget();
        HideScionGhost();
        isPointerDown = false;
        isDragging = false;
    }

    private bool CanInteract()
    {
        return branch != null
            && branch.Data != null
            && branch.Data.State != BranchState.Growing
            && branch.Data.State != BranchState.GraftGrowing
            && branch.Data.State != BranchState.SourceRecovering;
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
            if (hitBranch != null && hitBranch != branch && hitBranch.CanReceiveGraft())
            {
                return hitBranch;
            }
        }

        return null;
    }

    private void ApplyFusionToTarget(BranchController targetBranch)
    {
        FlowerColor fusedColor = ColorFusionSystem.Fuse(branch.CurrentColor, targetBranch.CurrentColor);
        targetBranch.TryBeginGraftedGrowth(fusedColor);
    }

    private void UpdatePreviewTarget()
    {
        BranchController targetBranch = FindTargetBranchUnderMouse();
        if (targetBranch == currentPreviewTarget)
        {
            return;
        }

        if (currentPreviewTarget != null)
        {
            currentPreviewTarget.SetGraftPreviewActive(false);
        }

        currentPreviewTarget = targetBranch;
        if (currentPreviewTarget != null)
        {
            currentPreviewTarget.SetGraftPreviewActive(true);
        }
    }

    private void ClearPreviewTarget()
    {
        if (currentPreviewTarget != null)
        {
            currentPreviewTarget.SetGraftPreviewActive(false);
            currentPreviewTarget = null;
        }
    }

    private void EnsureScionGhost()
    {
        if (scionDragGhost != null)
        {
            return;
        }

        scionDragGhost = new GameObject($"ScionDragGhost_{name}");
        scionDragRenderer = scionDragGhost.AddComponent<SpriteRenderer>();
        scionDragRenderer.sortingOrder = 40;
        scionDragRenderer.color = Color.white;
        scionDragGhost.SetActive(false);
    }

    private void UpdateScionGhostPosition(Vector3 worldPosition)
    {
        if (scionDragGhost == null)
        {
            return;
        }

        scionDragRenderer.sprite = branch != null ? branch.ScionDragSprite : null;
        scionDragGhost.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
        scionDragGhost.transform.localScale = branch != null ? branch.ScionDragScale : Vector3.one;
    }

    private void HideScionGhost()
    {
        if (scionDragGhost != null)
        {
            scionDragGhost.SetActive(false);
        }
    }
}
