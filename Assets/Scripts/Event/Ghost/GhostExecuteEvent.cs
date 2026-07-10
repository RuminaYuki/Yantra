using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

public record struct GhostExecuteEvent(Transform Ghost) : IEvent;
