using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        PlayerInputSystem _input;

        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }

        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;


        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;
        bool _thrustUpwards;
        bool _thrustDownwards;

        private void OnEnable()
        {
            _input = new PlayerInputSystem();
            _input.Player.Disable();
            _input.Drone.Enable();
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void Start()
        {
            _input.Drone.ThrustUpwards.performed += ThrustUpwards_Performed;
            _input.Drone.ThrustDownwards.performed += ThrustDownwards_Performed;
            _input.Drone.ThrustUpwards.canceled += ThrustUpwards_Canceled;
            _input.Drone.ThrustDownwards.canceled += ThrustDownwards_Canceled;
        }

        private void ThrustDownwards_Canceled(InputAction.CallbackContext obj)
        {
            _thrustDownwards = false;
        }

        private void ThrustUpwards_Canceled(InputAction.CallbackContext obj)
        {
            _thrustUpwards = false;
        }

        private void ThrustDownwards_Performed(InputAction.CallbackContext context)
        {
            _thrustDownwards = true;
        }

        private void ThrustUpwards_Performed(InputAction.CallbackContext context)
        {
            _thrustUpwards = true;
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);
            _input.Player.Enable();
            _input.Drone.Disable();
        }

        private void Update()
        {
            if (_inFlightMode)
            {
                CalculateTilt();
                CalculateMovementUpdate();

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    _inFlightMode = false;
                    onExitFlightmode?.Invoke();
                    ExitFlightMode();
                }
            }
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);

            if (_inFlightMode)
               CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate()
        {
            var move = _input.Drone.Movement.ReadValue<Vector2>();
            if (move.x < 0)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y -= _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            if (move.x > 0)
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
        }

        private void CalculateMovementFixedUpdate() 
        {
            if (_thrustUpwards)
            {
                _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
            }
            if (_thrustDownwards)
            {
                _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
            }
        } 

        private void CalculateTilt()
        {
            var move = _input.Drone.Movement.ReadValue<Vector2>();

            if (move.y > 0)
                transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            else if (move.y < 0)
                transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
            else
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
        }
    }
}
