using Kogetsu.Library.DesignPatternCore;

public static class PlayerVoice
{
    public const string Tag = "PlayerSoundEvent";

    // ─── เหตุการณ์ตามสถานที่ ───────────────────────────────────────────
    public const string EnterVillage           = "EnterVillage";
    public const string FindFatherClue         = "FindFatherClue";
    public const string WalkingToVillageCenter = "WalkingToVillageCenter";
    public const string FirstGhostEncounter    = "FirstGhostEncounter";
    public const string EnterYantraHouse       = "EnterYantraHouse";
    public const string FindCave               = "FindCave";
    public const string FindSculptureRoom      = "FindSculptureRoom";
    public const string AfterLeaveYantraHouse  = "AfterLeaveYantraHouse";

    // ─── เหตุการณ์ตามสถานะ (Stats) ────────────────────────────────────
    public const string WhileDrawing    = "WhileDrawing";
    public const string LowHp          = "LowHp";
    public const string StaminaLow      = "StaminaLow";      // เหนื่อยปกติ — stamina ใกล้หมด
    public const string StaminaDepleted = "StaminaDepleted"; // เหนื่อยมากๆ — stamina = 0

    public static void Publish(string line)
    {
        if (EventBus.Instance)
            EventBus.Instance.Publish(new EventNameAndTag(line, Tag));
    }
}
