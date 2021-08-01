using UnityEngine;

public class TeamSoldier: MonoBehaviour
{
    // material to change color of the flag
    public GameObject body;
    public GameObject head;
    public GameObject arms;
    public Color teamColor;

    // Use this for initialization
    void Start()
    {
        Utils.ChangeColor(body.GetComponent<Renderer>(), teamColor);
        Utils.ChangeColor(head.GetComponent<Renderer>(), teamColor);
        Utils.ChangeColor(arms.GetComponent<Renderer>(), teamColor);
    }


}

