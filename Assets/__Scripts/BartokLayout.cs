using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>(); // Не используется в Bartok
    public float rot; // Поворот в зависимости от игрока
    public string type = "slot";
    public Vector2 stagger;
    public int player; // Порядковый номер игрока
    public Vector2 pos; // Вычисляется на основе x,y и multiplier
}

public class BartokLayout : MonoBehaviour
{
    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml; // Используется для ускорения доступа к xml
    public Vector2 multiplier; // Смещение в раскладке

    // Ссылки на SlotDef
    public List<SlotDef> slotDefs; // Список SlotDef для игроков
    public SlotDef drawPile;
    public SlotDef discardPile;
    public SlotDef target;

    /// <summary>
    /// Вызывается для чтения файла BartokLayoutXML.xml
    /// </summary>
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText); // Загрузить XML
        xml = xmlr.xml["xml"][0]; // И определяется xml для ускорения доступа к XML


        // Прочитать множители, определяющие расстояние между картами
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"), CultureInfo.InvariantCulture);
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"), CultureInfo.InvariantCulture);

        // Прочитать слоты
        SlotDef tSD;
        // slotsX используется для ускорения доступа к элемментам <slot>
        PT_XMLHashList slotsX = xml["slot"];

        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef(); // Создать новый экземпляр SlotDef
            if (slotsX[i].HasAtt("type"))
            {
                // Если <slot> имеет атрибут type, прочитать его
                tSD.type = slotsX[i].att("type");
            }
            else
            {
                // Иначе определить тип как "slot" это отдельная карта в ряду
                tSD.type = "slot";
            }
            // Преобразовать некоторые атрибуты в числовые значения
            tSD.x = float.Parse(slotsX[i].att("x"), CultureInfo.InvariantCulture);
            tSD.y = float.Parse(slotsX[i].att("y"), CultureInfo.InvariantCulture);
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);

            // Слои сортировки
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();

            // Прочитать дополнительные атрибуты, опираясь на тип слота
            switch (tSD.type)
            {
                case "slot":
                    // Игнорировать
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"), CultureInfo.InvariantCulture);
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
                case "target":
                    target = tSD;
                    break;
                case "hand":
                    tSD.player = int.Parse(slotsX[i].att("player"));
                    tSD.rot = int.Parse(slotsX[i].att("rot"));
                    slotDefs.Add(tSD);
                    break;
            }
        }
    }
}
