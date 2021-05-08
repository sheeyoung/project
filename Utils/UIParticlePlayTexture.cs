using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParticlePlayTexture : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> textureChangeObjects;
    public bool IsLoop { get { return isLoop; } }
    public float LifeTime { get { return lifeTime; } }
    public float PlayTime { get { return playTime; } }

    private Transform parent;

    private List<ParticleSystem> effects;

    private bool isLoop = false;
    private float lifeTime = 0;
    private float playTime = 0;

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    public void Play(Sprite changeSprite, bool is_loop = false)
    {
        gameObject.SetActive(true);

        isLoop = is_loop;
        playTime = 0;

        for (int i = 0; i < textureChangeObjects.Count; i++)
        {
            textureChangeObjects[i].textureSheetAnimation.SetSprite(0, changeSprite);
        }

        for (int i = 0, iMax = effects.Count; i < iMax; i++)
        {
            if (effects[i] == null)
                continue;
            effects[i].Play();
        }
    }

    public void Stop()
    {
        for (int i = 0, iMax = effects.Count; i < iMax; i++)
            effects[i].Stop();
        gameObject.SetActive(false);
    }

    public void SetParticle(Transform parent, bool is_pooled = true)
    {
        this.parent = parent;

        effects = new List<ParticleSystem>();
        
        transform.SetParent(parent);
        transform.localPosition = new Vector3(0, 0, 0);

        lifeTime = 0;

        for (int i = 0, iMax = transform.childCount; i < iMax; i++)
        {
            Transform child = transform.GetChild(i);

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
}

