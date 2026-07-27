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
public class NPCController : MonoBehaviour, IDamageable
{
    [SerializeField] private Animator anim;

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
    public float hostileThreshold = 0.50f;
    public float shootCheckInterval = 3f;

    [Header("Player Target")]
    [Tooltip("Transform del player. Se lasciato vuoto viene cercato automaticamente tramite il Tag 'Player'.")]
    public Transform playerTransform;
    [Range(0f, 1f)] public float playerAnger = 0f;

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
    public float maxCombatDistance = 40f;
    public float repositionCheckInterval = 2f;
    public float combatSpeed = 2.5f;
    [Tooltip("Layer usati per capire se qualcosa blocca la linea di tiro (muri, banco del bar, altri NPC...)")]
    public LayerMask obstacleMask;

    [Header("Movimento - Fuga")]
    public float fleeDistance = 10f;
    public float fleeSpeed = 4f;

    [Header("Sparo - Visuale")]
    [Tooltip("Punto da cui parte il proiettile visivo (es. la canna della pistola). Se vuoto usa la posizione dell'NPC.")]
    public Transform firePoint;
    public float bulletSpeed = 40f;
    [Tooltip("Offset verticale per mirare più o meno al 'busto' del bersaglio invece che ai piedi")]
    public float aimHeightOffset = 1f;
    [Tooltip("Quanto si allarga il tiro quando il colpo va completamente a vuoto (nessuna vittima per errore)")]
    public float totalMissSpread = 1.5f;

    public NPCState State { get; private set; } = NPCState.Idle;

    private Dictionary<NPCController, float> angerTowards = new Dictionary<NPCController, float>();
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
        // anim = GetComponent<Animator>();
        homePosition = transform.position;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        NpcManager.Instance.Register(this);
        ScheduleNextDrink();
        ScheduleNextIdleMove();
    }

    private void OnDestroy()
    {
        if (NpcManager.Instance != null)
            NpcManager.Instance.Unregister(this);
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

        UpdateAnimator();
    }
    private void UpdateAnimator()
    {
        if (anim == null || agent == null) return;

        bool isMoving = agent.isOnNavMesh &&
                        !agent.isStopped &&
                        agent.velocity.magnitude > 0.05f;

        bool isFleeing = State == NPCState.Fleeing;

        anim.SetBool("IsMoving", isMoving);
        anim.SetBool("IsFleeing", isFleeing);
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
        if (anim != null)
            anim.SetTrigger("Drink");
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
        if (!TryGetCombatTarget(out Transform targetTransform, out NPCController npcTarget, out float _)) return;
        if (targetTransform == null) return;
        if (npcTarget != null && !npcTarget.IsAlive) return;

        repositionTimer -= Time.deltaTime;
        if (repositionTimer > 0f) return;
        repositionTimer = repositionCheckInterval;

        float distance = Vector3.Distance(transform.position, targetTransform.position);
        bool hasLineOfSight = HasLineOfSight(targetTransform);

        if (distance < minCombatDistance || distance > maxCombatDistance || !hasLineOfSight)
        {
            MoveToBetterCombatPosition(targetTransform);
        }
    }

    private bool HasLineOfSight(Transform targetTransform)
    {
        Vector3 origin = transform.position + Vector3.up * aimHeightOffset;
        Vector3 targetPos = targetTransform.position + Vector3.up * aimHeightOffset;
        Vector3 dir = targetPos - origin;

        // Esegui il Raycast fino a un soffio prima del bersaglio per non rischiare di colpire il bersaglio stesso
        float distance = dir.magnitude;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, distance - 0.2f, obstacleMask))
        {
            return false; // Qualcosa (parete, tavolo) blocca la visuale
        }
        return true;
    }

    private void MoveToBetterCombatPosition(Transform targetTransform)
    {
        agent.speed = combatSpeed;

        float desiredDistance = Random.Range(minCombatDistance, maxCombatDistance);

        // angolo casuale: invece di andare sempre dritto verso/via dal bersaglio, si sposta di lato
        // per cercare un angolo di tiro migliore (accerchiamento leggero)
        float randomAngle = Random.Range(-60f, 60f);
        Vector3 dirFromTarget = (transform.position - targetTransform.position).normalized;
        if (dirFromTarget == Vector3.zero) dirFromTarget = Random.insideUnitSphere.normalized;
        dirFromTarget = Quaternion.Euler(0f, randomAngle, 0f) * dirFromTarget;

        Vector3 desiredPoint = targetTransform.position + dirFromTarget * desiredDistance;

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
        float heatBonus = 1f + NpcManager.Instance.GetPlayerHeat() * 0.5f;

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
        NpcManager.Instance.IncreasePlayerHeat(NpcManager.Instance.heatGainOnCaught);

        if (playerTransform != null && thief == playerTransform)
        {
            // ti ha visto rubare con i suoi occhi: rabbia massima verso di te
            playerAnger = 1f;
        }
        else
        {
            // fallback: nel caso il "ladro" non sia il player ma un altro NPC
            NPCController thiefNpcController = thief.GetComponent<NPCController>();
            if (thiefNpcController != null) SetAngerMax(thiefNpcController);
        }

        Debug.Log($"{npcName} ti ha beccato con le mani nel sacco!");
    }

    private void AccuseRandomBystander(Vector3 theftPosition)
    {
        List<NPCController> candidates = NpcManager.Instance.GetNPCsNear(theftPosition, noticeTheftWithoutSeeingRadius, exclude: this);
        if (candidates.Count == 0) return;

        NPCController scapegoat = PickWeightedByAlibi(candidates);
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
    private NPCController PickWeightedByAlibi(List<NPCController> candidates)
    {
        List<float> weights = new List<float>(candidates.Count);
        float totalWeight = 0f;

        foreach (var candidate in candidates)
        {
            bool hasAlibi = NpcManager.Instance
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

    private void SpreadRumor(NPCController scapegoat, float sourceAnger)
    {
        List<NPCController> nearby = NpcManager.Instance.GetNPCsNear(transform.position, gossipRadius, exclude: this);
        foreach (var listener in nearby)
        {
            if (listener == scapegoat) continue; // non puoi convincere il sospettato stesso
            listener.ReceiveRumor(scapegoat, sourceAnger, this);
        }
    }

    // Chiamato su un NPC quando un altro NPC vicino sta spargendo un sospetto su qualcuno
    public void ReceiveRumor(NPCController target, float sourceAnger, NPCController gossiper)
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

    public float GetAnger(NPCController target)
    {
        return angerTowards.TryGetValue(target, out float value) ? value : 0f;
    }

    public void IncreaseAnger(NPCController target, float amount)
    {
        if (target == null || target == this) return;
        float current = GetAnger(target);
        angerTowards[target] = Mathf.Clamp01(current + amount);
    }

    public void SetAngerMax(NPCController target)
    {
        if (target == null || target == this) return;
        angerTowards[target] = 1f;
        timeSinceLastIncident = 0f;
    }

    private void DecayAnger()
    {
        List<NPCController> keys = new List<NPCController>(angerTowards.Keys);
        foreach (var key in keys)
        {
            // la rabbia scende più lentamente quanto più sei ubriaco (l'alcol alimenta il rancore)
            float decay = angerDecayPerSecond * (1f - drunkenness * 0.5f) * Time.deltaTime;
            angerTowards[key] = Mathf.Max(0f, angerTowards[key] - decay);
        }

        // la rabbia verso il player decade con la stessa logica
        float playerDecay = angerDecayPerSecond * (1f - drunkenness * 0.5f) * Time.deltaTime;
        playerAnger = Mathf.Max(0f, playerAnger - playerDecay);
    }

    private bool TryGetHighestAnger(out NPCController target, out float amount)
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

    // Sceglie il bersaglio con la rabbia più alta in assoluto, che sia un NPC oppure il player.
    // npcTarget torna null quando il bersaglio scelto è il player.
    private bool TryGetCombatTarget(out Transform targetTransform, out NPCController npcControllerTarget, out float amount)
    {
        TryGetHighestAnger(out npcControllerTarget, out amount);
        targetTransform = npcControllerTarget != null ? npcControllerTarget.transform : null;

        if (playerTransform != null && playerAnger > amount)
        {
            amount = playerAnger;
            npcControllerTarget = null;
            targetTransform = playerTransform;
        }

        return targetTransform != null;
    }

    // ---------------- ESCALATION ----------------

    private void EvaluateEscalation()
    {
        shootCheckTimer -= Time.deltaTime;
        if (shootCheckTimer > 0f) return;
        shootCheckTimer = shootCheckInterval;

        if (!TryGetCombatTarget(out Transform targetTransform, out NPCController npcTarget, out float highestAnger)) return;

        if (highestAnger >= hostileThreshold)
        {
            TryEscalateToViolence(targetTransform, npcTarget, highestAnger);
        }
        else if (npcTarget != null && highestAnger >= accusationThreshold && State != NPCState.Accusing)
        {
            // l'accusa aperta ha senso solo verso un altro avventore, non verso il player
            Accuse(npcTarget);
        }
    }

    private void Accuse(NPCController target)
    {
        SetState(NPCState.Accusing);
        timeSinceLastIncident = 0f;
        Debug.Log($"{npcName} accusa apertamente {target.npcName}!");
        // qui puoi far partire dialoghi / animazioni di accusa; magari il bersaglio
        // può provare a calmarlo con una battuta (skill check) riducendo un po' la rabbia
    }

    private void TryEscalateToViolence(Transform targetTransform, NPCController npcControllerTarget, float angerLevel)
    {
        float distance = Vector3.Distance(transform.position, targetTransform.position);

        // Se la visuale è ostruita o la distanza è davvero eccessiva, riposizionati
        if (distance > maxCombatDistance + 2f || !HasLineOfSight(targetTransform))
        {
            SetState(NPCState.Hostile);
            return;
        }

        // Calcolo probabilità sparatoria
        float shootChance = angerLevel * 0.6f
                           + drunkenness * 0.2f
                           + aggression * 0.2f
                           + courage * 0.1f;

        // Se la rabbia verso il bersaglio è massima (1.0), spara sempre
        if (angerLevel >= 0.60f || Random.value < shootChance)
        {
            ShootAt(targetTransform, npcControllerTarget);
        }
        else
        {
            SetState(NPCState.Hostile);
        }
    }
    // ---------------- COMBATTIMENTO ----------------

    // Spara un proiettile puramente visivo (VFX) verso un punto già deciso dalla logica di gioco.
    // Il danno NON dipende da questo: è già stato risolto prima di chiamarlo.
    private void FireVisualBullet(Vector3 aimPoint)
    {
        if (BulletManager.Instance == null) return;

        Transform spawn = firePoint != null ? firePoint : transform;
        Bullet bullet = BulletManager.Instance.GetObject();
        if (bullet == null) return;

        bullet.Setup(spawn.forward, bulletSpeed, Bullet.ShooterFaction.NPC, this);
        bullet.transform.position = spawn.position;
        bullet.transform.rotation = Quaternion.identity;

        Vector3 dir = (aimPoint - spawn.position).normalized;
        if (dir == Vector3.zero) dir = spawn.forward;

        // Passo la mia faction (NPC) e me stesso come shooter: se il proiettile colpisce
        // fisicamente un altro NPC, Bullet.cs non gli farà danno (fuoco amico disattivato).
        // Se colpisce il player invece, il danno viene applicato normalmente.
        bullet.Setup(dir, bulletSpeed, Bullet.ShooterFaction.NPC, this);
    }

    // npcTarget è null quando il bersaglio è il player
    private void ShootAt(Transform targetTransform, NPCController npcControllerTarget)
    {
        SetState(NPCState.Combat);
        timeSinceLastIncident = 0f;

        if (anim != null)
            anim.SetTrigger("Shoot");

        AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.shootSound, transform.position);
        bool targetIsPlayer = npcControllerTarget == null;
        string targetName = targetIsPlayer ? "il player" : npcControllerTarget.npcName;

        // più sei ubriaco, più è probabile che il colpo vada a vuoto (o peggio, colpisca qualcun altro)
        float missChance = Mathf.Clamp01(drunkenness * 0.6f - courage * 0.15f);

        if (Random.value < missChance)
        {
            NPCController accidentalVictim = NpcManager.Instance
                .GetRandomNearby(transform.position, strayBulletRadius, this, npcControllerTarget);

            if (accidentalVictim != null)
            {
                Debug.Log($"{npcName} spara a {targetName} ma, ubriaco com'è, colpisce {accidentalVictim.npcName} per sbaglio!");
                FireVisualBullet(accidentalVictim.transform.position + Vector3.up * aimHeightOffset);
                accidentalVictim.OnShotBy(this);
                NpcManager.Instance.BroadcastShooting(this, accidentalVictim, transform.position);
            }
            else
            {
                Debug.Log($"{npcName} spara a {targetName} ma manca completamente il colpo!");
                Vector3 missOffset = Random.insideUnitSphere * totalMissSpread;
                missOffset.y = Mathf.Abs(missOffset.y); // evita che il colpo vada a finire sotto il pavimento
                FireVisualBullet(targetTransform.position + Vector3.up * aimHeightOffset + missOffset);
                if (!targetIsPlayer)
                    NpcManager.Instance.BroadcastShooting(this, npcControllerTarget, transform.position);
            }

            return;
        }

        if (targetIsPlayer)
        {
            Debug.Log($"{npcName} ha sparato al player!!");
            // il danno arriva quando il proiettile colpisce fisicamente il player (vedi Bullet.cs -> TryApplyDamage)
            FireVisualBullet(targetTransform.position + Vector3.up * aimHeightOffset);
            // opzionale: se vuoi che gli altri NPC reagiscano vedendo sparare al player,
            // aggiungi un metodo tipo SaloonManager.Instance.BroadcastShootingAtPlayer(this, transform.position);
        }
        else
        {
            Debug.Log($"{npcName} ha sparato a {npcControllerTarget.npcName}!!");
            FireVisualBullet(npcControllerTarget.transform.position + Vector3.up * aimHeightOffset);
            npcControllerTarget.OnShotBy(this);
            NpcManager.Instance.BroadcastShooting(this, npcControllerTarget, transform.position);
        }
    }

    public void OnShotBy(NPCController shooter)
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

    // Implementazione di IDamageable: chiamato da Bullet.cs quando un proiettile ti colpisce fisicamente.
    // shooter è null quando a sparare è stato il player (stessa convenzione usata nel resto dello script).
    public void TakeDamage(NPCController shooter)
    {
        if (shooter != null)
        {
            // colpito da un altro NPC: in teoria non dovrebbe mai succedere fisicamente
            // (il fuoco amico è bloccato in Bullet.cs), ma resta gestito qui per coerenza/sicurezza
            OnShotBy(shooter);
        }
        else
        {
            OnShotByPlayer();
        }
    }

    private void OnShotByPlayer()
    {
        if (!IsAlive) return;
        NpcManager.Instance.BroadcastPlayerShooting(this, transform.position);
        Die();
    }


    // Chiamato da SaloonManager quando un NPC vicino assiste a una sparatoria
    public void OnWitnessShooting(NPCController shooter, NPCController victim)
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

    // Chiamato da SaloonManager quando un NPC vicino assiste a una sparatoria del PLAYER
    public void OnWitnessPlayerShooting(NPCController victim)
    {
        if (!IsAlive) return;
        if (victim == this) return; // gestito da TakeDamage/OnShotByPlayer

        timeSinceLastIncident = 0f;

        // vedere il player sparare a un innocente fa scattare rabbia diretta verso il player
        float outrage = Random.Range(0.3f, 0.6f) * (1f + loyalty);
        playerAnger = Mathf.Clamp01(playerAnger + outrage);

        // stessa logica di paura già presente in OnWitnessShooting
        if (courage < 0.3f && Random.value > courage)
        {
            lastThreatPosition = playerTransform != null ? playerTransform.position : transform.position;
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
        if (anim != null)
            anim.SetTrigger("Die");
        AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.dieSound, transform.position);
        NpcManager.Instance.SetHeat(1f);
    }

    // ---------------- CALMA DOPO LA TEMPESTA ----------------

    private void EvaluateCalmDown()
    {
        if (State == NPCState.Idle || State == NPCState.Drinking || State == NPCState.Chatting || State == NPCState.Dead)
            return;

        if (timeSinceLastIncident < calmDownTime) return;

        TryGetCombatTarget(out _, out _, out float highestAnger);
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
