using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum NPCState
{
    Idle,
    Drinking,
    Chatting,
    Suspicious,
    Accusing,
    Hostile,
    Combat,
    Fleeing,
    Dead
}

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NavMeshAgent))]
public class SaloonNPC : MonoBehaviour
{
    [Header("Identità")]
    public string npcName = "Cowboy";

    [Header("Percezione")]
    [Range(0f, 1f)] public float baseAwareness = 0.5f; // capacità di notare il furto
    public float perceptionRadius = 6f;
    public float noticeTheftWithoutSeeingRadius = 10f; // raggio entro cui "sente" che qualcosa non va anche se non vede il ladro

    [Header("Ubriachezza")]
    [Range(0f, 1f)] public float drunkenness = 0f;
    public float drunkennessGainPerDrink = 0.15f;
    public float soberingRate = 0.01f; // quanto si smaltisce l'alcol al secondo
    public float drinkIntervalMin = 8f;
    public float drinkIntervalMax = 20f;

    [Header("Personalità")]
    [Range(0f, 1f)] public float aggression = 0.4f;  // tendenza a passare dalle parole ai fatti
    [Range(0f, 1f)] public float courage = 0.5f;     // quanto è disposto a rischiare sparando
    [Range(0f, 1f)] public float loyalty = 0.3f;     // quanto si infuria se un amico viene colpito
    [Range(0f, 1f)] public float gullibility = 0.5f; // quanto crede facilmente alle voci altrui

    [Header("Rabbia")]
    public float angerDecayPerSecond = 0.02f;
    public float accusationThreshold = 0.5f;
    public float hostileThreshold = 0.75f;
    public float shootCheckInterval = 3f;

    [Header("Alibi")]
    [Tooltip("Se un NPC ha un altro NPC entro questo raggio, si considera 'visto con qualcuno' e ha meno probabilità di essere incolpato")]
    public float alibiCheckRadius = 2.5f;
    [Range(0f, 1f)] public float alibiWeightPenalty = 0.15f; // peso residuo nella scelta random se ha un alibi

    [Header("Voce che gira")]
    public float gossipRadius = 8f;
    [Range(0f, 1f)] public float gossipTransferFactor = 0.5f; // quanta rabbia (in %) viene trasferita a chi crede alla voce

    [Header("Colpo a vuoto")]
    public float strayBulletRadius = 5f; // raggio in cui può finire un proiettile vagante

    [Header("Calma dopo la tempesta")]
    public float calmDownTime = 20f; // secondi senza incidenti prima di tornare tranquilli
    public float calmAngerThreshold = 0.2f; // sotto questa soglia la rabbia residua non blocca la calma

    [Header("Movimento - Tranquillo")]
    [Tooltip("Raggio entro cui l'NPC gironzola quando è tranquillo, intorno al suo 'posto' iniziale")]
    public float wanderRadius = 3f;
    public float idleMoveIntervalMin = 5f;
    public float idleMoveIntervalMax = 15f;
    public float wanderSpeed = 1.2f;

    [Header("Movimento - Combattimento")]
    [Tooltip("Distanza minima e massima che l'NPC cerca di mantenere dal bersaglio quando è ostile")]
    public float minCombatDistance = 3f;
    public float maxCombatDistance = 8f;
    public float repositionCheckInterval = 2f;
    public float combatSpeed = 2.5f;
    [Tooltip("Layer usati per capire se qualcosa blocca la linea di tiro (muri, banco del bar, altri NPC...)")]
    public LayerMask obstacleMask;

    [Header("Movimento - Fuga")]
    public float fleeDistance = 10f;
    public float fleeSpeed = 4f;

    public NPCState State { get; private set; } = NPCState.Idle;

    private Dictionary<SaloonNPC, float> angerTowards = new Dictionary<SaloonNPC, float>();
    private float drinkTimer;
    private float shootCheckTimer;
    private float timeSinceLastIncident;
    public bool IsAlive { get; private set; } = true;

    private NavMeshAgent agent;
    private Vector3 homePosition;
    private float idleMoveTimer;
    private float repositionTimer;
    private Vector3 lastThreatPosition;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        homePosition = transform.position;

        SaloonManager.Instance.Register(this);
        ScheduleNextDrink();
        ScheduleNextIdleMove();
    }

    private void OnDestroy()
    {
        if (SaloonManager.Instance != null)
            SaloonManager.Instance.Unregister(this);
    }

    private void Update()
    {
        if (!IsAlive) return;

        HandleDrinking();
        DecayAnger();
        timeSinceLastIncident += Time.deltaTime;
        EvaluateEscalation();
        EvaluateCalmDown();

        HandleIdleMovement();
        HandleCombatRepositioning();
        HandleFleeing();
    }

    // ---------------- DRINKING ----------------

    private void ScheduleNextDrink()
    {
        drinkTimer = Random.Range(drinkIntervalMin, drinkIntervalMax);
    }

    private void HandleDrinking()
    {
        drinkTimer -= Time.deltaTime;
        if (drinkTimer <= 0f)
        {
            Drink();
            ScheduleNextDrink();
        }

        // smaltimento lento dell'alcol nel tempo
        drunkenness = Mathf.Max(0f, drunkenness - soberingRate * Time.deltaTime);
    }

    private void Drink()
    {
        drunkenness = Mathf.Clamp01(drunkenness + drunkennessGainPerDrink);
        if (State == NPCState.Idle || State == NPCState.Chatting)
            SetState(NPCState.Drinking);
        // qui puoi far partire l'animazione "beve dal bicchiere"
    }

    // ---------------- MOVIMENTO: TRANQUILLO ----------------

    private void ScheduleNextIdleMove()
    {
        idleMoveTimer = Random.Range(idleMoveIntervalMin, idleMoveIntervalMax);
    }

    private bool IsCalmState()
    {
        return State == NPCState.Idle || State == NPCState.Drinking
            || State == NPCState.Chatting || State == NPCState.Suspicious
            || State == NPCState.Accusing;
    }

    private void HandleIdleMovement()
    {
        if (!IsCalmState()) return;

        // se sta ancora camminando verso la destinazione precedente, aspetta
        if (agent.pathPending || agent.remainingDistance > agent.stoppingDistance) return;

        idleMoveTimer -= Time.deltaTime;
        if (idleMoveTimer <= 0f)
        {
            agent.speed = wanderSpeed;

            Vector3 randomPoint = homePosition + Random.insideUnitSphere * wanderRadius;
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            ScheduleNextIdleMove();
        }
    }

    // ---------------- MOVIMENTO: RIPOSIZIONAMENTO IN COMBATTIMENTO ----------------

    private void HandleCombatRepositioning()
    {
        if (State != NPCState.Hostile && State != NPCState.Combat) return;
        if (!TryGetHighestAnger(out SaloonNPC target, out float _)) return;
        if (target == null || !target.IsAlive) return;

        repositionTimer -= Time.deltaTime;
        if (repositionTimer > 0f) return;
        repositionTimer = repositionCheckInterval;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        bool hasLineOfSight = HasLineOfSight(target);

        if (distance < minCombatDistance || distance > maxCombatDistance || !hasLineOfSight)
        {
            MoveToBetterCombatPosition(target);
        }
    }

    private bool HasLineOfSight(SaloonNPC target)
    {
        Vector3 origin = transform.position + Vector3.up;
        Vector3 targetPos = target.transform.position + Vector3.up;
        Vector3 dir = targetPos - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dir.magnitude, obstacleMask))
        {
            return false; // qualcosa (banco, tavolo, un altro cliente...) blocca la visuale
        }
        return true;
    }

    private void MoveToBetterCombatPosition(SaloonNPC target)
    {
        agent.speed = combatSpeed;

        float desiredDistance = Random.Range(minCombatDistance, maxCombatDistance);

        // angolo casuale: invece di andare sempre dritto verso/via dal bersaglio, si sposta di lato
        // per cercare un angolo di tiro migliore (accerchiamento leggero)
        float randomAngle = Random.Range(-60f, 60f);
        Vector3 dirFromTarget = (transform.position - target.transform.position).normalized;
        if (dirFromTarget == Vector3.zero) dirFromTarget = Random.insideUnitSphere.normalized;
        dirFromTarget = Quaternion.Euler(0f, randomAngle, 0f) * dirFromTarget;

        Vector3 desiredPoint = target.transform.position + dirFromTarget * desiredDistance;

        if (NavMesh.SamplePosition(desiredPoint, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // ---------------- MOVIMENTO: FUGA ----------------

    private void HandleFleeing()
    {
        if (State != NPCState.Fleeing) return;
        if (agent.pathPending || agent.remainingDistance > agent.stoppingDistance) return;

        agent.speed = fleeSpeed;

        Vector3 fleeDir = (transform.position - lastThreatPosition).normalized;
        if (fleeDir == Vector3.zero) fleeDir = Random.insideUnitSphere.normalized;

        Vector3 fleePoint = transform.position + fleeDir * fleeDistance;

        if (NavMesh.SamplePosition(fleePoint, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // ---------------- FURTO / PERCEZIONE ----------------

    // Chiamato dal PlayerTheft/StealableItem quando il player ruba un oggetto nelle vicinanze
    public void OnTheftAttempt(Transform thief, float stealthValue, Vector3 theftPosition)
    {
        if (!IsAlive) return;

        float distance = Vector3.Distance(transform.position, theftPosition);
        if (distance > noticeTheftWithoutSeeingRadius) return;

        // la reputazione del player (quante volte è già stato beccato) rende tutti più all'erta
        float heatBonus = 1f + SaloonManager.Instance.GetPlayerHeat() * 0.5f;

        // probabilità di vedere DIRETTAMENTE il ladro
        float distanceFactor = Mathf.Clamp01(1f - (distance / perceptionRadius));
        float drunkPenalty = 1f - (drunkenness * 0.7f); // più ubriaco = meno percettivo
        float detectChance = baseAwareness * distanceFactor * drunkPenalty * (1f - stealthValue) * heatBonus;

        if (distance <= perceptionRadius && Random.value < detectChance)
        {
            // Ti ha visto rubare: reazione diretta verso il player
            DirectlyCatchThief(thief);
            return;
        }

        // Non ha visto il ladro, ma nota che qualcosa è sparito / percepisce il disturbo
        float noticeChance = baseAwareness * 0.6f * (1f - drunkenness * 0.4f) * heatBonus;
        if (Random.value < noticeChance)
        {
            AccuseRandomBystander(theftPosition);
        }
    }

    private void DirectlyCatchThief(Transform thief)
    {
        SetState(NPCState.Hostile);
        timeSinceLastIncident = 0f;
        SaloonManager.Instance.IncreasePlayerHeat(SaloonManager.Instance.heatGainOnCaught);
        // qui puoi gestire l'allarme diretto contro il player (fuori scope di questo script,
        // collegalo al tuo PlayerController / sistema di combattimento)
        Debug.Log($"{npcName} ti ha beccato con le mani nel sacco!");
    }

    private void AccuseRandomBystander(Vector3 theftPosition)
    {
        List<SaloonNPC> candidates = SaloonManager.Instance.GetNPCsNear(theftPosition, noticeTheftWithoutSeeingRadius, exclude: this);
        if (candidates.Count == 0) return;

        SaloonNPC scapegoat = PickWeightedByAlibi(candidates);
        if (scapegoat == null) return;

        // più sei ubriaco, più la rabbia che ti si accende è irrazionale e forte
        float angerGain = Random.Range(0.15f, 0.35f) * (1f + drunkenness * 0.5f);
        IncreaseAnger(scapegoat, angerGain);
        timeSinceLastIncident = 0f;

        Debug.Log($"{npcName} sospetta di {scapegoat.npcName} per il furto (rabbia: {GetAnger(scapegoat):0.00})");

        // la voce gira: chi è vicino a me e mi crede si convince anche lui
        SpreadRumor(scapegoat, angerGain);
    }

    // Scelta pesata: chi ha un "alibi" (era vicino a un altro NPC, quindi presumibilmente visto con lui)
    // ha molto meno probabilità di essere scelto come capro espiatorio
    private SaloonNPC PickWeightedByAlibi(List<SaloonNPC> candidates)
    {
        List<float> weights = new List<float>(candidates.Count);
        float totalWeight = 0f;

        foreach (var candidate in candidates)
        {
            bool hasAlibi = SaloonManager.Instance
                .GetNPCsNear(candidate.transform.position, alibiCheckRadius, exclude: candidate).Count > 0;

            float weight = hasAlibi ? alibiWeightPenalty : 1f;
            weights.Add(weight);
            totalWeight += weight;
        }

        if (totalWeight <= 0f) return candidates[Random.Range(0, candidates.Count)];

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative) return candidates[i];
        }

        return candidates[candidates.Count - 1];
    }

    // ---------------- VOCE CHE GIRA ----------------

    private void SpreadRumor(SaloonNPC scapegoat, float sourceAnger)
    {
        List<SaloonNPC> nearby = SaloonManager.Instance.GetNPCsNear(transform.position, gossipRadius, exclude: this);
        foreach (var listener in nearby)
        {
            if (listener == scapegoat) continue; // non puoi convincere il sospettato stesso
            listener.ReceiveRumor(scapegoat, sourceAnger, this);
        }
    }

    // Chiamato su un NPC quando un altro NPC vicino sta spargendo un sospetto su qualcuno
    public void ReceiveRumor(SaloonNPC target, float sourceAnger, SaloonNPC gossiper)
    {
        if (!IsAlive || target == this) return;

        // più sei ubriaco e più sei credulone, più bevi la voce senza discuterla
        float believeChance = gullibility * 0.6f + drunkenness * 0.4f;
        if (Random.value > believeChance) return;

        float transferredAnger = sourceAnger * gossipTransferFactor;
        IncreaseAnger(target, transferredAnger);
        timeSinceLastIncident = 0f;

        Debug.Log($"{npcName} sente {gossiper.npcName} parlare male di {target.npcName} e comincia a crederci ({GetAnger(target):0.00})");
    }

    // ---------------- RABBIA ----------------

    public float GetAnger(SaloonNPC target)
    {
        return angerTowards.TryGetValue(target, out float value) ? value : 0f;
    }

    public void IncreaseAnger(SaloonNPC target, float amount)
    {
        if (target == null || target == this) return;
        float current = GetAnger(target);
        angerTowards[target] = Mathf.Clamp01(current + amount);
    }

    public void SetAngerMax(SaloonNPC target)
    {
        if (target == null || target == this) return;
        angerTowards[target] = 1f;
        timeSinceLastIncident = 0f;
    }

    private void DecayAnger()
    {
        List<SaloonNPC> keys = new List<SaloonNPC>(angerTowards.Keys);
        foreach (var key in keys)
        {
            // la rabbia scende più lentamente quanto più sei ubriaco (l'alcol alimenta il rancore)
            float decay = angerDecayPerSecond * (1f - drunkenness * 0.5f) * Time.deltaTime;
            angerTowards[key] = Mathf.Max(0f, angerTowards[key] - decay);
        }
    }

    private bool TryGetHighestAnger(out SaloonNPC target, out float amount)
    {
        target = null;
        amount = 0f;
        foreach (var pair in angerTowards)
        {
            if (pair.Value > amount)
            {
                amount = pair.Value;
                target = pair.Key;
            }
        }
        return target != null;
    }

    // ---------------- ESCALATION ----------------

    private void EvaluateEscalation()
    {
        shootCheckTimer -= Time.deltaTime;
        if (shootCheckTimer > 0f) return;
        shootCheckTimer = shootCheckInterval;

        if (!TryGetHighestAnger(out SaloonNPC angriestTarget, out float highestAnger)) return;

        if (highestAnger >= hostileThreshold)
        {
            TryEscalateToViolence(angriestTarget, highestAnger);
        }
        else if (highestAnger >= accusationThreshold && State != NPCState.Accusing)
        {
            Accuse(angriestTarget);
        }
    }

    private void Accuse(SaloonNPC target)
    {
        SetState(NPCState.Accusing);
        timeSinceLastIncident = 0f;
        Debug.Log($"{npcName} accusa apertamente {target.npcName}!");
        // qui puoi far partire dialoghi / animazioni di accusa; magari il bersaglio
        // può provare a calmarlo con una battuta (skill check) riducendo un po' la rabbia
    }

    private void TryEscalateToViolence(SaloonNPC target, float angerLevel)
    {
        // se non è ancora a distanza/angolo di tiro buono, prima si riposiziona (gestito in HandleCombatRepositioning)
        // e rimanda la decisione di sparare al prossimo check
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance < minCombatDistance || distance > maxCombatDistance || !HasLineOfSight(target))
        {
            SetState(NPCState.Hostile);
            return;
        }

        // probabilità di sparare = combinazione di rabbia, ubriachezza, aggressività, coraggio
        float shootChance = angerLevel * 0.5f
                           + drunkenness * 0.3f
                           + aggression * 0.15f
                           + courage * 0.15f;

        shootChance = Mathf.Clamp01(shootChance);

        if (Random.value < shootChance)
        {
            ShootAt(target);
        }
        else
        {
            SetState(NPCState.Hostile);
        }
    }

    // ---------------- COMBATTIMENTO ----------------

    private void ShootAt(SaloonNPC intendedTarget)
    {
        SetState(NPCState.Combat);
        timeSinceLastIncident = 0f;

        // più sei ubriaco, più è probabile che il colpo vada a vuoto (o peggio, colpisca qualcun altro)
        float missChance = Mathf.Clamp01(drunkenness * 0.6f - courage * 0.15f);

        if (Random.value < missChance)
        {
            SaloonNPC accidentalVictim = SaloonManager.Instance
                .GetRandomNearby(transform.position, strayBulletRadius, this, intendedTarget);

            if (accidentalVictim != null)
            {
                Debug.Log($"{npcName} spara a {intendedTarget.npcName} ma, ubriaco com'è, colpisce {accidentalVictim.npcName} per sbaglio!");
                accidentalVictim.OnShotBy(this);
                SaloonManager.Instance.BroadcastShooting(this, accidentalVictim, transform.position);
            }
            else
            {
                Debug.Log($"{npcName} spara a {intendedTarget.npcName} ma manca completamente il colpo!");
                SaloonManager.Instance.BroadcastShooting(this, intendedTarget, transform.position);
            }

            return;
        }

        Debug.Log($"{npcName} ha sparato a {intendedTarget.npcName}!!");
        intendedTarget.OnShotBy(this);
        SaloonManager.Instance.BroadcastShooting(this, intendedTarget, transform.position);
    }

    public void OnShotBy(SaloonNPC shooter)
    {
        if (!IsAlive) return;

        // se sopravvive, la rabbia verso lo sparatore va al massimo: si ricorderà e reagirà
        SetAngerMax(shooter);
        SetState(NPCState.Hostile);

        // qui collega la tua logica di danno/morte reale
        // esempio semplificato:
        bool died = Random.value < 0.5f;
        if (died)
        {
            Die();
        }
    }

    // Chiamato da SaloonManager quando un NPC vicino assiste a una sparatoria
    public void OnWitnessShooting(SaloonNPC shooter, SaloonNPC victim)
    {
        if (!IsAlive || shooter == this) return;

        if (victim == this) return; // gestito da OnShotBy

        timeSinceLastIncident = 0f;

        // se la vittima non aveva colpe evidenti, i presenti si indignano con lo sparatore
        float outrage = Random.Range(0.3f, 0.6f) * (1f + loyalty);
        IncreaseAnger(shooter, outrage);

        // paura: un NPC poco coraggioso potrebbe scappare invece di intervenire
        if (courage < 0.3f && Random.value > courage)
        {
            lastThreatPosition = shooter.transform.position;
            SetState(NPCState.Fleeing);
        }
    }

    private void Die()
    {
        IsAlive = false;
        SetState(NPCState.Dead);
        agent.isStopped = true;
        Debug.Log($"{npcName} è morto.");
        // disabilita collider, avvia ragdoll/animazione morte, ecc.
    }

    // ---------------- CALMA DOPO LA TEMPESTA ----------------

    private void EvaluateCalmDown()
    {
        if (State == NPCState.Idle || State == NPCState.Drinking || State == NPCState.Chatting || State == NPCState.Dead)
            return;

        if (timeSinceLastIncident < calmDownTime) return;

        TryGetHighestAnger(out _, out float highestAnger);
        if (highestAnger > calmAngerThreshold) return;

        // nessun incidente da un po', la rabbia residua è bassa: si torna tranquilli
        SetState(NPCState.Chatting);

        // torna tranquillamente verso il proprio posto al bancone/tavolo
        agent.speed = wanderSpeed;
        if (NavMesh.SamplePosition(homePosition, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        Debug.Log($"{npcName} si è calmato e torna a bere tranquillo.");
    }

    private void SetState(NPCState newState)
    {
        if (State == newState) return;
        State = newState;
        // qui puoi agganciare Animator.SetTrigger/SetInteger in base allo stato
    }
}
