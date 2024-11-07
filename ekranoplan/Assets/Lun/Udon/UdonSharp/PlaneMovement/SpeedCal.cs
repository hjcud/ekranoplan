
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SpeedCal : UdonSharpBehaviour
{
    private float engineThrust = 1019.2f * 1000f;                       // 1019.2 kN to Newton
    private float mass = 286000f;                                       // 286 tons to kg
    [UdonSynced(UdonSyncMode.Linear)] public float currentSpeed = 0f;   // Current Speed

    public Text Plane_Speed;

    public void CalculatePlaneSpeed(float throttle)
    {
        float yaw = 0;
        float pitch = 0;
        float roll = 0;

        float throttleForce = throttle * engineThrust;
        
        // Yaw, Pitch, Roll
        float yawEffect = Mathf.Abs(yaw) * 0.1f;
        float pitchEffect = Mathf.Abs(pitch) * 0.1f;
        float rollEffect = Mathf.Abs(roll) * 0.05f;

        float totalDragCoefficient = 0.02f + yawEffect + pitchEffect + rollEffect;
        float drag = totalDragCoefficient * Mathf.Pow(currentSpeed, 2) / mass;

        float acceleration = (throttleForce / mass) - drag;
        currentSpeed += acceleration * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0, 297f * 0.514f);
        RequestSerialization();
        UpdatePlaneSpeed();
    }

    public override void OnDeserialization()
    {
        UpdatePlaneSpeed();
    }

    public void UpdatePlaneSpeed()
    {
        Plane_Speed.text = currentSpeed.ToString();
    }
}
