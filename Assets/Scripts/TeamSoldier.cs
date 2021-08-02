using UnityEngine;


namespace HillDefence
{
    public class TeamSoldier : MonoBehaviour
    {
        // material to change color of the flag
        public GameObject body;
        public GameObject head;
        public GameObject arms;

        private readonly float AttackRange = SceneConfig.SOLDIER.AttackRange;
        private readonly float AttackRamdomRange = SceneConfig.SOLDIER.AttackRamdomRange;
        private readonly float SoldierVelocity = SceneConfig.SOLDIER.SoldierVelocity;
        private readonly int SoldierFrameRate = SceneConfig.SOLDIER.SoldierFrameRate;
        private readonly int FindEnemyFrameRate = SceneConfig.SOLDIER.FindEnemyFrameRate;


        private float AttackDistance = 0f;

        //team
        public Team team;


        [HideInInspector]
        public  GameObject enemy = null;


        private Animator animator;
        private bool isWalking = false;
        private string animateStatus = "";




        // Use this for initialization

        void Awake()
        {
            animator = GetComponent<Animator>();
            AttackDistance = Random.Range(-AttackRamdomRange, AttackRamdomRange);
            
        }
        void Start()
        {
            Utils.ChangeColor(body.GetComponent<Renderer>(), team.teamColor);
            Utils.ChangeColor(head.GetComponent<Renderer>(), team.teamColor);
            Utils.ChangeColor(arms.GetComponent<Renderer>(), team.teamColor);
            InvokeRepeating("UpdateSoldier",0, 1f / SoldierFrameRate);
            InvokeRepeating("findEnemy", 0, 1f / SoldierFrameRate);

        }

        public void setTeam(Team currentTeam)
        {
            team = currentTeam;
            animator.Play("standing_idle_looking_ver_1", -1, Random.Range(0.0f, 1.0f));
          //  animator.SetBool("is_run", false);
            animateStatus = "idle";
            enemy = team.enemyTeam.teamFlag;
        }

        //find nearest enemy
        public void findEnemy(){
            float distance = float.MaxValue;
            foreach (GameObject enemyFind in team.enemyTeam.soldiers){
                float tempDistance = Vector3.Distance(transform.position, enemyFind.transform.position);
                if (tempDistance < AttackRange){
                    distance = tempDistance;
                    enemy = enemyFind;
                }
            }
        }
        private void UpdateSoldier()
        {
            if (enemy != null)
            {
            
                Vector3 myPosition = transform.position;
                float distance = Vector3.Distance(enemy.transform.position, myPosition) ;
                //print("distance: " + distance);
                if (distance > AttackRange+AttackDistance)
                {
                    //Lerp to enemy flag ajust to frame rate
                    transform.position = Vector3.Lerp(transform.position, enemy.transform.position, Time.deltaTime * SoldierVelocity * (1f / SoldierFrameRate));
                    float y = Terrain.activeTerrain.SampleHeight(transform.position);
                    transform.position = new Vector3(transform.position.x, y, transform.position.z);
                    transform.rotation = Quaternion.LookRotation(enemy.transform.position - transform.position);
                    isWalking = true;
                }
                else
                {
                    isWalking = false;
                }

            }

            if (isWalking)
            {
                if (animateStatus != "walking")
                {
                    animator.SetBool("is_run", true);
                    animator.SetBool("is_ataka", false);
                    animator.SetBool("is_hi", false);
                    animateStatus = "walking";
                }
            }
            else
            {
                if (animateStatus != "idle")
                {
                    animator.SetBool("is_run", false);
                    animator.SetBool("is_ataka", false);
                    animator.SetBool("is_hi", true);
                    animateStatus = "idle";
                }
            }

        }

    }
}





