
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class StateCal : UdonSharpBehaviour
{
    public Throttle_Controll Throttle_Controll;
    public Controller_Controll Controller_Controll;

    [UdonSynced(UdonSyncMode.Linear)] public float planeVelocity = 0f;  // Current Speed
    [UdonSynced(UdonSyncMode.Linear)] public float planePitch = 0f; 
    [UdonSynced(UdonSyncMode.Linear)] public float planeYaw = 0f; 
    [UdonSynced(UdonSyncMode.Linear)] public float planeRoll = 0f; 

    [UdonSynced(UdonSyncMode.Linear)] public float dragAcceleration = 0f;
    [UdonSynced(UdonSyncMode.Linear)] public float acceleration = 0f;
    
    [UdonSynced(UdonSyncMode.Linear)] public float worldPitch = 0f; 
    [UdonSynced(UdonSyncMode.Linear)] public float worldYaw = 0f; 
    [UdonSynced(UdonSyncMode.Linear)] public float worldRoll = 0f; 

    public Text Plane_Speed;
    public Text Plane_Yaw;
    public Text Plane_Pitch;
    public Text Plane_Roll;
    public Text Plane_drag;
    public Text Plane_acceleration;

    public Quaternion planeRotation = Quaternion.identity;
    public float localVelocity = 0;

    public void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        CalculateRotation(dt);
        CalculateVelocity();
        //CalculateState(dt);
        //CalculateAngleOfAttack();
        //CalculateGForce(dt);

        RequestSerialization();
        UpdatePlaneSpeed();
    }

    public void CalculateRotation(float dt)
    {
        float lerpSpeed = 1.25f;

        float yaw = Controller_Controll.yaw;
        float pitch = Controller_Controll.pitch;
        float roll = Controller_Controll.roll;

        /*************/
        //Bug: planeYaw = 0 && yaw = 0 >> StrangeNumber
        /*************/
        planeYaw = Mathf.Lerp(planeYaw, yaw, lerpSpeed * dt);
        planePitch = Mathf.Lerp(planePitch, pitch, lerpSpeed * dt);
        planeRoll = Mathf.Lerp(planeRoll, roll, lerpSpeed * dt);

        /* Old Way
        planeYaw += yaw < 0 ? -changeVol : changeVol;
        planeYaw = Mathf.Clamp(planeYaw, -yaw, yaw);
        planePitch += pitch < 0 ? -changeVol : changeVol;
        planePitch = Mathf.Clamp(planePitch, -pitch, pitch);
        planeRoll += roll < 0 ? -changeVol : changeVol;
        planeRoll = Mathf.Clamp(planeRoll, -roll, roll);
        */
    }

    public void CalculateVelocity()
    {
        // DragForce = 1/2*pv^2AC
        float dragConstant = 1.96f;                                 // 0.5(1/2) * air density(p: 1.225) * surface area(A: 80) * drag coefficient(C: 0.04)
        float speedSquared = Mathf.Pow(planeVelocity, 2);           // v^2
        dragAcceleration = speedSquared * dragConstant / 286000f;   // mass of an airplane (286ton = 286000kg)

        float throttle = Throttle_Controll.throttlePower;           // throttle Range -0.3 ~ 1.2
        acceleration = throttle * 101920f * 8f / 286000f * 0.581f;  // a = F/m * Output Ratio (F = Power of each engine * Number of engines)

        planeVelocity += (acceleration - dragAcceleration) * 0.1f;  // Acceleration - Drag
    }

    public void CalculateAngleOfAttack()
    {
        float AngleOfAttackPitch = 0;
        float AngleOfAttackYaw = 0;

        Mathf.Atan2(AngleOfAttackPitch, worldPitch);
        Mathf.Atan2(AngleOfAttackYaw, worldYaw);
    }

    /*
    public void CalculateState(float dt)
    {
        var invRotation = Quaternion.Inverse(planeRotation);
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
        Plane_Speed.text = planeVelocity.ToString();

        Plane_Yaw.text = planeYaw.ToString();
        Plane_Pitch.text = planePitch.ToString();
        Plane_Roll.text = planeRoll.ToString();

        Plane_drag.text = dragAcceleration.ToString();
        Plane_acceleration.text = acceleration.ToString();
    }
}
