using TMPro;
using UnityEngine;

namespace HillDefence
{
    public class UIController : MonoBehaviour
    {

        public static UIController instance;
        public GameObject cursorPointer;
        public Terrain anchorToTerrain;

        public TextMeshProUGUI winText;

        public Transform healthLayoutHolder;
        public GameObject healthLayout;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);

            }
        }

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

        public void ShowWin(Team Team)
        {
            winText.transform.gameObject.SetActive(true);
            winText.text = Team.teamNumber + " wins!";
           // winText.color = Team.teamColor;

        }

        public void CreateHealthbars()
        {
            // Clear healthbars
            for (int i = 0; i < healthLayoutHolder.childCount; i++)
            {
                GameObject.Destroy(healthLayoutHolder.GetChild(i).gameObject);
            }

            // print ("CreateHealthbars"+HillDefenceCreator.soldiers.Count);

            for (int i = 0; i < HillDefenceCreator.soldiers.Count; i++)
            {
                CreateHealthbar(HillDefenceCreator.soldiers[i]);
            }

        }

        void CreateHealthbar(TeamSoldier teamSoldier)
        {
            GameObject newHealthbar = GameObject.Instantiate(healthLayout, healthLayoutHolder);
            newHealthbar.transform.SetParent(healthLayoutHolder);
            newHealthbar.transform.position = Vector3.zero;

            newHealthbar.GetComponent<HealthLayout>().SetUp(teamSoldier);
        }
    }

}