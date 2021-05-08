using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using App.UI;
using Platform;
using Net;
using Net.Api;

public class BattleRecordList : BaseList
{
    public void SetList(List<GameOpponent> records)
    {
        if (scrollRect == null)
            scrollRect = gameObject.GetComponent<LoopScrollRect>();

        var data = new object[records.Count];
        for (int i = 0; i < records.Count; i++)
        {
            data[i] = records[i];
        }

        scrollRect.prefabSource.Parent = gameObject;
        scrollRect.totalCount = records.Count;
        scrollRect.objectsToFill = data;
        scrollRect.prefabSource.prefabName = "UI/Prefab/Lobby/BattleRecordItem";
        scrollRect.RefillCells();
    }
}
