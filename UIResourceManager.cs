using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Libs.Unity;
public class UIResourceManager : MonoSingleton<UIResourceManager>
{
    // 로드 된 리소스 리스트
    private Dictionary<string, Object> resDic = new Dictionary<string, Object>();

    void Awake()
    {
        resDic.Clear();
    }
    // 리소스 로드 후 resources 에 등록. 
    public T Load<T>(string name) where T : Object
    {
        string resourceName = name.Trim();
        T t = null;
        if (IsExist(resourceName))
            t = (T)resDic[resourceName];
        else
        {
            t = Resources.Load<T>(resourceName);
            if (t != null)
                resDic.Add(resourceName, t);
        }

        if (t == null)
        {
#if UNITY_EDITOR
            string log = string.Format("ResourceManager ------ resource load fail........name is {0}", resourceName);
            Debug.LogErrorFormat(log);
#endif
        }

        return t;
    }
    
    // 로드된 리소스 리스트에 해당 이름의 리소스가 존재하는지 체크
    public bool IsExist(string name)
    {
        return resDic.ContainsKey(name);
    }
}
