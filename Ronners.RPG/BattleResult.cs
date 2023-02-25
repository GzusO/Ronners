namespace Ronners.RPG;

public class BattleResult
{
    public IEnumerable<string> Logs {get;set;}
    public Combatant Player {get;set;}
    public Combatant Enemy {get;set;}

    public BattleResult(IEnumerable<string> logs, Combatant player, Combatant enemy)
    {
        Logs = logs;
        Player = player;
        Enemy = enemy;
    }
}
