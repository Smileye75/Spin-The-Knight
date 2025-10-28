using System;

[Serializable]
public class PlayerSaveData
{
    public int coins;
    public int lives;

    public bool shieldUnlocked;
    public bool heavyAttackUnlocked;
    public bool jumpAttackUnlocked;
    public bool rollJumpUnlocked;
}