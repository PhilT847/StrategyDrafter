public class Bow : Weapon
{
    public bool OdysseusBow; //cannot make follow up attacks, and max range is determined by 10% of max HP.

    public override int GetMaxRange()
    {
        if (!OdysseusBow)
        {
            return MaxRange;
        }
        else
        {
            return WeaponOwner.MaxHealth / 10;
        }
    }
}
