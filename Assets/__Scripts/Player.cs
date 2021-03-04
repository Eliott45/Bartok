using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Игрок может быть ИИ или человеком
/// </summary>
public enum PlayerType
{
	human,
	ai
}

[System.Serializable]
public class Player 
{
	public PlayerType type = PlayerType.ai;
	public int playerNum;
	public SlotDef handSlotDef;
	public List<CardBartok> hand; // Карты в руках игрока

	/// <summary>
	/// Добавить карту в руки
	/// </summary>
	public CardBartok AddCard(CardBartok eCB)
    {
		if (hand == null) hand = new List<CardBartok>();

		// Добавить карту 
		hand.Add(eCB);
		return (eCB);
    }

	// Удаляет карту из рук
	public CardBartok RemoveCard(CardBartok cb)
    {
		// Если список hand пуст или не содержит карты cb, вернуть null
		if (hand == null || !hand.Contains(cb)) return null;
		hand.Remove(cb);
		return (cb);
    } 
}
