using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private bool _isReadyToBreak = false;


        PlayerInputSystem _input;

        private List<Rigidbody> _brakeOff = new List<Rigidbody>();
        private bool _ActionHeldDown;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            
            if (_isReadyToBreak == false && _brakeOff.Count > 0)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
            }

            if (_isReadyToBreak && zone.GetZoneID() == 6) //Crate zone            
            {
                if (_brakeOff.Count > 0)
                {
                    _input = new PlayerInputSystem();
                    _input.Player.Enable();
                    _input.Player.Interaction.started += InteractionPunch_Started;
                    _input.Player.Interaction.canceled += InteractionPunch_Canceled;
                    BreakPart();
                    StartCoroutine(PunchDelay());
                    StartCoroutine(BreakMultiple());
                }
                else if(_brakeOff.Count <= 0)
                {
                    _isReadyToBreak = false;
                    _crateCollider.enabled = false;
                    _interactableZone.CompleteTask(6);
                    _input.Player.Interaction.started -= InteractionPunch_Started;
                    _input.Player.Interaction.canceled -= InteractionPunch_Canceled;
                    StopAllCoroutines();;
                    Debug.Log("Completely Busted");
                }
            }
        }

        private void InteractionPunch_Started(InputAction.CallbackContext context)
        {
            _ActionHeldDown = true;
        }
        private void InteractionPunch_Canceled(InputAction.CallbackContext context)
        {
            _ActionHeldDown = false;
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
        }
        
        public void BreakPart()
        {
            int rng = Random.Range(0, _brakeOff.Count);
            _brakeOff[rng].constraints = RigidbodyConstraints.None;
            _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
            _brakeOff.Remove(_brakeOff[rng]);            
        }

        IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(6);
        }


        IEnumerator BreakMultiple()
        {
            while(_ActionHeldDown)
            {
                int rng = Random.Range(0, _brakeOff.Count);
                _brakeOff[rng].constraints = RigidbodyConstraints.None;
                _brakeOff[rng].AddForce(new Vector3(1f, 1f, 1f), ForceMode.Force);
                _brakeOff.Remove(_brakeOff[rng]);
                yield return new WaitForSeconds(1f);
            }
                
            yield return null;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
        }
    }
}
