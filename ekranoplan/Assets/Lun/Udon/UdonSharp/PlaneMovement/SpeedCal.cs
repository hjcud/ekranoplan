
using UdonSharp;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SpeedCal : UdonSharpBehaviour
{
    public Throttle_Controll Throttle_Controll;
    public Controller_Controll Controller_Controll;

    private float engineThrust = 1.0192f;                                   // 1.0192 Newton
    private float mass = 286000f;                                           // 286 tons to kg
    [UdonSynced(UdonSyncMode.Linear)] public float currentVelocity = 0f;    // Current Speed
    [UdonSynced(UdonSyncMode.Linear)] public float drag = 0f;
    [UdonSynced(UdonSyncMode.Linear)] public float acceleration = 0f;

    public Text Plane_Speed;
    public Text Plane_drag;
    public Text Plane_acceleration;

    public Quaternion planeRotation = Quaternion.identity;
    public float planeVelocity = 0;
    public float localVelocity = 0;

    public void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        CalculateVelocity();
        //CalculateState(dt);
        //CalculateAngleOfAttack();
        //CalculateGForce(dt);
    }

    public void CalculateVelocity()
    {
        // throttle Range -0.3 ~ 1.2
        float throttle = Throttle_Controll.throttlePower;

        currentVelocity += throttle * 0.1f;
        Mathf.Clamp(currentVelocity, 0, 550);
        RequestSerialization();
        UpdatePlaneSpeed();
    }

    /*
    public void CalculateState(float dt)
    {
        var invRotation = Quaternion.Inverse(planeRotation);
    }

    public void CalculateAngleOfAttack()
    {

    }

    public void CalculateGForce(float dt)
    {

    }
    */

    public override void OnDeserialization()
    {
        UpdatePlaneSpeed();
    }

    public void UpdatePlaneSpeed()
    {
        Plane_Speed.text = currentVelocity.ToString();
        Plane_drag.text = drag.ToString();
        Plane_acceleration.text = acceleration.ToString();
    }
}
