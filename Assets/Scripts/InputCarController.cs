﻿using Assets.Car;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
    class InputCarController : MonoBehaviour
    {
        private NewCar _car;

        void Start()
        {
            _car = GetComponent<NewCar>();
        }

        void Update()
        {
            var throttle = Input.GetAxis("Vertical");
            var brake = -Mathf.Min(0, throttle);
            throttle = Mathf.Max(0, throttle);

            _car.Throttle = throttle;
            _car.Brake = brake;

            var steering = Input.GetAxis("Horizontal");
            _car.Steer = steering;

            _car.EBrake = Input.GetButton("E-brake");
            // _car.RearSlip = ebrake;

        }
    }

}