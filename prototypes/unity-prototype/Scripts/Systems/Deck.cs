using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 50장 덱을 관리하는 클래스.
/// 셔플, 드로우, 이월 처리.
/// </summary>
public class Deck
{
    private List<CardData> drawPile;   // 드로우 더미 (아직 안 뽑은 카드)
    private List<CardData> hand;       // 현재 손패
    private List<CardData> discard;    // 사용한 카드 (이번 게임에서 재사용 안 함)

    public List<CardData> Hand => hand;
    public int DrawPileCount => drawPile.Count;

    public Deck(List<CardData> allCards)
    {
        drawPile = new List<CardData>(allCards);
        hand = new List<CardData>();
        discard = new List<CardData>();
        Shuffle();
    }

    /// <summary>
    /// 드로우 더미를 셔플
    /// </summary>
    public void Shuffle()
    {
        for (int i = drawPile.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
        }
    }

    /// <summary>
    /// 카드를 N장 드로우해서 손패에 추가
    /// </summary>
    public List<CardData> Draw(int count)
    {
        List<CardData> drawn = new List<CardData>();

        for (int i = 0; i < count; i++)
        {
            if (drawPile.Count == 0) break;

            CardData card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);
            drawn.Add(card);
        }

        return drawn;
    }

    /// <summary>
    /// 손패에서 카드를 배치에 사용 (discard로 이동)
    /// </summary>
    public void PlayCard(CardData card)
    {
        hand.Remove(card);
        discard.Add(card);
    }

    /// <summary>
    /// 현재 손패 장수
    /// </summary>
    public int HandCount => hand.Count;
}
