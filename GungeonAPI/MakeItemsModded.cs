using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace GungeonAPI
{
    class MakeItemsModded : ETGModule
    {
        public float weight;
        public override void Exit()
        {
        }
        public override void Start()
        {
        }
        public override void Init()
        {
            weight = 1000;
            foreach (WeightedGameObject obj in GameManager.Instance.RewardManager.GunsLootTable.defaultItemDrops.elements)
            {
                if (obj.pickupId > 823 || obj.pickupId < 0)
                {
                    obj.weight *= this.weight;
                }
            }

            foreach (WeightedGameObject obj in GameManager.Instance.RewardManager.ItemsLootTable.defaultItemDrops.elements)
            {
                if (obj.pickupId > 823 || obj.pickupId < 0)
                {
                    obj.weight *= this.weight;
                }


            }
        }
      
       
    }
}
