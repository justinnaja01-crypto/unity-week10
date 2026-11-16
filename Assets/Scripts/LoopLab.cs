using System.Threading;
using UnityEngine;

public class LoopLab : MonoBehaviour
{
    [SerializeField] int energy = 0;
    void Start()
    {
        //while (energy > 0)
        //{
        //    Debug.Log("while round");
        //    energy--;
        //}
        //do
        //{
        //    Debug.Log("while: round");
        //    energy--;
        //} while (energy < 0);

        while (energy < 50)
        {
            if (energy == 15)
            {
                Debug.Log("Exit Energy Loop");
                break;
            }
            energy++;
            Debug.Log($"Energy: {energy}");
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
