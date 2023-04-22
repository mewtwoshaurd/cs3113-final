using System.Collections;
using System.Collections.Generic;

public static class CardDicts
{
    public static Dictionary<UnitType, List<UnitType>> encounterDict = new Dictionary<UnitType, List<UnitType>>
    {
        {UnitType.Camel, new List<UnitType> {UnitType.Camel}},
        {UnitType.BaseGame, new List<UnitType> {UnitType.BaseGame}}
    };

    public static Dictionary<UnitType, int> unitHealthDict = new Dictionary<UnitType, int>
    {
        {UnitType.Camel, 2},
        {UnitType.BaseGame, 5}
    };

    public static Dictionary<UnitType, int> unitDamageDict = new Dictionary<UnitType, int>
    {
        {UnitType.Camel, 1},
        {UnitType.BaseGame, 5}
    };
}