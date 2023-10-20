namespace Ronners.RPG;

public class BattleManagerTest
{
    [Fact]
    public void LoadData()
    {
        var p = new List<Combatant>()
        {
            new Combatant().SetName("Garret"),
            new Combatant().SetName("Test")
        };
        var m = new List<Combatant>()
        {
            new Combatant(strength:2).SetName("Rat"),
            new Combatant(nutrition:2,strength:2,rapidity:2).SetName("Big Rat")
        };
        var w = new List<Weapon>()
        {
            new Weapon("Stick","sticky",1,5,1.5,"poke"),
            new Weapon("Rusty Nail","rusty",2,2,1.75,"stab")

        };
        var bm = new BattleManager("players.json","monsters.json","weapons.json");
        bm.Players = p;
        bm.Monsters = m;
        bm.Weapons = w;
        bm.SaveData();
    }

}