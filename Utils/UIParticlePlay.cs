using UnityEngine;
using System.Collections.Generic;

public class UIParticlePlay
{
    public GameObject Go { get { return go; } }
    public bool IsLoop { get { return isLoop; } }
    public float LifeTime { get { return lifeTime; } }
    public float PlayTime { get { return playTime; } }

    private Transform parent;
    private string path;

    private List<ParticleSystem> effects;

    private GameObject go;
    private bool isLoop = false;
    private float lifeTime = 0;
    private float playTime = 0;

    public UIParticlePlay(Transform parent, string path)
    {
        this.parent = parent;
        this.path = path;
        SetParticle(path);
    }
    public void SetPosition(Vector3 pos)
    {
        if (go == null)
            return;
        go.transform.position = pos;
    }

    public void Play(bool is_loop = false)
    {
        if (null != go) go.SetActive(true);

        isLoop = is_loop;
        playTime = 0;

        if (effects == null) SetParticle(path);

        bool isNull = false;
        for (int i = 0, iMax = effects.Count; i < iMax; i++)
        {
            if (effects[i] == null)
                isNull = true;
        }
        if(isNull == true)
            SetParticle(path);

        for (int i = 0, iMax = effects.Count; i < iMax; i++)
        {
            if (effects[i] == null)
                continue;
            effects[i].Play();
        }
    }

    public void UpdatePlayTime(float added_time)
    {
        if (false == go.activeSelf)
            return;

        playTime += added_time;
    }

    public void Stop()
    {
        if (effects == null) SetParticle(path);

        for (int i = 0, iMax = effects.Count; i < iMax; i++)
            effects[i].Stop();

        if (null != go) go.SetActive(false);
    }

    private void SetParticle(string path, bool is_pooled = true)
    {
        if(effects == null)
            effects = new List<ParticleSystem>();
        effects.Clear();

        GameObject particle = UIResourceManager.Inst.Load<GameObject>(path);
        GameObject ins = GameObject.Instantiate(particle) as GameObject;

        ins.transform.SetParent(parent);
        Transform tfParticle = ins.transform;
        tfParticle.localPosition = new Vector3(0, 0, 0);
        go = ins;
        lifeTime = 0;

        for (int i = 0, iMax = tfParticle.childCount; i < iMax; i++)
        {
            Transform child = tfParticle.GetChild(i);

            effects.Add(child.GetComponent<ParticleSystem>());
            lifeTime += child.GetComponent<ParticleSystem>().main.duration;
        }
    }

    public void SetColor(Color color)
    {
        for (int i = 0, iMax = effects.Count; i < iMax; i++)
        {
            ParticleSystem.MainModule main = effects[i].main;
            main.startColor = color;
        }
    }

    public void SetParticleLayer(int layer)
    {
        UIUtil.SetLayer(Go.transform, layer);
    }
}

