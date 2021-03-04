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

		// Если это человек, отсортировать карты по достоинству с помощью LINQ
		if(type == PlayerType.human)
        {
			CardBartok[] cards = hand.ToArray();

			// Это вызов LINQ
			cards = cards.OrderBy(cd => cd.rank).ToArray();

			hand = new List<CardBartok>(cards);
        }
		FanHand();
		return (eCB);
    }

	// Удаляет карту из рук
	public CardBartok RemoveCard(CardBartok cb)
    {
		// Если список hand пуст или не содержит карты cb, вернуть null
		if (hand == null || !hand.Contains(cb)) return null;
		hand.Remove(cb);
		FanHand();
		return (cb);
    } 

	public void FanHand()
    {
		// Угол поворота первой карты относительно z
		float startRot = 0;
		startRot = handSlotDef.rot;
		if(hand.Count > 1)
        {
			startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
        }

		// Переместить все карты в новые позиции
		Vector3 pos;
		float rot;
		Quaternion rotQ;
		for(int i = 0; i < hand.Count; i++)
        {
			rot = startRot - Bartok.S.handFanDegrees * i;
			rotQ = Quaternion.Euler(0, 0, rot);

			pos = Vector3.up * CardBartok.CARD_HEIGHT / 2;

			pos = rotQ * pos;

			// Прбивать координаты позиции руки игрока (внизу в центре веера карт)
			pos += handSlotDef.pos;
			pos.z = -0.5f * i;

			// Установить локальную позицию и поворот i-й карты руках
			hand[i].MoveTo(pos, rotQ); // Сообщить карте, что она должна начать интерполяцию
			hand[i].state = CBState.toHand; // Закончив перемещение, карта запишет в поле state значении
			
			/*
			hand[i].transform.localPosition = pos;
			hand[i].transform.rotation = rotQ;
			hand[i].state = CBState.hand;
			*/

			hand[i].faceUp = (type == PlayerType.human);

			// Установить SortOrder карт, чтобы обеспечить правильное перекрытие
			hand[i].SetSortOrder(i * 4);
        }
    }
}
