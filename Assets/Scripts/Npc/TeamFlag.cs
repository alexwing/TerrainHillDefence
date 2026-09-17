using UnityEngine;

namespace HillDefence
{
    public class TeamFlag : NpcInfo
    {
        // material to change color of the flag
        public GameObject flag;

        int flagShootsReceived;

        /// <summary>Whether this flag is currently receiving fire.</summary>
        [HideInInspector] public bool isUnderAttack = false;
        /// <summary>The NPC that last shot at this flag (used by soldiers to defend).</summary>
        [HideInInspector] public GameNpc lastAttacker = null;

        // How long (seconds) after last hit to keep isUnderAttack = true
        private const float UnderAttackCooldown = 5f;
        private float _lastHitTime = -999f;

        // Use this for initialization
        void Start()
        {
            Utils.ChangeColor(flag.GetComponent<Renderer>(), HillDefenceCreator.teams[npcInfo.teamNumber].teamColor);
            Utils.ChangeColor(flag.GetComponent<Renderer>(), Utils.Darken(HillDefenceCreator.teams[npcInfo.teamNumber].teamColor, 0.75f), "_EmissionColor");
        }

        void Update()
        {
            // Clear attack state after cooldown
            if (isUnderAttack && Time.time - _lastHitTime > UnderAttackCooldown)
            {
                isUnderAttack = false;
                lastAttacker = null;
            }
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
            if (!collision.gameObject) return;

            // Bullets have name format "bullet_<teamNumber>"
            if (collision.gameObject.tag == "bullet" && "bullet_" + npcInfo.teamNumber != collision.gameObject.name)
            {
                flagShootsReceived++;

                // Track attacker so nearby soldiers can defend
                Bullet bullet = collision.gameObject.GetComponent<Bullet>();
                if (bullet != null && bullet.npcInfo != null && bullet.npcInfo.teamNumber != npcInfo.teamNumber)
                {
                    lastAttacker = bullet.npcInfo;
                    isUnderAttack = true;
                    _lastHitTime = Time.time;
                }

                if (SceneConfig.FLAG.Lives <= flagShootsReceived)
                {
                    deathNPC(collision.gameObject);
                }
                Destroy(collision.gameObject);
            }
        }

        public void deathNPC(GameObject collision)
        {
            npcInfo.isDead = true;
            Bullet bullet = collision.GetComponent<Bullet>();
            if (bullet != null && bullet.npcInfo != null)
            {
                HillDefenceCreator.teams[bullet.npcInfo.teamNumber].flagsWinsCount++;
            }
            Destroy(gameObject);
            TargetTerrain.instance.ModifyTerrain(gameObject, 80, 1000, false);
            TargetTerrain.instance.DetonationTerrain(collision, SceneConfig.FLAG.DetonationSize);
            HillDefenceCreator.instance.EvaluateWin();
        }
    }
}
