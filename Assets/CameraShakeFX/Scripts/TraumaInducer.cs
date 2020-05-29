 using UnityEngine;
using System.Collections;

/* Example script to apply trauma to the camera or any game object */
public class TraumaInducer : MonoBehaviour 
{
    [Tooltip("Maximum stress the effect can inflict upon objects Range([0,1])")]
    public float maximumStress = 0.6f;
    [Tooltip("Maximum distance in which objects are affected by this TraumaInducer")]
    public float range = 45;

    public void GenerateTrauma()
    {
        /* Find all gameobjects in the scene and loop through them until we find all the nearvy stress receivers */
        var targets = FindObjectsOfType<GameObject>();
        for(int i = 0; i < targets.Length; ++i)
        {
            var receiver = targets[i].GetComponent<StressReceiver>();
            if(receiver == null) continue;
            float distance = Vector3.Distance(transform.position, targets[i].transform.position);
            /* Apply stress to the object, adjusted for the distance */
            if(distance > range) continue;
            float distance01 = Mathf.Clamp01(distance / range);
            float stress = (1 - Mathf.Pow(distance01, 2)) * maximumStress;
            receiver.InduceStress(stress);
        }
    }

}