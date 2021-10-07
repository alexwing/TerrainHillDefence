using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HillDefence
{
    public class TeamFlag : NpcInfo
    {
        // material to change color of the flag
        public GameObject flag;



        int flagShootsReceived;

        // Use this for initialization
        void Start()
        {
            // changeFlagColor(teamColor);

            Utils.ChangeColor(flag.GetComponent<Renderer>(), HillDefenceCreator.teams[npcInfo.teamNumber].teamColor);
            Utils.ChangeColor(flag.GetComponent<Renderer>(), Utils.Darken(HillDefenceCreator.teams[npcInfo.teamNumber].teamColor, 0.75f), "_EmissionColor");
        }

        // change the color of the flag
        public void changeFlagColor(Color color)
        {
            Renderer newRenderer = flag.GetComponent<Renderer>();
            var propBlock = new MaterialPropertyBlock();
            newRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color);
            propBlock.SetColor("_EmissionColor", Utils.Darken(color, 0.25f));
            newRenderer.SetPropertyBlock(propBlock);
        }

        void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "bullet" && "bullet" + npcInfo.teamNumber != collision.gameObject.name)
            {
                flagShootsReceived++;
                if (SceneConfig.FLAG.Lives == flagShootsReceived)
                {
                    //win a flag 
                    npcInfo.isDead = true;
                    HillDefence.HillDefenceCreator.teams[collision.gameObject.GetComponent<Bullet>().npcInfo.teamNumber].flagsWinsCount++;
                    Destroy(gameObject);
                    TargetTerrain.instance.ModifyTerrain(gameObject, 80, 1000,false);
                    TargetTerrain.instance.DetonationTerrain(collision.gameObject, 50);
                    HillDefenceCreator.instance.EvaluateWin();
                }
                Destroy(collision.gameObject);

            }
        }
    }

}
