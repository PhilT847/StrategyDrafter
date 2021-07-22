public class Lance : Weapon //Lances have low base Hit, but gain bonus Hit when either attacking or defending
{
    public bool PlayerPhaseLance; //if player phase, double hit when initiating. Else, the bonus is given when defending
    public bool DenyEnemyFollowUp; //denies enemy follow up on enemy phase

    public override int GetMaxRange()
    {
        return MaxRange;
    }
}
