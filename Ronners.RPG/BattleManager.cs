using System.Text;
using System.Text.Json;

namespace Ronners.RPG;

public class BattleManager
{
    public List<Combatant> Players {get;set;}
    public List<Combatant> Monsters {get;set;}
    public List<Weapon> Weapons {get;set;}


    private string PlayerFilePath;
    private string MonsterFilePath;
    private string WeaponFilePath;

    public BattleManager (string playerFilePath, string monsterFilePath, string weaponFilePath)
    {
        PlayerFilePath = playerFilePath;
        MonsterFilePath = monsterFilePath;
        WeaponFilePath = weaponFilePath;
    }

    public BattleManager(List<Combatant> players, List<Combatant> enemies, List<Weapon> weapons)
    {
        Players = players;
        Monsters = enemies;
        Weapons = weapons;
    }

    public void LoadData()
    {
        LoadMonsters(MonsterFilePath);
        LoadPlayers(PlayerFilePath);
        LoadWeapons(WeaponFilePath);
    }

    public void SaveData()
    {
        SaveMonsters(MonsterFilePath);
        SavePlayers(PlayerFilePath);
        SaveWeapons(WeaponFilePath);
    }

    private void SaveMonsters(string filePath)
    {
        var json = JsonSerializer.Serialize(Monsters, new JsonSerializerOptions(){WriteIndented=true});
        File.WriteAllText(filePath,json,new UTF8Encoding(false));
    }
    private void SavePlayers(string filePath)
    {
        var json = JsonSerializer.Serialize(Players, new JsonSerializerOptions(){WriteIndented=true});
        File.WriteAllText(filePath,json,new UTF8Encoding(false));
    }
    private void SaveWeapons(string filePath)
    {
        var json = JsonSerializer.Serialize(Weapons, new JsonSerializerOptions(){WriteIndented=true});
        File.WriteAllText(filePath,json,new UTF8Encoding(false));
    }
    private void LoadMonsters(string filePath)
    {
        var json = string.Empty;

        if(!File.Exists(filePath))
        {
            json = JsonSerializer.Serialize(new List<Combatant>(),new JsonSerializerOptions(){WriteIndented =true});
            File.WriteAllText(filePath,json,new UTF8Encoding(false));
        }

        json = File.ReadAllText(filePath,new UTF8Encoding(false));
        Monsters = JsonSerializer.Deserialize<List<Combatant>>(json);
}

    private void LoadPlayers(string filePath)
    {
        var json = string.Empty;

        if(!File.Exists(filePath))
        {
            json = JsonSerializer.Serialize(new List<Combatant>(),new JsonSerializerOptions(){WriteIndented =true});
            File.WriteAllText(filePath,json,new UTF8Encoding(false));
        }

        json = File.ReadAllText(filePath,new UTF8Encoding(false));
        Players = JsonSerializer.Deserialize<List<Combatant>>(json);
    }
    
    private void LoadWeapons(string filePath)
    {
        var json = string.Empty;

        if(!File.Exists(filePath))
        {
            json = JsonSerializer.Serialize(new List<Weapon>(),new JsonSerializerOptions(){WriteIndented =true});
            File.WriteAllText(filePath,json,new UTF8Encoding(false));
        }

        json = File.ReadAllText(filePath,new UTF8Encoding(false));
        Weapons = JsonSerializer.Deserialize<List<Weapon>>(json);
    }

    public void AddPlayer(ulong id, string name)
    {
        var player = new Combatant(1,1,1,1,1,1,1).SetName(name);
        player.UserID = id;

        Players.Add(player);
        SavePlayers(PlayerFilePath);
    }

    public Combatant? GetPlayerByID(ulong id)
    {
        return Players.Find(x=> x.UserID == id);
    }
}