using Grigios;
using FMODUnity;
using UnityEngine;

public class FMODEventsManager : Singleton<FMODEventsManager>
{
    [field: Header("Ambience")]
    [field: SerializeField]
    public EventReference ambiance { get; private set; }
    
    [field: Header("Player SFX")]
    [field: SerializeField]
    public EventReference playerFootstepsSFX { get; private set; }

    [field: Header("Music di base")]
    [field: SerializeField] public EventReference baseMusic { get; private set; }
}
