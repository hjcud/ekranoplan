
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
    [UdonSynced(UdonSyncMode.Linear)] public float planeYaw = 0f; 
    [UdonSynced(UdonSyncMode.Linear)] public float planePitch = 0f; 
    [UdonSynced(UdonSyncMode.Linear)] public float planeRoll = 0f; 

    [UdonSynced(UdonSyncMode.Linear)] public float dragAcceleration = 0f;
    [UdonSynced(UdonSyncMode.Linear)] public float acceleration = 0f;
    [UdonSynced(UdonSyncMode.Linear)] public float localGForce = 0f;
    [UdonSynced(UdonSyncMode.Linear)] public float liftForce = 0f;
    
    [UdonSynced(UdonSyncMode.Linear)] public float worldYaw = 0f; 
    [UdonSynced(UdonSyncMode.Linear)] public float worldPitch = 0f; 
    [UdonSynced(UdonSyncMode.Linear)] public float worldRoll = 0f;

    [UdonSynced(UdonSyncMode.Linear)] public Vector3 WorldPos = new Vector3(0f, 0f, 0f);

    public Transform FloorHnR;

    public Text Plane_Speed;
    public Text Plane_Yaw;
    public Text Plane_Pitch;
    public Text Plane_Roll;
    public Text Plane_drag;
    public Text Plane_acceleration;
    public Text Plane_G;
    public Text Plane_Lift;
    public Text World_Pitch;

    public Quaternion planeRotation = Quaternion.identity;
    public float localVelocity = 0;

    public float lastVelocity = 0;

    public void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        CalculateRotation(dt);
        CalculateVelocity();
        CalculateAngleOfAttack(dt);
        //CalculateState(dt);
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

        if (Mathf.Abs(planeYaw - yaw) < 0.001f)
            planeYaw = yaw;
        else planeYaw = Mathf.Lerp(planeYaw, yaw, lerpSpeed * dt);

        if (Mathf.Abs(planePitch - pitch) < 0.001f)
            planePitch = pitch;
        else planePitch = Mathf.Lerp(planePitch, pitch, lerpSpeed * dt);

        if (Mathf.Abs(planeRoll - roll) < 0.001f)
            planeRoll = roll;
        else planeRoll = Mathf.Lerp(planeRoll, roll, lerpSpeed * dt);

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

    public void CalculateAngleOfAttack(float dt)
    {
        // 목표 각도 계산
        float targetAngle = planePitch * 30f;

        float rotateAcceleration = planeVelocity - lastVelocity;
        localGForce = Mathf.Abs(targetAngle * rotateAcceleration) + 1; // 얼마나 회전 했는지 * 가속도 = G 중력 가속도?
        lastVelocity = planeVelocity;
        
        // 게임 물리 계산 편의상 기체의 각도가 아닌 목표 각도로 lift와 drag 계산.
        dragAcceleration += Mathf.Abs(Mathf.Sin(targetAngle * Mathf.Deg2Rad)) * 0.1f;
        liftForce = (acceleration - dragAcceleration) * Mathf.Cos(targetAngle * Mathf.Deg2Rad) * 0.1f;
        if (worldPitch + targetAngle < 0) liftForce *= -1;

        // 중력 가속도 계산
        //weightForce = 286000f * 9.81f;

        // lift 적용
        float currentHeight = FloorHnR.position.y;
        WorldPos = new Vector3(0f, currentHeight - liftForce > 0 ? 0 : currentHeight - liftForce, 0f);

        if (targetAngle != 0) worldPitch = Mathf.Lerp(worldPitch, worldPitch + targetAngle, 0.25f * dt);
        worldPitch = Mathf.Clamp(worldPitch, -45, 45);
    }

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
        Plane_G.text = localGForce.ToString();
        Plane_Lift.text = liftForce.ToString();

        World_Pitch.text = worldPitch.ToString();

        FloorHnR.position = WorldPos;
        FloorHnR.eulerAngles = new Vector3(worldPitch, 0f, 0f);
    }
}
