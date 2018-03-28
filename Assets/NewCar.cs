﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewCar : MonoBehaviour {
    private Rigidbody _rigidbody;

    public float Throttle;
    public float Brake;
    public bool EBrake;

    public float EngineForce;
    public float BrakeConstant;
    public float DragConstant;
    public float RollingResistanceConstant;
    public float WheelRadius = 0.33f;
    public RaySusp[] FrontWheels, RearWheels;
    public Transform Chassis;
    public float SteerAngle;
    public float CorneringStiffnessFront;
    public float CorneringStiffnessRear;
    public float MaxGrip;


    private Vector2 _carVelocity;
    private float _speed;
    private float _weightFront;
    private float _weightRear;
    private Vector2 _carAcceleration;
    private float _wheelAngularVelocity;
    private float _percentFront;
    private float _totalWeight;
    private Vector2 _fTraction;
    private float _fTractionMax;
    private float _slipLongitudal;
    private float _b, _c;
    private float _heightRatio;



    // Use this for initialization
    void Start () {
        _rigidbody = GetComponent<Rigidbody>();
        _b = Mathf.Abs(FrontWheels[0].transform.localPosition.z);
        _c = Mathf.Abs(RearWheels[0].transform.localPosition.z);
        _heightRatio = 2.0f / (_b + _c);
    }
	
	// Update is called once per frame
	void Update () {
        UpdateCar();
	}

    void UpdateCar()
    {
        var allWheelsGrounded = FrontWheels.All(x => x.Grounded) && RearWheels.All(x => x.Grounded);
        if (!allWheelsGrounded)
            return;

        var vel3d = transform.InverseTransformVector(_rigidbody.velocity);
        _carVelocity = new Vector2(vel3d.z, vel3d.x);

        _speed = _carVelocity.magnitude;
                
        if(Mathf.Abs(Throttle) < 0.1 && Mathf.Abs(_speed) < 0.1f)
        {
            _rigidbody.velocity = Vector3.zero;
            _carVelocity = Vector2.zero;
            _speed = 0;
        }

        var rotAngle = 0.0f;
        var sideslip = 0.0f;
        if(Mathf.Abs(_speed) > 0.1f)
        {
            rotAngle = Mathf.Atan2(_rigidbody.angularVelocity.y, _carVelocity.x);
            sideslip = Mathf.Atan2(_carVelocity.y, _carVelocity.x);
        }

        var slipAngleFront = sideslip + rotAngle - SteerAngle;
        var slipAngleRear = sideslip - rotAngle;

        _totalWeight = _rigidbody.mass * Mathf.Abs(Physics.gravity.y);
        var weight = _totalWeight * 0.5f; // Weight per axle

        var weightDiff = _heightRatio * _rigidbody.mass * _carAcceleration.x; // --weight distribution between axles(stored to animate body)
        _weightFront = weight - weightDiff;
        _weightRear = weight + weightDiff;
        
        _percentFront = _weightFront / weight - 1.0f;
        var weightShiftAngle = Mathf.Clamp(_percentFront * 45, -20, 20);
        var euler = Chassis.localRotation.eulerAngles;
        euler.x = weightShiftAngle;
        Chassis.localRotation = Quaternion.Euler(euler);

        var fLateralFront = new Vector2(0, Mathf.Clamp(CorneringStiffnessFront * slipAngleFront, -MaxGrip, MaxGrip)) * weight;
        var fLateralRear = new Vector2(0, Mathf.Clamp(CorneringStiffnessRear * slipAngleRear, -MaxGrip, MaxGrip)) * weight;
        if (EBrake)
            fLateralRear *= 0.5f;

        //_wheelAngularVelocity = _speed / WheelRadius;
        //_slipLongitudal = _speed == 0 ? 0.1f : (_wheelAngularVelocity * WheelRadius - _speed) / Mathf.Abs(_speed);

        _fTraction = Vector2.right * EngineForce * Throttle;
        //_fTractionMax = TyreFrictionConstant * _weightRear;

        if (_speed > 0 && Brake > 0)
        {
            _fTraction = -Vector2.right * BrakeConstant * Brake * Mathf.Sign(_carVelocity.x);
            //_fTractionMax = TyreFrictionConstant * _weightFront;
        }
        //_fTraction = Vector2.ClampMagnitude(_fTraction, _fTractionMax);

        var fDrag = -DragConstant * _carVelocity * Mathf.Abs(_carVelocity.x);
        var fRollingResistance = -RollingResistanceConstant * _carVelocity;
        var fLong = _fTraction + fDrag + fRollingResistance;

        var forces = fLong + fLateralFront + fLateralRear;

        var torque = _b * fLateralFront.y - _c * fLateralRear.y;
        var angularAcceleration = torque / _rigidbody.mass; // Really inertia but...
        _rigidbody.angularVelocity += Vector3.up * angularAcceleration * Time.deltaTime;
        
        _carAcceleration = Time.deltaTime * forces / _rigidbody.mass;
        
        var worldAcceleration = transform.TransformVector(new Vector3(_carAcceleration.y, 0, _carAcceleration.x));
        _rigidbody.velocity += worldAcceleration;
        
        //var l = _c + _b;
        //var cgHeight = _rigidbody.centerOfMass.y + 0.5f;

        //var acceleration = _fTraction / _rigidbody.mass;
        //_weightFront = ((_c / l) * _totalWeight) - ((cgHeight / l) * _rigidbody.mass * acceleration.x);
        //_weightRear = ((_b / l) * _totalWeight) + ((cgHeight / l) * _rigidbody.mass * acceleration.x);

    }

    private void OnGUI()
    {
        GUILayout.Label("Local velocity: " + _carVelocity + ", acceleration: " + _carAcceleration);
        GUILayout.Label("Speed: " + _speed);
        GUILayout.Label("Half weight: " + (_totalWeight*0.5f) + ", Weight front: " + _weightFront + ", rear: " + _weightRear + ", percent front: " + _percentFront*100 + "%");
        GUILayout.Label("Traction: " + _fTraction + ", max: " + _fTractionMax);
        GUILayout.Label("Wheel angvel: " + _wheelAngularVelocity+ ", slip longitudal: " + _slipLongitudal);
    }
}
