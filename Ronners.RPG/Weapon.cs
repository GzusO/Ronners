namespace Ronners.RPG;

public class Weapon : Equipment
{
    public double AttackSpeed {get;set;}
    public int MinDamage {get;set;}
    public int MaxDamage {get;set;}
    public string ActionWord {get;set;}

    public Weapon (int minDamage=1, int maxDamage=5, double attackSpeed=1.0) : base("","",EquipmentType.Weapon)
    {
        AttackSpeed = attackSpeed;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
        ActionWord = "";
    }
    
    public Weapon (string name, string description, int minDamage, int maxDamage, double attackSpeed, string actionWord) : base(name, description, EquipmentType.Weapon)
    {
        AttackSpeed = attackSpeed;
        MinDamage = minDamage;
        MaxDamage = maxDamage;
        ActionWord = actionWord;
    }



}