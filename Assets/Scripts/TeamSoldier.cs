using UnityEngine;


namespace HillDefence
{
public class TeamSoldier: MonoBehaviour
{
    // material to change color of the flag
    public GameObject body;
    public GameObject head;
    public GameObject arms;

        //team
        public Team team;

    

    // Use this for initialization
    void Start()
    {
        Utils.ChangeColor(body.GetComponent<Renderer>(), team.teamColor);
        Utils.ChangeColor(head.GetComponent<Renderer>(), team.teamColor);
        Utils.ChangeColor(arms.GetComponent<Renderer>(), team.teamColor);
    }

        public void setTeam(Team currentTeam)
        {
            team = currentTeam;

        }

}
}

