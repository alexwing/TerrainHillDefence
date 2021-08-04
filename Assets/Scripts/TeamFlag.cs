using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HillDefence
{
    public class TeamFlag : MonoBehaviour
    {
        // material to change color of the flag
        public GameObject flag;
        public Color teamColor;

        public int teamNumber;



        int flagShootsReceived;

        // Use this for initialization
        void Start()
        {
            // changeFlagColor(teamColor);

            Utils.ChangeColor(flag.GetComponent<Renderer>(), teamColor);
            Utils.ChangeColor(flag.GetComponent<Renderer>(), Utils.Darken(teamColor, 0.75f), "_EmissionColor");
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
            if (collision.gameObject.tag == "bullet" && "bullet" + teamNumber != collision.gameObject.name)
            {
                flagShootsReceived++;
                if (SceneConfig.flagShootsToWin == flagShootsReceived)
                {
                    //win a flag 
                    HillDefence.HillDefenceCreator.teams[collision.gameObject.GetComponent<Bullet>().teamNumber].flagsWinsCount++;
                    Destroy(gameObject);
                    TargetTerrain.instance.ModifyTerrain(gameObject, 1000, 1000,false);
                    TargetTerrain.instance.DetonationTerrain(collision.gameObject, 1000);
                }
                Destroy(collision.gameObject);

            }
        }
    }

}
