using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Reward : MonoBehaviour
{
    [SerializeField] bool TimedReward = true;
    [SerializeField] float TimePeriod = 2.5f;
    public bool CanGet;
    public List<int> Rewards;

    float TimeAvailable;

    private void Update()
    {
        if (TimedReward)
        {
            if (Time.time >= TimeAvailable)
            {
                bool[] Checks = new bool[Rewards.Count];

                for (int c = 0; c < Rewards.Count; c++)
                {
                    if ((GameManager.Instance._Stats[c] + Rewards[c]) < GameManager.Instance.StatsMax)
                        Checks[c] = true;
                }
                if (!Checks.Contains(false))
                    CanGet = true;
                else
                    CanGet = false;
                TimeAvailable = Time.time + TimePeriod;
            }
            else
                CanGet = false;
        }
        else
        {
            bool[] Checks = new bool[Rewards.Count];

            for (int c = 0; c < Rewards.Count; c++)
            {
                if ((GameManager.Instance._Stats[c] + Rewards[c]) < GameManager.Instance.StatsMax)
                    Checks[c] = true;
            }
            if (!Checks.Contains(false))
                CanGet = true;
            else
                CanGet = false;
        }
    }

    public void GetRewards()
    {
        if (CanGet)
        {
            for (int c = 0; c < Rewards.Count; c++)
            {
                GameManager.Instance._Stats[c] += Rewards[c];
            }
        }
    }
}
