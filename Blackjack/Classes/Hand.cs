using Blackjack.Classes;
using Blackjack.Enums;

public class Hand
{
    public List<Card> Cards { get; } = [];
    public HandResult Result { get; set; } = HandResult.Pending;
    public bool IsBust => GetValue() > 21;

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

    public string GetCardsAsString()
    {
        return string.Join(", ", Cards);
    }
}