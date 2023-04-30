using System.Collections;
using System.Collections.Generic;

public static class CardDicts
{
    public static Dictionary<UnitType, List<UnitType>> encounterDict = new Dictionary<UnitType, List<UnitType>>
    {
        // dog -> 3 dogs
        {UnitType.Dog, new List<UnitType> {UnitType.Dog, UnitType.Dog, UnitType.Dog}},
        // bat -> 2 bats, 1 spider
        {UnitType.Bat, new List<UnitType> {UnitType.Bat, UnitType.Bat, UnitType.Spider}},
        // gorilla -> 1 gorillas, 2 bees
        {UnitType.Gorilla, new List<UnitType> {UnitType.Gorilla, UnitType.Bee, UnitType.Bee}},
        // monkey -> 2 monkeys, 2 porcupines,
        {UnitType.Monkey, new List<UnitType> {UnitType.Monkey, UnitType.Monkey, UnitType.HedgeHog, UnitType.HedgeHog}},
        // lion -> 1 lion, 3 dogs
        {UnitType.Lion, new List<UnitType> {UnitType.Lion, UnitType.Dog, UnitType.Dog, UnitType.Dog}},
    };

    public static Dictionary<UnitType, int> unitHealthDict = new Dictionary<UnitType, int>
    {
        {UnitType.Dog, 6},
        {UnitType.Bat, 4},
        {UnitType.Gorilla, 13},
        {UnitType.Bee, 3},
        {UnitType.HedgeHog, 5},
        {UnitType.Monkey, 3},
        {UnitType.Spider, 1},
        {UnitType.Lion, 10}
    };

    public static Dictionary<UnitType, int> unitDamageDict = new Dictionary<UnitType, int>
    {
        {UnitType.Dog, 3},
        {UnitType.Bat, 2},
        {UnitType.Gorilla, 6},
        {UnitType.Bee, 1},
        {UnitType.HedgeHog, 2},
        {UnitType.Monkey, 5},
        {UnitType.Spider, 0},
        {UnitType.Lion, 4}
    };

    public static Dictionary<UnitType, AbilityType> unitAbilityDict = new Dictionary<UnitType, AbilityType>
    {
        {UnitType.Dog, AbilityType.None},
        {UnitType.Bat, AbilityType.Bloodsucker},
        {UnitType.Gorilla, AbilityType.Lazy},
        {UnitType.Bee, AbilityType.Swarm},
        {UnitType.HedgeHog, AbilityType.Spikey},
        {UnitType.Monkey, AbilityType.Wild},
        {UnitType.Spider, AbilityType.Curse},
        {UnitType.Lion, AbilityType.Intimidate}
    };

    public static Dictionary<UnitType, ItemType> unitItemDict = new Dictionary<UnitType, ItemType>
    {
        {UnitType.Dog, ItemType.Apple},
        {UnitType.Bat, ItemType.SmokeBomb},
        {UnitType.Gorilla, ItemType.Coffee},
        {UnitType.Bee, ItemType.SmokeBomb},
        {UnitType.HedgeHog, ItemType.SmokeBomb},
        {UnitType.Monkey, ItemType.Dagger},
        {UnitType.Spider, ItemType.Star},
        {UnitType.Lion, ItemType.Coffee}
    };
}