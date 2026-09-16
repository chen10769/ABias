using UnityEngine;
using UnityEngine.EventSystems;

public class EvidenceRaycaster : MonoBehaviour
{
    public LayerMask evidenceLayer;
    public float rayDistance = 100f;

    public  Evidence currentEvidence;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, evidenceLayer)&&!EventSystem.current.IsPointerOverGameObject()&&UIManager.instance.isUI==false&&VisualSearchExperiment.instance.imagePanel.activeInHierarchy==false)
        {
            Evidence evidence = hit.collider.GetComponent<Evidence>();
            if (evidence != null)
            {
                if (currentEvidence != evidence)
                {
                    currentEvidence?.OnHoverExit();
                    currentEvidence = evidence;
                }
                currentEvidence.OnHover();

                if (Input.GetMouseButtonDown(0))
                {
                    currentEvidence.OnClick();
                }
                return;
            }
        }

        // 没有命中或移开
        if (currentEvidence != null)
        {
            currentEvidence.OnHoverExit();
            currentEvidence = null;
        }
    }
}
