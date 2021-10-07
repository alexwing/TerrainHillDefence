using UnityEngine;
using UnityEngine.UI;
namespace HillDefence
{
    public class HealthLayout : MonoBehaviour
    {
        public TeamSoldier teamSoldier;

        public Image healthImage;

        public float yOffset = 3;

        public void SetUp(TeamSoldier teamSoldier)
        {
            this.teamSoldier = teamSoldier;
        }

        void Update()
        {
            if (teamSoldier)
                Destroy(gameObject);

            healthImage.fillAmount = (float)teamSoldier.npcInfo.shootCount / (float)SceneConfig.SOLDIER.Lives;

            transform.position = Camera.main.WorldToScreenPoint(teamSoldier.transform.position + (Vector3.up * yOffset));
        }

    }
}