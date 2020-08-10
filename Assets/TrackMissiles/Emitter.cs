using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xx
{
	public class Emitter : MonoBehaviour
	{
        public GameObject missile;

        float currentTime;

		void Update()
		{
            currentTime += Time.deltaTime;
            if(currentTime > 2)
            {
                currentTime = 0;
                GameObject m = GameObject.Instantiate(missile);
                m.transform.localPosition = Vector3.zero;
                m.SetActive(true);
            }
        }
	}
}

