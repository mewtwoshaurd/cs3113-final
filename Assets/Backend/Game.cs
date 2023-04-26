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
        Random random = new Random();
        foreach (Card unit in enemyUnits)
        {
            // 1 in 5 chance of having an item
            int randomNumber = random.Next(0, 5);
            if (randomNumber == 0)
            {
                unit.heldItem = Card.ItemCard(CardDicts.unitItemDict[unit.unitType]);
                unit.heldItemTurn = turn;
            }
        }
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
            // change phase
            phase = Phase.PlayerUnits;
            events.Add(new GameEvent { eventType = EventType.PhaseEnded, data = new List<object> { phase } });
            // move hand back to deck
            foreach (Card card in hand)
            {
                deck.Add(card);
            }
            hand.Clear();
            events.Add(new GameEvent { eventType = EventType.HandTaken, data = new List<object> { } });
            // units regenerate attacks
            foreach (Card card in playerUnits)
            {
                // swarm units get 3 attacks
                if (card.abilityType == AbilityType.Swarm)
                {
                    card.attacksRemaining += 3;
                }
                else
                {
                    card.attacksRemaining += 1;
                }
            }
        }
        else if (phase == Phase.PlayerUnits)
        {
            // change phase
            phase = Phase.EnemyUnits;
            events.Add(new GameEvent { eventType = EventType.PhaseEnded, data = new List<object> { phase } });
        }
        else if (phase == Phase.EnemyUnits)
        {
            // change phase
            phase = Phase.PlayerCards;
            events.Add(new GameEvent { eventType = EventType.PhaseEnded, data = new List<object> { phase } });
            // new turn
            turn++;
            events.Add(new GameEvent { eventType = EventType.TurnEnded, data = new List<object> { turn } });
            // give hand to player
            deck = deck.OrderBy(x => Guid.NewGuid()).ToList();
            int cardsToDraw = Math.Min(5, deck.Count);
            hand = deck.Take(cardsToDraw).ToList();
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
        HandleAttack(events, attacker, defender);

        // If the defender has the intimidate ability, lower attackers atk by 2 (min 1)
        if (defender.abilityType == AbilityType.Intimidate)
        {
            events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { defender.id } });
            int oldDamage = attacker.damage;
            attacker.damage = Math.Max(1, attacker.damage - 2);
            int damageChange = attacker.damage - oldDamage;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { attacker.id, 0, damageChange } });
        }

        // If the defender has the wild ability, swap attack and defense stats as long as its hp isnt already 0
        if (defender.abilityType == AbilityType.Wild && defender.health > 0)
        {
            events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { defender.id } });
            int oldDamage = defender.damage;
            defender.damage = defender.health;
            defender.health = oldDamage;

            int damageChange = defender.damage - defender.health;
            int healthChange = defender.health - defender.damage;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defender.id, healthChange, damageChange } });
        }

        // If the defender has the bloodsucker ability, heal the defender by 1hp as long as its hp isnt already 0
        if (defender.abilityType == AbilityType.Bloodsucker && defender.health > 0)
        {
            events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { defender.id } });
            defender.health++;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defender.id, 1, 0 } });
        }

        // If the defender has the spikey ability, deal 1 damage to the attacker
        if (defender.abilityType == AbilityType.Spikey && attacker.health > 0)
        {
            events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { defender.id } });
            attacker.health--;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { attacker.id, -1, 0 } });
            if (attacker.health <= 0)
            {
                HandleDeath(events, attacker);
            }
        }

        // If the attacker has the lazy ability, decrease its attacks remaining by 1
        if (attacker.abilityType == AbilityType.Lazy && attacker.health > 0)
        {
            events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { attacker.id } });
            attacker.attacksRemaining--;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { attacker.id, 0, -1 } });
        }

        return events;
    }

    private static void HandleAttack(List<GameEvent> events, Card attacker, Card defender)
    {
        events.Add(new GameEvent { eventType = EventType.UnitAttacked, data = new List<object> { attacker.id, defender.id } });
        attacker.attacksRemaining--;
        // If the attacker is holding coffee, then they get an extra attack
        if (attacker.heldItem != null && attacker.heldItem.itemType == ItemType.Coffee)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { attacker.id, attacker.heldItem.id } });
            attacker.attacksRemaining++;
            attacker.heldItem = null;
        }

        HandleDamage(events, attacker, defender);
    }

    private static void HandleDamage(List<GameEvent> events, Card attacker, Card defender)
    {
        int damageDone = attacker.damage;
        // If the attacker has a sword, then they do +2 damage
        if (attacker.heldItem != null && attacker.heldItem.itemType == ItemType.Sword)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { attacker.id, attacker.heldItem.id } });
            damageDone += 2;
            attacker.heldItem = null;
        }
        // If the defender is holding a smoke bomb, they negate all damage
        if (defender.heldItem != null && defender.heldItem.itemType == ItemType.SmokeBomb)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { defender.id, defender.heldItem.id } });
            damageDone = 0;
            defender.heldItem = null;
        }
        defender.health -= damageDone;
        events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defender.id, -attacker.damage, 0 } });
        // If the defender took a non-zero amount of damage and has more than 0 health remaining and is holding a water, then they recieve +3 health
        if (damageDone > 0 && defender.health > 0 && defender.heldItem != null && defender.heldItem.itemType == ItemType.Water)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { defender.id, defender.heldItem.id } });
            defender.health += 3;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defender.id, 3, 0 } });
            defender.heldItem = null;
        }

        // If after all of this the attacker or defender has 0 or less health, they die
        if (defender.health <= 0)
        {
            HandleDeath(events, defender);

            // if the defender has the curse ability, kill the attacker
            if (defender.abilityType == AbilityType.Curse && attacker.health > 0)
            {
                events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { defender.id } });
                attacker.health = 0;
            }
        }
        if (attacker.health <= 0)
        {
            HandleDeath(events, attacker);
        }
    }

    private static void HandleDeath(List<GameEvent> events, Card unit)
    {
        // If the unit is holding a pentagram, set unit health to 1 and do not die
        if (unit.heldItem != null && unit.heldItem.itemType == ItemType.Pentagram)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { unit.id, unit.heldItem.id } });
            unit.health = 1;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { unit.id, 1, 0 } });
            unit.heldItem = null;
        }
        // otherwise, the unit dies
        else
        {
            // Remove unit from field
            if (phase == Phase.PlayerUnits)
            {
                enemyUnits.Remove(unit);
            }
            else
            {
                playerUnits.Remove(unit);
            }
            // Send death event
            events.Add(new GameEvent { eventType = EventType.UnitDied, data = new List<object> { unit.id } });

            // Determine if the game has ended
            if (HasWon())
            {
                events.Add(new GameEvent { eventType = EventType.EncounterEnded, data = new List<object> { true } });
            }
            else if (HasLost())
            {
                events.Add(new GameEvent { eventType = EventType.EncounterEnded, data = new List<object> { false } });
            }
        }
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
    public AbilityType abilityType;
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
        card.abilityType = CardDicts.unitAbilityDict[unitType];
        return card;
    }

    public static Card ItemCard(ItemType itemType)
    {
        Card card = new Card();
        card.cardType = CardType.Item;
        card.unitType = UnitType.NotApplicable;
        card.itemType = itemType;
        card.abilityType = AbilityType.None;
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
    Dog,
    Bat,
    Gorilla,
    Bee,
    Porcupine,
    Monkey,
    Spider,
    Lion
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

public enum AbilityType
{
    None,
    Intimidate,     // If attacked, lower attackers atk by 2 (min 1)
    Curse,          // If attacked, set hp of attacker and defender to 0
    Wild,           // After attacking, attack and defense stats swap
    Bloodsucker,    // After attacking, regenerate 1 hp
    Lazy,           // After attacking, require an extra turn's worth of rest to get another attack
    Swarm,          // Gets triple the amount of rest per turn
    Spikey          // When attacked, deal 1 damage to the attacker
}