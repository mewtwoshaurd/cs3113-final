using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameEvent
{
    public EventType eventType;
    public List<object> data;

    public override string ToString()
    {
        List<String> dataStrings = data.Select(x => x.ToString()).ToList();
        return eventType.ToString() + ": {" + String.Join(", ", dataStrings) + "}";
    }
}

public enum EventType
{
    EncounterStarted,               // data: [enemyUnits]
    HandGiven,                      // data: [hand]
    HandTaken,                      // data: []
    UnitPlayed,                     // data: [unitId]
    ItemAttached,                   // data: [unitId, itemId, returnedItemId]
    PhaseEnded,                     // data: [phase]
    TurnEnded,                      // data: [turn]
    UnitAttacked,                   // data: [attackerId, defenderId]
    UnitStatChanged,                // data: [unitId, healthChange, damageChange]
    UnitAbilityActivation,          // TBD
    UnitItemActivation,             // TBD
    UnitStatusInflicted,            // TBD
    UnitStatusUpdated,              // TBD
    UnitStatusRemoved,              // TBD
    UnitDied,                       // data: [unitId]
    EncounterEnded,                 // data: [boolPlayerWon]
    Error,                          // data: [errorMessage]
}