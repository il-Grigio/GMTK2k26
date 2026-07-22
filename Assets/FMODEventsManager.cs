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
    
    [field: Header("Sword SFX")]
    [field: SerializeField]
    public EventReference swordSFX { get; private set; }
}
