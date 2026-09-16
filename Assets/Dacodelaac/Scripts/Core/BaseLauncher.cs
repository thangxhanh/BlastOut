using System.Collections.Generic;
using UnityEngine;

namespace Dacodelaac.Core
{
    public class BaseLauncher : BaseMono
    {
        [SerializeField] private GameObject[] prefabs;

        public override void Initialize()
        {
            base.Initialize();
            GetPools().Initialize();

            /* Spawn, bind variables, listen to events */
            var list = new List<BaseMono>();
            foreach (var prefab in prefabs)
            {
                var monoes = Instantiate(prefab).GetComponentsInChildren<BaseMono>(true);
                list.AddRange(monoes);
            }

            /* Initialize */
            foreach (var mono in list)
            {
                mono.Initialize();
            }
        }
    }
}