using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleSineButton : MonoBehaviour
{
    [SerializeField] private TileMapController controller;
    private bool running;
    private void Awake()
        => GetComponent<Button>().onClick.AddListener(()=>{
            if (running) controller.StopSineWave();
            else         controller.StartSineWave();
            running=!running;
        });
}
