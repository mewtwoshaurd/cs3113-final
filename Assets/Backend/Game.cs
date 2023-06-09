using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public static partial class Game
{
    private static List<Card> deck = new List<Card>();
    public static List<Card> hand = new List<Card>();
    public static List<Card> playerUnits = new List<Card>();
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

    static void RemoveFromList(List<Card> list, int id)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].id == id)
            {
                list.RemoveAt(i);
                return;
            }
        }
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
            // 1 in 4 chance of having an item
            int randomNumber = random.Next(0, 4);
            if (randomNumber == 0)
            {
                unit.heldItem = Card.ItemCard(CardDicts.unitItemDict[unit.unitType]);
                unit.heldItemTurn = turn;
            }
        }
        events.Add(new GameEvent { eventType = EventType.EncounterStarted, data = new List<object> { CardListCopy(enemyUnits) } });

        Game.deck = CardListCopy(deck);

        // give player 2 units and 3 items. it should be guaranteed they have this
        hand = new List<Card>();
        // shuffle deck then separate units and items
        deck = deck.OrderBy(x => Guid.NewGuid()).ToList();
        List<Card> unitCards = deck.Where(x => x.cardType == CardType.Unit).ToList();
        List<Card> itemCards = deck.Where(x => x.cardType == CardType.Item).ToList();
        // take 2 units and 3 items
        for (int i = 0; i < 2; i++)
        {
            if (unitCards.Count - 1 < i)
                break;
            hand.Add(unitCards[i]);
            RemoveFromList(deck, unitCards[i].id);
        }
        for (int i = 0; i < 3; i++)
        {
            if (itemCards.Count - 1 < i)
                break;
            hand.Add(itemCards[i]);
            RemoveFromList(deck, itemCards[i].id);
        }

        events.Add(new GameEvent { eventType = EventType.HandGiven, data = new List<object> { CardListCopy(Game.hand) } });

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

        RemoveFromList(hand, unit.id);
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

        RemoveFromList(hand, item.id);
        Card returnedItem = null;
        int retId = -1;
        if (unit.heldItem != null && unit.heldItemTurn == turn)
        {
            returnedItem = unit.heldItem;
            retId = returnedItem.id;
            hand.Add(unit.heldItem);
        }
        unit.heldItem = item;
        unit.heldItemTurn = turn;
        events.Add(new GameEvent { eventType = EventType.ItemAttached, data = new List<object> { unit.id, item.id, retId } });

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
                //print what the card id is and unittype
                deck.Add(card);
            }
            hand.Clear();
            events.Add(new GameEvent { eventType = EventType.HandTaken, data = new List<object> { } });
            // units regenerate attacks
            foreach (Card card in playerUnits)
            {
                // lazy units get attack stat modified
                if (card.abilityType == AbilityType.Lazy)
                {
                    events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { card.id } });
                    int delta = -card.damage;
                    card.damage = (card.damage == 0) ? CardDicts.unitDamageDict[card.unitType] : 0;
                    delta += card.damage;
                    events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { card.id, 0, delta, card.health, card.damage } });
                }

                // swarm units get +1 attack if they did not attack last turn
                if (card.abilityType == AbilityType.Swarm && card.attacksRemaining != 0)
                {
                    events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { card.id } });
                    card.damage++;
                    events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { card.id, 0, 1, card.health, card.damage } });
                }

                card.attacksRemaining = 1;
            }
        }
        else if (phase == Phase.PlayerUnits)
        {
            // change phase
            phase = Phase.EnemyUnits;
            events.Add(new GameEvent { eventType = EventType.PhaseEnded, data = new List<object> { phase } });

            foreach (Card card in enemyUnits)
            {
                // lazy units get attack stat modified
                if (card.abilityType == AbilityType.Lazy)
                {
                    events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { card.id } });
                    int delta = -card.damage;
                    card.damage = (card.damage == 0) ? CardDicts.unitDamageDict[card.unitType] : 0;
                    delta += card.damage;
                    events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { card.id, 0, delta, card.health, card.damage } });
                }

                // swarm units get +1 attack if they did not attack last turn
                if (card.abilityType == AbilityType.Swarm && card.attacksRemaining != 0)
                {
                    events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { card.id } });
                    card.damage++;
                    events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { card.id, 0, 1, card.health, card.damage } });
                }

                card.attacksRemaining = 1;
            }

            HandleEnemyTurn(events);
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
            // give 1 unit card (or 0 if there are none) and 2 item cards (or 1 if there is only 1 / 0 if there are none) to player
            deck = deck.OrderBy(x => Guid.NewGuid()).ToList();

            hand = new List<Card>();
            // get lists of units and items in deck
            List<Card> unitCards = deck.FindAll(x => x.cardType == CardType.Unit);
            List<Card> itemCards = deck.FindAll(x => x.cardType == CardType.Item);
            // get 1 unit card
            if (unitCards.Count > 0)
            {
                hand.Add(unitCards[0]);
                RemoveFromList(deck, unitCards[0].id);
            }
            // get 2 item cards
            if (itemCards.Count > 0)
            {
                hand.Add(itemCards[0]);
                RemoveFromList(deck, itemCards[0].id);
            }
            if (itemCards.Count > 1)
            {
                hand.Add(itemCards[1]);
                RemoveFromList(deck, itemCards[1].id);
            }

            hand = hand.Distinct().ToList();

            events.Add(new GameEvent { eventType = EventType.HandGiven, data = new List<object> { CardListCopy(hand) } });
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
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { attacker.id, 0, damageChange, attacker.health, attacker.damage } });
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
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defender.id, healthChange, damageChange, defender.health, defender.damage } });
        }

        // If the defender has the bloodsucker ability, heal the defender by 1hp as long as its hp isnt already 0
        if (defender.abilityType == AbilityType.Bloodsucker && defender.health > 0)
        {
            events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { defender.id } });
            defender.health++;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defender.id, 1, 0, defender.health, defender.damage } });
        }

        // If the defender has the spikey ability, deal 1 damage to the attacker
        if (defender.abilityType == AbilityType.Spikey && attacker.health > 0)
        {
            events.Add(new GameEvent { eventType = EventType.UnitAbilityActivation, data = new List<object> { defender.id } });
            attacker.health--;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { attacker.id, -1, 0, attacker.health, attacker.damage } });
            if (attacker.health <= 0)
            {
                HandleDeath(events, attacker);
            }
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
        if (attacker.heldItem != null && attacker.heldItem.itemType == ItemType.Dagger)
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
        events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defender.id, -damageDone, 0, defender.health, defender.damage } });
        // If the defender took a non-zero amount of damage and has more than 0 health remaining and is holding a water, then they recieve +3 health
        if (damageDone > 0 && defender.health > 0 && defender.heldItem != null && defender.heldItem.itemType == ItemType.Apple)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { defender.id, defender.heldItem.id } });
            defender.health += 3;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { defender.id, 3, 0, defender.health, defender.damage } });
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
        if (unit.heldItem != null && unit.heldItem.itemType == ItemType.Star)
        {
            events.Add(new GameEvent { eventType = EventType.UnitItemActivation, data = new List<object> { unit.id, unit.heldItem.id } });
            unit.health = 1;
            events.Add(new GameEvent { eventType = EventType.UnitStatChanged, data = new List<object> { unit.id, 1, 0, unit.health, unit.damage } });
            unit.heldItem = null;
        }
        // otherwise, the unit dies
        else
        {
            // Remove unit from field
            if (phase == Phase.PlayerUnits)
            {
                RemoveFromList(enemyUnits, unit.id);
            }
            else
            {
                RemoveFromList(playerUnits, unit.id);
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

    private static void HandleEnemyTurn(List<GameEvent> events)
    {
        // Make a list of the enemy units and order it by the unit's attack strength from lowest to highest
        List<Card> orderedEnemyUnits = enemyUnits.OrderBy(unit => unit.damage).ToList();

        // while the player has units alive, the enemy has units alive, and the enemy has units that can attack
        while (playerUnits.Count > 0 && enemyUnits.Count > 0 && orderedEnemyUnits.Count > 0)
        {
            Card weakestRemaining = orderedEnemyUnits[0];
            if (weakestRemaining.attacksRemaining <= 0 || weakestRemaining.health <= 0)
            {
                orderedEnemyUnits.RemoveAt(0);
                continue;
            }
            // Find the player unit with the lowest health and attack it
            Card weakestPlayerUnit = playerUnits.OrderBy(unit => unit.health).ToList()[0];
            List<GameEvent> attackEvents = AttackUnit(weakestRemaining.id, weakestPlayerUnit.id);
            events.AddRange(attackEvents);
        }

        List<GameEvent> endPhaseEvents = EndPhase();
        events.AddRange(endPhaseEvents);
    }

    private static List<Card> CardListCopy(List<Card> cards)
    {
        List<Card> copy = new List<Card>();
        foreach (Card card in cards)
        {
            copy.Add(card.Clone());
        }
        return copy;
    }
}

public enum Phase
{
    PlayerCards,
    PlayerUnits,
    EnemyUnits,
}

public class Card
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

    public Card Clone()
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
    HedgeHog,
    Monkey,
    Spider,
    Lion
}

public enum ItemType
{
    NotApplicable,
    Apple,      // Cure 3 hp if damaged and not dead
    SmokeBomb,  // Avoid all damage from attack
    Star,       // If killed, revive with 1 hp
    Dagger,     // +2 damage for the attack
    Coffee      // Gain an extra attack after attacking
}

public enum AbilityType
{
    None,
    Intimidate,     // If attacked, lower attackers atk by 2 (min 1)
    Curse,          // If attacked, set hp of attacker and defender to 0
    Wild,           // After attacking, attack and defense stats swap
    Bloodsucker,    // After attacking, regenerate 1 hp
    Lazy,           // Every turn the attack stat gets changed between 0 and the original value
    Swarm,          // Every turn this unit does not attack they gain +1 attack
    Spikey          // When attacked, deal 1 damage to the attacker
}