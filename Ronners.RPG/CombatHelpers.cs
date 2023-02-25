namespace Ronners.RPG;

public static class CombatHelpers
{
    public static int BaseAttackSpeed = 100;

    public static Weapon GetRandomWeapon(IRandomGenerator rng, string choice)
    {
        List<Weapon> weapons = new List<Weapon>()
        {
            new Weapon("Stick","a stick.",1,5,1.5, "bash"),
            new Weapon("Rusty Nail","pointy",2,2,1.75, "stab"),
            new Weapon("Fists","your hands fool.",1,1,2.0, "punch")
        };

        var found = weapons.Where(x=> x.Name == choice);
        if(found.Count() > 0)
            return found.First();


        return weapons[rng.Next(weapons.Count)];
    }

    public static int CalculateCriticalCount(IRandomGenerator rng, int criticalHitChance)
    {
        //Make Negative Critical chance positve but return a negative count.
        int chanceMultiplier = 1;
        if(criticalHitChance < 0)
        {
            chanceMultiplier = -1;
            criticalHitChance *=-1;
        }

        int criticalCount = criticalHitChance / 100;  //Guranteed Critical Hits
        int chanceToCrit = criticalHitChance % 100; 

        if(rng.Next(0,100) < chanceToCrit)
            criticalCount++;

        return criticalCount*chanceMultiplier;
    }

    //Critical Multiplier raised to the power of number of crits.
    public static double CalculateCriticalMultiplier(double baseCritMultiplier, int critCount)
    {
        return Math.Pow(baseCritMultiplier,critCount);
    }

    public static int CalculateDamageRoll(IRandomGenerator rng, int minDamage, int maxDamage)
    {
        int difference = (maxDamage - minDamage) + 1;

        return Math.Min(minDamage + rng.Next(difference),maxDamage);
    }

    public static int CalculateDamage(double critMulitplier, int damage)
    {
        return  (int) Math.Round(critMulitplier*damage);
    }

    public static IEnumerable<Combatant> CalculateTurnOrder(Combatant first, Combatant second)
    {
        double firstTurnDelay = BaseAttackSpeed/first.AttackSpeed;
        double secondTurnDelay = BaseAttackSpeed/second.AttackSpeed;
        double sum = 0.0;

        //Equal values do each once.
        if(firstTurnDelay == secondTurnDelay)
        {
            yield return first;
            yield return second;
            yield break;
        }

        double smallerDelay = firstTurnDelay <= secondTurnDelay ? firstTurnDelay : secondTurnDelay;
        double largerDelay = firstTurnDelay > secondTurnDelay ? firstTurnDelay : secondTurnDelay;
        Combatant smaller = firstTurnDelay <= secondTurnDelay ? first : second;
        Combatant larger = firstTurnDelay > secondTurnDelay ? first : second;
        while(true)
        {
            if(smallerDelay + sum > largerDelay)
            {
                yield return larger;
                yield break;
            }
            yield return smaller;
            sum += smallerDelay;
        }
    }

    public static IEnumerable<string> Battle(IRandomGenerator rng, Combatant player, Combatant enemy)
    {
        var turns = CalculateTurnOrder(player,enemy);
        var currentTarget = enemy;
        foreach(var turn in turns)
        {
            if(turn == player)
                currentTarget = enemy;
            else
                currentTarget = player;

            int critCount = CombatHelpers.CalculateCriticalCount(rng,turn.CriticalHitChance);
            double critMultiplier = CombatHelpers.CalculateCriticalMultiplier(turn.CriticalHitMultiplier, critCount);
            int damageRoll = CombatHelpers.CalculateDamageRoll(rng,turn.MinDamage,turn.MaxDamage);
            int totalDamage = CombatHelpers.CalculateDamage(critMultiplier,damageRoll);
 
            

            yield return $"{turn.Name} attacks. It was a {CombatHelpers.Criticality(critCount)} hit, Dealing {totalDamage} damage.";

            if(currentTarget.DealDamage(totalDamage))
            {
                yield return $"{currentTarget.Name} was defeated!";
                break;
            }
        }
        yield break;
    }

    public static string Criticality(int critCount)
    {
        switch (critCount)
        {
            case 0:
                return "";
            case 1:
                return "Critical";
            case 2:
                return "Double Critical";
            case 3:
                return "Triple Critical";
            case 4:
                return "Quadruple Critical";
            case 5:
                return "Quintuple Critical";
            case 6:
                return "Sextuple Critical";
            case 7:
                return "Septuple Critical";
            case 8:
                return "Octuple Critical";
            case 9:
                return "Nonuple Critical";
            case 10:
                return "Ronners Critical";
            default:
                return $"{critCount}x Critical";
        }
    }

}

