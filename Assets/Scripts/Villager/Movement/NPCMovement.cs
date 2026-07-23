using UnityEngine;

// VIBE CODATO <3
public class NPCMovement : MonoBehaviour
{

    // Lista statica condivisa da tutti gli NPC in scena
    static System.Collections.Generic.List<NPCMovement> allNPCs =
        new System.Collections.Generic.List<NPCMovement>();

    void OnEnable() => allNPCs.Add(this);
    void OnDisable() => allNPCs.Remove(this);

    public Transform homeZone;
    public Transform player;


    public Transform talkTarget;


    [Header("AI")]
    public float activationDistance = 40f;
    public float thinkInterval = 0.4f;


    [Header("Movement")]
    public float speed = 2f;
    public float patrolRadius = 10f;

    public float talkDistance = 1.5f;
    public float talkDuration = 5f;

    [Header("Obstacle Avoidance")]
    public LayerMask wallLayer;
    public float avoidDistance = 1.2f;
    public float sideStep = 2f;

    [Header("Social")]
    public float talkSearchRadius = 8f;



    enum State
    {
        Patrol,
        GoTalk,
        Talking,
        Return
    }


    State state;


    Vector3 destination;

    float timer;

    float thinkTimer;



    void Start()
    {
        // Se non è stata assegnata una Home Zone in editor,
        // la creiamo automaticamente nel punto di spawn dell'NPC.
        if (homeZone == null)
        {
            GameObject homeZoneObj = new GameObject(gameObject.name + "_HomeZone");
            homeZoneObj.transform.position = transform.position;
            homeZoneObj.transform.parent = transform.parent; // opzionale, tiene pulita la gerarchia
            homeZone = homeZoneObj.transform;
        }

        state = State.Patrol;

        PickRandomPoint();
    }



    void Update()
    {
        // fuori dal raggio del player:

        if (player != null)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    player.position);


            if (distance > activationDistance)
                return;
        }

        // movimento sempre leggero

        Move();

        // pensa ogni tot

        thinkTimer -= Time.deltaTime;

        if (thinkTimer > 0)
            return;

        thinkTimer = thinkInterval;

        Think();

    }



    void Move()
    {
        Vector3 dir = destination - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f)
            return;

        dir.Normalize();

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        // Controlla se davanti c'è un muro
        if (Physics.SphereCast(origin, 0.3f, dir, out RaycastHit hit, avoidDistance, wallLayer))
        {
            Vector3 right = Vector3.Cross(Vector3.up, dir);
            Vector3 left = -right;

            // prova destra
            if (!Physics.SphereCast(origin, 0.3f, right, out _, avoidDistance, wallLayer))
            {
                destination = transform.position + right * sideStep;
            }
            // prova sinistra
            else if (!Physics.SphereCast(origin, 0.3f, left, out _, avoidDistance, wallLayer))
            {
                destination = transform.position + left * sideStep;
            }
            // completamente bloccato
            else
            {
                PickRandomPoint();
            }

            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            8f * Time.deltaTime);
    }





    void Think()
    {

        switch (state)
        {

            case State.Patrol:
                Patrol();
                break;


            case State.GoTalk:
                GoTalk();
                break;


            case State.Talking:
                Talking();
                break;


            case State.Return:
                ReturnHome();
                break;

        }

    }

    void Patrol()
    {

        if (Vector3.Distance(
            transform.position,
            destination) < 0.5f)
        {
            PickRandomPoint();
        }

    }

    void PickRandomPoint()
    {
        Vector3 random =
            Random.insideUnitSphere *
            patrolRadius;

        random.y = 0;

        destination =
            homeZone.position + random;
    }

    public void StartTalking(
        Transform target)
    {

        talkTarget = target;

        destination = target.position;

        state = State.GoTalk;

    }

    void GoTalk()
    {

        float d =
            Vector3.Distance(
                transform.position,
                talkTarget.position);



        if (d < talkDistance)
        {

            destination =
                transform.position;


            timer = talkDuration;


            state = State.Talking;

        }

    }

    void Talking()
    {
        if (talkTarget != null)
        {
            Vector3 lookDir = talkTarget.position - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDir.normalized),
                    8f * Time.deltaTime);
            }
        }

        timer -= thinkInterval;

        if (timer <= 0)
        {
            // Prova a propagare il pettegolezzo a un altro NPC prima di tornare
            TryStartTalkingWithNearbyNPC();

            // Se non ha trovato nessuno, torna a casa normalmente
            if (state == State.Talking) // non è cambiato stato = nessun NPC trovato
            {
                destination = homeZone.position;
                state = State.Return;
            }
        }
    }
    void ReturnHome()
    {

        if (Vector3.Distance(
            transform.position,
            homeZone.position) < 0.5f)
        {

            state = State.Patrol;


            PickRandomPoint();
            if (Random.value < 0.05f) // 5% di chance ogni volta che raggiunge un punto
                TryStartTalkingWithNearbyNPC();
        }

    }

    public NPCMovement TryStartTalkingWithNearbyNPC()
    {
        // Se sta già parlando o andando a parlare, non fare nulla
        if (state == State.GoTalk || state == State.Talking)
            return null;

        NPCMovement best = null;
        float bestDist = talkSearchRadius;

        foreach (var other in allNPCs)
        {
            if (other == this) continue;
            if (other.state == State.GoTalk || other.state == State.Talking) continue;

            float d = Vector3.Distance(transform.position, other.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = other;
            }
        }

        if (best != null)
        {
            StartTalking(best.transform);
            best.StartTalking(this.transform);
        }

        return best; // null se non ha trovato nessuno
    }
}