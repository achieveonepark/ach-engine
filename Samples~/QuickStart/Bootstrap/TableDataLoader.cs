using AchEngine.DI;
using AchEngine.Table;
using UnityEngine;

namespace AchEngine.Samples
{
    /// <summary>
    /// ?뚯씠釉??곗씠??濡쒕뱶 & 議고쉶 ?덉떆.
    /// ?ㅼ젣 ?꾨줈?앺듃?먯꽌??TableLoaderGenerated.LoadAll()???ъ슜?섏꽭??
    /// </summary>
    public class TableDataLoader : MonoBehaviour
    {
        private void Start()
        {
            // ?몃씪??JSON?쇰줈 ?섑뵆 ?곗씠??濡쒕뱶 (?ㅼ젣 ?ъ슜 ??.bytes/.json ?뚯씪 ?ъ슜)
            TableManager.LoadFromJsonText<MonsterData>(
                "[" +
                "{\"Id\":1,\"Name\":\"?щ씪??",\"Hp\":100,\"Speed\":1.5}," +
                "{\"Id\":2,\"Name\":\"怨좊툝由?",\"Hp\":250,\"Speed\":2.0}," +
                "{\"Id\":3,\"Name\":\"?쒕옒怨?",\"Hp\":9999,\"Speed\":3.5}" +
                "]"
            );

            // ?⑥씪 議고쉶
            var slime = TableManager.Get<MonsterData>(1);
            Debug.Log($"[TableSample] ID 1: {slime.Name} (HP:{slime.Hp})");

            // ?덉쟾??議고쉶
            if (TableManager.TryGet<MonsterData>(99, out var boss))
                Debug.Log($"[TableSample] 蹂댁뒪: {boss.Name}");
            else
                Debug.Log("[TableSample] ID 99 紐ъ뒪???놁쓬");

            // ?꾩껜 ?쒗쉶
            foreach (var m in TableManager.GetAll<MonsterData>())
                Debug.Log($"[TableSample] 紐ъ뒪?? {m.Name} / HP:{m.Hp} / ?띾룄:{m.Speed}");

            // VContainer ITableService 吏곸젒 ?ъ슜
            // var db = tableService.Database;
            // var allMonsters = db.GetAll<MonsterData>();
        }
    }
}
