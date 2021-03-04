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

		eCB.SetSortingLayerName("10"); // Перенести перемещаемую карту в верхниий слой
		eCB.eventualSortLayer = handSlotDef.layerName;

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
			hand[i].eventualSortOrder = i * 4;
			// hand[i].SetSortOrder(i * 4);
		}
	}

	/// <summary>
	/// ИИ для игроков
	/// </summary>
	public void TakeTurn()
	{
		Utils.tr(Utils.RoundToPlaces(Time.time), "Player.TakeTurn");

		// Ничего не делать для игрока-человека
		if (type == PlayerType.human) return;

		Bartok.S.phase = TurnPhase.waiting;

		CardBartok cb;

		// Если этим игроком управляет ПК, нужно выбрать карту для хода, найти допустимые ходы
		List<CardBartok> validCards = new List<CardBartok>();
		foreach (CardBartok tCB in hand)
		{
			if (Bartok.S.ValidPlay(tCB))
			{
				validCards.Add(tCB);
			}
		}
		// Если нет допустимых ходов
		if (validCards.Count == 0)
		{
			// ...then draw a card
			cb = AddCard(Bartok.S.Draw());
			cb.callbackPlayer = this;
			return;
		}

		// Выбрать одну из возможных карт которую можно сыграть
		cb = validCards[Random.Range(0, validCards.Count)];
		RemoveCard(cb);
		Bartok.S.MoveToTarget(cb);
		cb.callbackPlayer = this;
	}

	public void CBCallback(CardBartok tCB)
    {
		Utils.tr(Utils.RoundToPlaces(Time.time), "Player.CBCallback()", tCB.name, "Player " + playerNum);
		// Карта завершила перемещение 
		Bartok.S.PassTurn();
	}
}
