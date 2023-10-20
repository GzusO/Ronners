namespace Ronners.RPG;

public class CombatantTest
{
    [Theory]
    [InlineData("")]
    [InlineData("Gzus")]
    public void SetName_ValidValues(string name)
    {
        var player = new Combatant().SetName(name);
        Assert.Equal(name,player.Name);
    }
    [Fact]
    public void Combatant_DefaultName()
    {
        var player = new Combatant();
        Assert.Equal("",player.Name);
    }

    [Theory]
    [InlineData(1,1,1,1,1,1,1)]
    [InlineData(1,2,1,3,1,4,1)]
    [InlineData(1,2,3,4,5,6,7)]
    [InlineData(10,9,8,7,6,5,4)]
    public void SetAllAttributes_ValidValues(int ronners, int objectivity, int normalcy, int nutrition, int erudition, int rapidity, int strength)
    {
        var player = new Combatant().SetRonners(ronners).SetObjectivity(objectivity).SetNormalcy(normalcy).SetNutrition(nutrition).SetErudition(erudition).SetRapidity(rapidity).SetStrength(strength);
        Assert.Equal(ronners,player.Ronners);
        Assert.Equal(objectivity,player.Objectivity);
        Assert.Equal(normalcy,player.Normalcy);
        Assert.Equal(nutrition,player.Nutrition);
        Assert.Equal(erudition,player.Erudition);
        Assert.Equal(rapidity,player.Rapidity);
        Assert.Equal(strength,player.Strength);
    }
    [Theory]
    [InlineData(-1,-1,-1,-1,-1,-1,-1)]
    [InlineData(-10,-9,-8,-7,-6,-5,-4)]
    [InlineData(-10,-11,-12,-13,-14,-15,-16)]
    [InlineData(0,0,0,0,0,0,0)]
    public void SetAttribute_NegativeValues(int ronners, int objectivity, int normalcy, int nutrition, int erudition, int rapidity, int strength)
    {
        var player = new Combatant().SetRonners(ronners).SetObjectivity(objectivity).SetNormalcy(normalcy).SetNutrition(nutrition).SetErudition(erudition).SetRapidity(rapidity).SetStrength(strength);
        int expected = 1;

        Assert.Equal(expected,player.Ronners);
        Assert.Equal(expected,player.Objectivity);
        Assert.Equal(expected,player.Normalcy);
        Assert.Equal(expected,player.Nutrition);
        Assert.Equal(expected,player.Erudition);
        Assert.Equal(expected,player.Rapidity);
        Assert.Equal(expected,player.Strength);
    }

    [Theory]
    [InlineData(-1,1)]
    [InlineData(1,1)]
    [InlineData(100,100)]
    [InlineData(101,101)]
    public void CriticalHitChance_SimpleValues(int value, int expected)
    {
        var player = new Combatant().SetRonners(value);

        Assert.Equal(expected,player.CriticalHitChance);
    }

    [Theory]
    [InlineData(5,5,5,5)]
    [InlineData(5,1,5,5)]
    [InlineData(2,1,2,10)]
    public void MinDamage_Capped(int expected, int minDamage, int maxDamage, int strength)
    {
        var player = new Combatant().SetStrength(strength);
        player.HandSlot = new Weapon(minDamage,maxDamage);

        var actual = player.MinDamage;

        Assert.Equal(expected,actual);
    }
    
    [Theory]
    [InlineData(2,1,5,0)]
    [InlineData(3,1,10,2)]
    [InlineData(1,1,1,1)]
    public void MinDamage_UnCapped(int expected, int minDamage, int maxDamage, int strength)
    {
        var player = new Combatant().SetStrength(strength);
        player.HandSlot = new Weapon(minDamage,maxDamage);

        var actual = player.MinDamage;

        Assert.Equal(expected,actual);
    }

    [Theory]
    [InlineData(1.0, 1.0, 1)]
    [InlineData(2.0, 2.0, 1)]
    [InlineData(5.0, 2.5, 2)]
    public void AttackSpeed_WithWeapon(double expected, double weaponSpeed, int rapidity)
    {
        var player = new Combatant().SetRapidity(rapidity);
        player.HandSlot = new Weapon(attackSpeed:weaponSpeed);

        var actual = player.AttackSpeed;

        Assert.Equal(expected,actual);
    }

    [Fact]
    public void AttackSpeed_NoWeapon()
    {
        var player = new Combatant().SetRapidity(1);
        
        var actual = player.AttackSpeed;
        var expected = 2; //2*Rapidity for no weapon, fists are fast

        Assert.Equal(expected,actual);
    }

    [Fact]
    public void MinDamage_WithWeapon()
    {
        var player = new Combatant().SetStrength(10);
        var weapon = new Weapon("Fists","hands bro",1,1,2,"punch");
        player.HandSlot = weapon;

        var actual = player.MinDamage;
        var expected = 1;

        Assert.Equal(expected,actual);
    }

    [Fact]
    public void MaxDamage_WithWeapon()
    {
        var player = new Combatant().SetStrength(10);
        var weapon = new Weapon("Fists","hands bro",1,1,2,"punch");
        player.HandSlot = weapon;

        var actual = player.MinDamage;
        var expected = 1;

        Assert.Equal(expected,actual);
    }
}