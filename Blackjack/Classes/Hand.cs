using Blackjack.Classes;
using Blackjack.Enums;

public class Hand
{
    public Guid Id { get; } = Guid.NewGuid();
    public List<Card> Cards { get; } = [];
    public HandResult Result { get; set; } = HandResult.Pending;
    public HandState State { get; set; } = HandState.Active;
    public bool IsBust => GetValue() > 21;
    public decimal Bet { get; set; }
    public decimal Payout { get; set; }
    public decimal Net => Payout - Bet;

    public void AddCard(Card card)
    {
        Cards.Add(card);
    }

    public int GetValue()
    {
        int value = 0;
        int aceCount = 0;

        foreach (var card in Cards)
        {
            value += card.Value;

            if (card.Rank == "Ace")
            {
                aceCount++;
            }
        }

        while (value > 21 && aceCount > 0)
        {
            value -= 10;
            aceCount--;
        }

        return value;
    }

    public bool IsSoft()
    {
        int value = 0;
        int aceCount = 0;

        foreach (var card in Cards)
        {
            value += card.Value;

            if (card.Rank == "Ace")
                aceCount++;
        }

        while (value > 21 && aceCount > 0)
        {
            value -= 10;
            aceCount--;
        }

        return aceCount > 0;
    }

    public Hand Split(Deck deck)
    {
        var card1 = Cards[0];
        var card2 = Cards[1];

        Cards.Clear();
        Cards.Add(card1);

        var newHand = new Hand();
        newHand.AddCard(card2);

        AddCard(deck.DrawCard());
        newHand.AddCard(deck.DrawCard());

        return newHand;
    }

    public bool CanSplit()
    {
        if (Cards.Count != 2)
            return false;

        return Cards[0].Rank == Cards[1].Rank;
    }

    public bool CanDouble()
    {
        return Cards.Count == 2;
    }

    public bool CanSurrender()
    {
        return Cards.Count == 2;
    }

    public string GetCardsAsString()
    {
        return string.Join(", ", Cards);
    }
}