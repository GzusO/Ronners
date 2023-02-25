namespace Ronners.RPG;

public class CombatHelpersTest
{
    [Theory]
    [InlineData(0,0)]
    [InlineData(1,1)]
    [InlineData(1,100)]
    [InlineData(2,101)]
    [InlineData(7,650)]
    [InlineData(-1,-1)]
    [InlineData(-1,-100)]
    [InlineData(-2,-101)]
    [InlineData(-7, -650)]
    public void CalculateCriticalCount_GuranteedCriticals(int expected, int critChance)
    {
        var gen = new MockRandomGenerator(0);

        int actual = CombatHelpers.CalculateCriticalCount(gen,critChance);

        Assert.Equal(expected,actual);
    }

    [Theory]
    [InlineData(0,0)]
    [InlineData(0,1)]
    [InlineData(1,100)]
    [InlineData(1,101)]
    [InlineData(6,650)]
    [InlineData(0,-1)]
    [InlineData(-1,-100)]
    [InlineData(-1,-101)]
    [InlineData(-6, -650)]
    public void CalculateCriticalCount_GuranteedNoCriticals(int expected, int critChance)
    {
        var gen = new MockRandomGenerator(100);

        int actual = CombatHelpers.CalculateCriticalCount(gen,critChance);

        Assert.Equal(expected,actual);
    }

    [Theory]
    [InlineData(1,1.5,0)]
    [InlineData(1,1,1)]
    [InlineData(2.0,2.0,1)]
    [InlineData(4.0,2.0,2)]
    [InlineData(2.25,1.5,2)]
    [InlineData(2147483648,2.0,31)]
    [InlineData(.5,2.0,-1)]
    [InlineData(.25,2.0,-2)]
    [InlineData(1,1.0,-1)]
    [InlineData(1,1.0,-2)]
    [InlineData(.6666666666666666,1.5,-1)]
    [InlineData(1.3333333333333333,.75,-1)]
    public void CalculateCriticalMultiplier_SimpleValues(double expected, double baseCritMultiplier, int critCount)
    {
        double actual = CombatHelpers.CalculateCriticalMultiplier(baseCritMultiplier,critCount);

        Assert.Equal(expected,actual);
    }

    [Theory]
    [InlineData(1,1,1)]
    [InlineData(1,1,5)]
    public void CalculateDamageRoll_MinDamage(int expected, int minDamage, int maxDamage)
    {
        var gen = new MockRandomGenerator(0);

        int actual = CombatHelpers.CalculateDamageRoll(gen,minDamage,maxDamage);

        Assert.Equal(expected,actual); 
    }

    [Theory]
    [InlineData(1,1,1)]
    [InlineData(5,1,5)]
    [InlineData(7,1,7)]
    public void CalculateDamageRoll_MaxDamage(int expected, int minDamage, int maxDamage)
    {
        var gen = new MockRandomGenerator(maxDamage-minDamage);

        int actual = CombatHelpers.CalculateDamageRoll(gen,minDamage,maxDamage);

        Assert.Equal(expected,actual); 
    }

    [Theory]
    [InlineData(2,1,5)]
    [InlineData(5,5,5)]
    [InlineData(1,1,1)]
    public void CalculateDamageRoll_SimpleValues(int expected, int minDamage, int maxDamage)
    {
        var gen = new MockRandomGenerator(1);

        int actual = CombatHelpers.CalculateDamageRoll(gen,minDamage,maxDamage);

        Assert.Equal(expected,actual); 
    }


    [Theory]
    [InlineData(1, 1.00, 1)]
    [InlineData(0, 0.00, 0)]
    [InlineData(15,1.50, 10)]
    [InlineData(2, 1.50, 1)] //Round up midway
    [InlineData(2, 1.75, 1)] //Round up
    [InlineData(1, 1.10, 1)] //Round down
    public void CalculateDamage_SimpleValues(int expected, double critMulitplier, int damage)
    {
        
        int actual = CombatHelpers.CalculateDamage(critMulitplier,damage);

        Assert.Equal(expected,actual);
    }

    [Fact]
    public void CalculateTurnOrder_SameSpeed()
    {
        var player = new Combatant();
        var enemy = new Combatant();

        var actual = CombatHelpers.CalculateTurnOrder(player,enemy);

        Assert.Equal(new List<Combatant>{player,enemy},actual);
    }

    [Fact]
    public void CalculateTurnOrder_FirstThenSecond()
    {
        var player = new Combatant().SetRapidity(3);
        var enemy = new Combatant().SetRapidity(2);

        var actual = CombatHelpers.CalculateTurnOrder(player,enemy);

        Assert.Equal(new List<Combatant>{player,enemy},actual);
    }

    [Fact]
    public void CalculateTurnOrder_TwoFirstThanSecond()
    {
        var player = new Combatant().SetRapidity(2);
        var enemy = new Combatant().SetRapidity(1);

        var actual = CombatHelpers.CalculateTurnOrder(player,enemy);

        Assert.Equal(new List<Combatant>{player,player,enemy},actual);
    }

    [Fact]
    public void CalculateTurnOrder_MultipleFirstThanSecond()
    {
        var player = new Combatant().SetRapidity(5);
        var enemy = new Combatant().SetRapidity(1);

        var actual = CombatHelpers.CalculateTurnOrder(player,enemy);

        Assert.Equal(new List<Combatant>{player,player,player,player,player,enemy},actual);
    }

    [Fact]
    public void CalculateTurnOrder_SecondThenFirst()
    {
        var player = new Combatant().SetRapidity(2);
        var enemy = new Combatant().SetRapidity(3);

        var actual = CombatHelpers.CalculateTurnOrder(player,enemy);

        Assert.Equal(new List<Combatant>{enemy,player},actual);
    }

    [Fact]
    public void CalculateTurnOrder_TwoSecondThenFirst()
    {
        var player = new Combatant().SetRapidity(1);
        var enemy = new Combatant().SetRapidity(2);

        var actual = CombatHelpers.CalculateTurnOrder(player,enemy);

        Assert.Equal(new List<Combatant>{enemy,enemy,player},actual);
    }

    [Fact]
    public void CalculateTurnOrder_MultipleSecondThenFirst()
    {
        var player = new Combatant().SetRapidity(1);
        var enemy = new Combatant().SetRapidity(5);

        var actual = CombatHelpers.CalculateTurnOrder(player,enemy);

        Assert.Equal(new List<Combatant>{enemy,enemy,enemy,enemy,enemy,player},actual);
    }

}