using UnityEngine;

public class UIController : MonoBehaviour
{


    public GameObject cursorPointer;
    public Terrain anchorToTerrain;
    private void Update()
    {

        //raycast mouse cursor to objetct pointer in terrain
        if (Input.GetMouseButton(0) && anchorToTerrain)
        {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 10000f))
            {
                if (hit.collider.gameObject == anchorToTerrain.gameObject)
                {
                    cursorPointer.SetActive(true);
                  //  Debug.Log("position x: " + hit.transform.position.x + " position z: " + hit.transform.position.z);
                    cursorPointer.transform.position = hit.point;
                }
            }
        }
        else
        {
            //hide cursor pointer when no mouse click
            cursorPointer.SetActive(false);
        }
    }
}