using UnityEngine;

public class HoverPreviewController : MonoBehaviour
{
    private BranchController branch;

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
    }

    private void OnMouseEnter()
    {
        if (branch == null)
        {
            return;
        }

        branch.ShowPreview();
    }

    private void OnMouseExit()
    {
        if (branch == null)
        {
            return;
        }

        branch.HidePreview();
    }
}
