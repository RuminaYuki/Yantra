using Kogetsu.Library.DesignPatternCore;
using UnityEngine;

public record struct YantCastEvent(ShapeMatchResult Result, GameObject YantarIns) : IEvent;
