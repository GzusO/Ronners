namespace Ronners.RPG;

public class Equipment 
{
    public int EquipmentID {get;set;}
    public string Name {get;set;}
    public string Description {get;set;}
    public EquipmentType EquipmentType {get;set;}

    public Equipment(string name, string description, EquipmentType equipmentType)
    {
        Name = name;
        Description = description;
        EquipmentType = equipmentType;
    }

}