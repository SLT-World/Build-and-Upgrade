using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public Reward _Reward;

    private void Start()
    {
        _Reward = GetComponent<Reward>();
    }

    public void GetRewards()
    {
        _Reward.GetRewards();
    }
}
