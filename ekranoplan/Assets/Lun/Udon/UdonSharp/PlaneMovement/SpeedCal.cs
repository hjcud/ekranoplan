
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SpeedCal : UdonSharpBehaviour
{
    public Throttle_Controll Throttle_Controll;
    public Controller_Controll Controller_Controll;

    private float engineThrust = 1.0192f;                       // 1.0192 Newton
    private float mass = 286000f;                                       // 286 tons to kg
    [UdonSynced(UdonSyncMode.Linear)] public float currentSpeed = 0f;   // Current Speed
    [UdonSynced(UdonSyncMode.Linear)] public float drag = 0f;
    [UdonSynced(UdonSyncMode.Linear)] public float acceleration = 0f;

    public Text Plane_Speed;
    public Text Plane_drag;
    public Text Plane_acceleration;

    public void Update()
    {
        CalculatePlaneSpeed();
    }

    public void CalculatePlaneSpeed()
    {
        float throttle = Throttle_Controll.Throttle_Rotation;
        float yaw = Controller_Controll.yaw;
        float pitch = Controller_Controll.pitch;
        float roll = Controller_Controll.roll;

        float throttleForce = throttle * engineThrust;
        
        // Yaw, Pitch, Roll
        float yawEffect = Mathf.Abs(yaw) * 0.1f;
        float pitchEffect = Mathf.Abs(pitch) * 0.1f;
        float rollEffect = Mathf.Abs(roll) * 0.05f;

        float totalDragCoefficient = 0.02f + yawEffect + pitchEffect + rollEffect;
        drag = totalDragCoefficient * Mathf.Pow(currentSpeed, 2) / mass;

        acceleration = (throttleForce / mass) - drag;
        currentSpeed += acceleration * Time.deltaTime;

        // 1 knot = 1.852 km/h
        currentSpeed = Mathf.Clamp(currentSpeed, 0, 297f * 1.852f);
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
        Plane_drag.text = drag.ToString();
        Plane_acceleration.text = acceleration.ToString();
    }
}
