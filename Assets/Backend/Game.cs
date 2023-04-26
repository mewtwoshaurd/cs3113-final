using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static partial class Game
{
    private static List<Card> deck = new List<Card>();
    private static List<Card> hand = new List<Card>();
    private static List<Card> playerUnits = new List<Card>();
    private static List<Card> enemyUnits = new List<Card>();
    private static Phase phase = Phase.PlayerCards;
    private static int turn = 1;

    // Interfaces
    public static partial List<GameEvent> StartEncounter(List<Card> deck, UnitType encounterType);
    public static partial List<GameEvent> PlayUnit(int unitId);
    public static partial List<GameEvent> AttachItem(int unitId, int itemId);
    public static partial List<GameEvent> EndPhase();
    public static partial List<GameEvent> AttackUnit(int attackerId, int defenderId);

    public static bool HasWon()
    {
        return enemyUnits.Count == 0;
    }

    public static bool HasLost()
    {
        int numUnitCardsInHand = hand.Count(x => x.cardType == CardType.Unit);
        return playerUnits.Count == 0 && numUnitCardsInHand == 0;
    }

    public static partial List<GameEvent> StartEncounter(List<Card> deck, UnitType encounterType)
    {
        List<GameEvent> events = new List<GameEvent>();

        if (deck.Count < 5)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Deck must have at least 5 cards" } });
            return events;
        }

        turn = 1;
        playerUnits = new List<Card>();
        Game.phase = Phase.PlayerCards;

        enemyUnits = CardDicts.encounterDict[encounterType].Select(x => Card.UnitCard(x)).ToList();
        events.Add(new GameEvent { eventType = EventType.EncounterStarted, data = new List<object> { enemyUnits } });

        Game.deck = deck;
        Game.deck = Game.deck.OrderBy(x => Guid.NewGuid()).ToList();
        Game.hand = Game.deck.Take(5).ToList();
        events.Add(new GameEvent { eventType = EventType.HandGiven, data = new List<object> { Game.hand } });

        return events;
    }

    public static partial List<GameEvent> PlayUnit(int unitId)
    {
        List<GameEvent> events = new List<GameEvent>();

        if (phase != Phase.PlayerCards)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Cannot play unit in phase " + phase } });
            return events;
        }

        Card unit = hand.Find(x => x.id == unitId);
        if (unit == null)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Card '" + unitId.ToString() + "' not found in hand" } });
            return events;
        }
        if (unit.cardType != CardType.Unit)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Card '" + unit.ToString() + "' not a unit" } });
            return events;
        }
        if (playerUnits.Count >= 5)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Cannot play more than 5 units" } });
            return events;
        }

        hand.Remove(unit);
        playerUnits.Add(unit);
        events.Add(new GameEvent { eventType = EventType.UnitPlayed, data = new List<object> { unit.id } });

        return events;
    }

    public static partial List<GameEvent> AttachItem(int unitId, int itemId)
    {
        List<GameEvent> events = new List<GameEvent>();

        if (phase != Phase.PlayerCards)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Cannot attach item in phase " + phase } });
            return events;
        }

        Card unit = playerUnits.Find(x => x.id == unitId);
        Card item = hand.Find(x => x.id == itemId);
        if (unit == null)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Unit '" + unitId.ToString() + "' not found in player units" } });
            return events;
        }
        if (item == null)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Item '" + itemId.ToString() + "' not found in hand" } });
            return events;
        }

        if (unit.cardType != CardType.Unit)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Card '" + unit.ToString() + "' not a unit" } });
            return events;
        }
        if (item.cardType != CardType.Item)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Card '" + item.ToString() + "' not an item" } });
            return events;
        }

        hand.Remove(item);
        Card returnedItem = null;
        if (unit.heldItem != null && unit.heldItemTurn == turn)
        {
            returnedItem = unit.heldItem;
            hand.Add(unit.heldItem);
        }
        unit.heldItem = item;
        unit.heldItemTurn = turn;
        events.Add(new GameEvent { eventType = EventType.ItemAttached, data = new List<object> { unit.id, item.id, returnedItem.id } });

        return events;
    }

    public static partial List<GameEvent> EndPhase()
    {
        List<GameEvent> events = new List<GameEvent>();

        if (phase == Phase.PlayerCards && playerUnits.Count == 0)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Cannot end phase with no player units on field" } });
            return events;
        }

        if (phase == Phase.PlayerCards)
        {
            phase = Phase.PlayerUnits;
            events.Add(new GameEvent { eventType = EventType.PhaseEnded, data = new List<object> { phase } });
            foreach (Card card in hand)
            {
                deck.Add(card);
            }
            hand.Clear();
            events.Add(new GameEvent { eventType = EventType.HandTaken, data = new List<object> { } });
        }
        else if (phase == Phase.PlayerUnits)
        {
            phase = Phase.EnemyUnits;
            events.Add(new GameEvent { eventType = EventType.PhaseEnded, data = new List<object> { phase } });
        }
        else if (phase == Phase.EnemyUnits)
        {
            phase = Phase.PlayerCards;
            events.Add(new GameEvent { eventType = EventType.PhaseEnded, data = new List<object> { phase } });
            turn++;
            events.Add(new GameEvent { eventType = EventType.TurnEnded, data = new List<object> { turn } });
            deck = deck.OrderBy(x => Guid.NewGuid()).ToList();
            hand = deck.Take(5).ToList();
            events.Add(new GameEvent { eventType = EventType.HandGiven, data = new List<object> { hand } });
        }

        return events;
    }

    public static partial List<GameEvent> AttackUnit(int attackerId, int defenderId)
    {
        List<GameEvent> events = new List<GameEvent>();

        Card attacker = phase == Phase.PlayerUnits ? playerUnits.Find(x => x.id == attackerId) : enemyUnits.Find(x => x.id == attackerId);
        String attackerString = phase == Phase.PlayerUnits ? "player" : "enemy";
        Card defender = phase == Phase.PlayerUnits ? enemyUnits.Find(x => x.id == defenderId) : playerUnits.Find(x => x.id == defenderId);
        String defenderString = phase == Phase.PlayerUnits ? "enemy" : "player";
        if (attacker == null)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Card '" + attackerId.ToString() + "' not found in " + attackerString + " units" } });
            return events;
        }
        if (defender == null)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Card '" + defenderId.ToString() + "' not found in " + defenderString + " units" } });
            return events;
        }

        if (attacker.cardType != CardType.Unit)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Card '" + attacker.ToString() + "' not a unit" } });
            return events;
        }
        if (defender.cardType != CardType.Unit)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Card '" + defender.ToString() + "' not a unit" } });
            return events;
        }
        if (attacker.attacksRemaining <= 0)
        {
            events.Add(new GameEvent { eventType = EventType.Error, data = new List<object> { "Unit '" + attacker.ToString() + "' has no attacks remaining" } });
            return events;
        }

        // If after all of this there have been no errors, the attack will be registered
        events.Add(new GameEvent { eventType = EventType.UnitAttacked, data = new List<object> { attackerId, defenderId } });
        attacker.attacksRemaining--;
        // If the attacker is holdinmg coffee, then they get an extra attack
        if (attacker.heldItem != null && attacker.heldItem.itemType == ItemType.Coffee)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { attackerId, attacker.heldItem.id } });
            attacker.attacksRemaining++;
            attacker.heldItem = null;
        }

        // Damage calculations
        int damageDone = attacker.damage;
        // If the attacker has a sword, then they do +2 damage
        if (attacker.heldItem != null && attacker.heldItem.itemType == ItemType.Sword)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { attackerId, attacker.heldItem.id } });
            damageDone += 2;
            attacker.heldItem = null;
        }
        // If the defender is holding a smoke bomb, they negate all damage
        if (defender.heldItem != null && defender.heldItem.itemType == ItemType.SmokeBomb)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { defenderId, defender.heldItem.id } });
            damageDone = 0;
            defender.heldItem = null;
        }
        defender.health -= damageDone;
        events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defenderId, -attacker.damage, 0 } });
        // If the defender took a non-zero amount of damage and has more than 0 health remaining and is holding a water, then they recieve +3 health
        if (damageDone > 0 && defender.health > 0 && defender.heldItem != null && defender.heldItem.itemType == ItemType.Water)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { defenderId, defender.heldItem.id } });
            defender.health += 3;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defenderId, 3, 0 } });
            defender.heldItem = null;
        }

        // Calculate deaths
        // If health is <= 0 and the unit is holding a pentagram, set unit health to 1 and do not die
        if (defender.health <= 0 && defender.heldItem != null && defender.heldItem.itemType == ItemType.Pentagram)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { defenderId, defender.heldItem.id } });
            defender.health = 1;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defenderId, 1, 0 } });
            defender.heldItem = null;
        }

        if (defender.health <= 0)
        {
            if (phase == Phase.PlayerUnits)
            {
                enemyUnits.Remove(defender);
            }
            else
            {
                playerUnits.Remove(defender);
            }
            events.Add(new GameEvent { eventType = EventType.UnitDied, data = new List<object> { defenderId } });

            if (HasWon())
            {
                events.Add(new GameEvent { eventType = EventType.EncounterEnded, data = new List<object> { true } });
            }
            else if (HasLost())
            {
                events.Add(new GameEvent { eventType = EventType.EncounterEnded, data = new List<object> { false } });
            }
        }

        return events;
    }
}

public enum Phase
{
    PlayerCards,
    PlayerUnits,
    EnemyUnits,
}

public class Card : ICloneable
{
    static int idCounter = 0;

    public int id;
    public CardType cardType;
    public UnitType unitType;
    public ItemType itemType;
    public int health;
    public int damage;
    public Card heldItem;
    public int heldItemTurn = -1;
    public int attacksRemaining = 1;

    public Card(int given_id = -1)
    {
        id = given_id == -1 ? idCounter++ : given_id;
        health = 1;
        damage = 0;
        heldItem = null;
    }

    public object Clone()
    {
        Card card = new Card(id);
        card.cardType = cardType;
        card.unitType = unitType;
        card.itemType = itemType;
        card.health = health;
        card.damage = damage;
        card.heldItem = (Card)(heldItem == null ? null : heldItem.Clone());
        return card;
    }

    public override string ToString()
    {
        if (cardType == CardType.Item)
            return "ItemCard (" + id.ToString() + ", " + itemType.ToString() + ")";
        else
            return "UnitCard (" + id.ToString() + ", " + unitType.ToString() + ")";
    }

    public static Card UnitCard(UnitType unitType)
    {
        Card card = new Card();
        card.cardType = CardType.Unit;
        card.unitType = unitType;
        card.itemType = ItemType.NotApplicable;
        card.health = CardDicts.unitHealthDict[unitType];
        card.damage = CardDicts.unitDamageDict[unitType];
        return card;
    }

    public static Card ItemCard(ItemType itemType)
    {
        Card card = new Card();
        card.cardType = CardType.Item;
        card.unitType = UnitType.NotApplicable;
        card.itemType = itemType;
        return card;
    }
}

public enum CardType
{
    Unit,
    Item
}

public enum UnitType
{
    NotApplicable,
    Camel,
    Wolf,
    Owl,
    Octopus,
    Snake,
    Gecko,
    Bee,
    Bat,
    Hedgehog,
    Crocodile,
    BaseGame
}

public enum ItemType
{
    NotApplicable,
    Water,      // Cure 3 hp if damaged and not dead
    SmokeBomb,  // Avoid all damage from attack
    Pentagram,  // If killed, revive with 1 hp
    Sword,      // +2 damage for the attack
    Coffee      // Gain an extra attack after attacking
}