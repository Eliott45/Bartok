using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// CBState включает состояния игры и состояния to... , описывающие движения
public enum CBState
{
    toDrawpile, 
    drawpile,
    toHand,
    hand,
    toTarget,
    target,
    discard,
    to,
    idle
}

public class CardBartok : Card
{
    // Статические переменные совместно используются всеми экземплярами CardBartok
    static public float MOVE_DURATION = 0.5f;
    static public string MOVE_EASING = Easing.InOut;
    static public float CARD_HEIGHT = 3.5f;
    static public float CARD_WIDTH = 2f;

    [Header("Set Dynamically: CardBartok")]
    public CBState state = CBState.drawpile;

    // Поля с информацией, необходимой для перемещения и поворачивания карты
    public List<Vector3> bezierPts;
    public List<Quaternion> bezierRots;
    public float timeStart, timeDuration;
    public int eventualSortOrder;
    public string eventualSortLayer;

    // По завершении перемещения карты будет вызываться reportFinishTo.SendMessage()
    public GameObject reportFinishTo = null;

    [System.NonSerialized]
    public Player callbackPlayer = null;

    /// <summary>
    /// Запускает перемещение карты в новое местоположение с заданным поворотом
    /// </summary>
    public void MoveTo(Vector3 ePos, Quaternion eRot)
    {
        // Создать новые списки для интерполяциии.
        // Траектории перемещения и поворота определяются двумя точками каждая
        bezierPts = new List<Vector3>();
        bezierPts.Add(transform.localPosition); // Текущие местоположение
        bezierPts.Add(ePos); // Новое местоположение

        bezierRots = new List<Quaternion>();
        bezierRots.Add(transform.rotation); // Текущий угол поворота
        bezierRots.Add(eRot); // Новый угол поворота

        if(timeStart == 0)
        {
            timeStart = Time.time;
        }

        timeDuration = MOVE_DURATION;
        state = CBState.to;
    }

    public void MoveTo(Vector3 ePos)
    {
        MoveTo(ePos, Quaternion.identity);
    }

    private void Update()
    {
        switch (state)
        {
            case CBState.toHand:
            case CBState.toTarget:
            case CBState.toDrawpile:
            case CBState.to:
                float u = (Time.time - timeStart) / timeDuration;
                float uC = Easing.Ease(u, MOVE_EASING);
                if(u < 0)
                {
                    transform.localPosition = bezierPts[0];
                    transform.rotation = bezierRots[0];
                    return;
                } else if (u >= 1)
                {
                    uC = 1;
                    // Перевести из состояния to в соответствующее
                    if (state == CBState.toHand) state = CBState.toHand;
                    if (state == CBState.toTarget) state = CBState.target;
                    if (state == CBState.toDrawpile) state = CBState.drawpile;
                    if (state == CBState.to) state = CBState.idle;

                    // Переместить в конечное положение
                    transform.localPosition = bezierPts[bezierPts.Count - 1];
                    transform.rotation = bezierRots[bezierPts.Count - 1];

                    // Сбросить time в 0, что бы в следующий раз можно было установить текущее время
                    timeStart = 0;

                    if (reportFinishTo != null)
                    {
                        reportFinishTo.SendMessage("CBCallback", this);
                        reportFinishTo = null;
                    }
                    else if (callbackPlayer != null)
                    {
                        // Если имеется ссылка на экземпляр Player
                        callbackPlayer.CBCallback(this);
                        callbackPlayer = null;
                    }
                    else
                    {
                        // Оставить все как есть.
                    }
                } else
                { // Нормальный режим интерполяции (0 <= u < 1)
                    Vector3 pos = Utils.Bezier(uC, bezierPts);
                    transform.localPosition = pos;
                    Quaternion rotQ = Utils.Bezier(uC, bezierRots);
                    transform.rotation = rotQ;

                    if( u > 0.5f)
                    {
                        SpriteRenderer sRend = spriteRenderers[0];
                        if(sRend.sortingOrder != eventualSortOrder)
                        {
                            // Установить конечный порядок сортировки
                            SetSortOrder(eventualSortOrder);
                        }
                        if(sRend.sortingLayerName != eventualSortLayer)
                        {
                            // Установить конечный слой сортировки
                            SetSortingLayerName(eventualSortLayer);
                        }
                    }
                }
                break;
        }
    }
}
