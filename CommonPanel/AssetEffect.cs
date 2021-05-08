using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetEffect : MonoBehaviour
{
    private Vector3 startPos = Vector3.zero;
    public float[] heights;
    public float movingTime = 0f;

    private List<ParticleSystem> effects;
    private float lifeTime = 0;

    public Net.REWARD_TYPE rewardtype;

    public void Init(Net.REWARD_TYPE rewardtype, Vector3 pos)
    {
        startPos = pos;
        transform.position = pos;
        this.rewardtype = rewardtype;

        if (effects == null)
        {
            effects = new List<ParticleSystem>();
            for (int i = 0, iMax = transform.childCount; i < iMax; i++)
            {
                Transform child = transform.GetChild(i);

                effects.Add(child.GetComponent<ParticleSystem>());
                lifeTime += child.GetComponent<ParticleSystem>().main.duration;
            }
        }
    }

    float moveSpeed = 5f;
    float accSpeed = 1.2f;
    public void MoveTarget(Vector3 pos, float startSpeed, float acc)
    {
        gameObject.SetActive(true);

        for (int i = 0, iMax = effects.Count; i < iMax; i++)
            effects[i].Play();
        moveSpeed = startSpeed;
        accSpeed = acc;
        StartCoroutine(StartMove(pos));
    }

    private IEnumerator StartMove(Vector3 pos)
    {
        yield return new WaitForSeconds(UIManager.Inst.UIData.accStartWaitTime);
        Vector3 target = pos;
        target.z = transform.position.z;
        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * moveSpeed);
            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                StartCoroutine(EndEffect());
                LobbyPanel lobbyPanel = UIManager.Inst.GetActivePanel<LobbyPanel>(UI.UIPanel.LobbyPanel);
                //lobbyPanel.SetAssetGetEffect(rewardtype);
                lobbyPanel.SetDestinationAssetEffect(rewardtype);
                break;
            }
            moveSpeed *= accSpeed;
            yield return 0;
        }
    }

    private IEnumerator EndEffect()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
        transform.position = startPos;
    }
}
