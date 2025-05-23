using UnityEngine;

public class FansControllerModule : MonoBehaviour
{
    public GameObject upperFan;
    public GameObject lowerFan;

    public ParticleSystem upperFanParticles;
    public ParticleSystem lowerFanParticles;

    public float rotationSpeed = 200f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        upperFanParticles.Stop();
        lowerFanParticles.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalSettings.Instance.UpperFanStatus)
        {
            // Rotate the upper fan around its own X-axis
            upperFan.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime, Space.World);
        }

        if (GlobalSettings.Instance.LowerFanStatus)
        {
            // Rotate the upper fan around its own X-axis
            lowerFan.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    void OnEnable()
    {
        EventCenter.Controls.OnTurnOnUpperFan += OnTurnOnUpperFan;
        EventCenter.Controls.OnTurnOffUpperFan += OnTurnOffUpperFan;
        EventCenter.Controls.OnTurnOnLowerFan += OnTurnOnLowerFan;
        EventCenter.Controls.OnTurnOffLowerFan += OnTurnOffLowerFan;
    }

    void OnDisable()
    {
        EventCenter.Controls.OnTurnOnUpperFan -= OnTurnOnUpperFan;
        EventCenter.Controls.OnTurnOffUpperFan -= OnTurnOffUpperFan;
        EventCenter.Controls.OnTurnOnLowerFan -= OnTurnOnLowerFan;
        EventCenter.Controls.OnTurnOffLowerFan -= OnTurnOffLowerFan;
    }

    void OnTurnOnUpperFan()
    {
        upperFanParticles.Play();
    }

    void OnTurnOffUpperFan()
    {
        upperFanParticles.Stop();
    }

    void OnTurnOnLowerFan()
    {
        lowerFanParticles.Play();
    }

    void OnTurnOffLowerFan()
    {
        lowerFanParticles.Stop();
    }
}
