using UnityEngine;

public class MethodDemo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Greet();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Greet(string name = "Player")
    {
        Debug.Log($"Hello, {name}!");
    }
}
