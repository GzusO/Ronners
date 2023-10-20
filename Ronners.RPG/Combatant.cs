namespace Ronners.RPG;

public class Combatant 
{
    public ulong UserID {get;set;}
    public string Name {get;set;}
    public int Ronners {get;set;}
    public int Objectivity {get;set;}
    public int Normalcy {get;set;}
    public int Nutrition {get;set;}
    public int Erudition {get;set;}
    public int Rapidity {get;set;}
    public int Strength {get;set;}

    public int MinDamage {get{return Math.Min(BaseMinDamage+Strength,MaxDamage);}}
    public int MaxDamage {get{return BaseMaxDamage;}}

    public int BaseMinDamage {get{return HandSlot is null ? 1 : HandSlot.MinDamage;}}
    public int BaseMaxDamage {get{return HandSlot is null ? 1 : HandSlot.MaxDamage;}}

    public int CriticalHitChance {get{return Ronners;}}
    public double CriticalHitMultiplier {get{return 1.5+(.01*Objectivity);}}
    public double AttackSpeed {get{return Rapidity * (HandSlot is null ? 2 : HandSlot.AttackSpeed);}}
    public int MaxHealth {get{return 10*Nutrition;}}
    public int CurrentHealth{get;set;}

    public Weapon? HandSlot {get;set;}
    public long Experience {get;set;} = 0;


    public Combatant(int ronners=1, int objectivity=1, int normalcy=1, int nutrition=1, int erudition=1, int rapidity=1, int strength=1)
    {
        Name = "";
        Ronners = ronners;
        Objectivity = objectivity;
        Normalcy = normalcy;
        Nutrition = nutrition;
        Erudition = erudition;
        Rapidity = rapidity;
        Strength = strength;
        CurrentHealth = MaxHealth;
    }

    public bool DealDamage(int damage)
    {
        CurrentHealth -= damage;

        return CurrentHealth <= 0;
    }

    public Combatant SetName(string name)
    {
        Name = name;
        return this;
    }
    public Combatant SetRonners(int value)
    {
        Ronners = Math.Max(value,1);
        return this;
    }
    public Combatant SetObjectivity(int value)
    {
        Objectivity = Math.Max(value,1);
        return this;
    }
    public Combatant SetNormalcy(int value)
    {
        Normalcy = Math.Max(value,1);
        return this;
    }
    public Combatant SetNutrition(int value)
    {
        Nutrition = Math.Max(value,1);
        return this;
    }
    public Combatant SetErudition(int value)
    {
        Erudition = Math.Max(value,1);
        return this;
    }
    public Combatant SetRapidity(int value)
    {
        Rapidity = Math.Max(value,1);
        return this;
    }
    public Combatant SetStrength(int value)
    {
        Strength = Math.Max(value,1);
        return this;
    }
}
