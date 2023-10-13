using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using System;

namespace ClocknestGames.Game.Core
{
    public class CinemachineBlendFinishedNotifier : MonoBehaviour
    {
        CinemachineVirtualCameraBase vcamBase;

        [Serializable] public class BlendFinishedEvent : UnityEvent<CinemachineVirtualCameraBase> { }
        public BlendFinishedEvent OnBlendFinished;

        void Start()
        {
            vcamBase = GetComponent<CinemachineVirtualCameraBase>();
            ConnectToVcam(true);
            enabled = false;
        }

        void ConnectToVcam(bool connect)
        {
            var vcam = vcamBase as CinemachineVirtualCamera;
            if (vcam != null)
            {
                vcam.m_Transitions.m_OnCameraLive.RemoveListener(OnCameraLive);
                if (connect)
                    vcam.m_Transitions.m_OnCameraLive.AddListener(OnCameraLive);
            }
            var freeLook = vcamBase as CinemachineFreeLook;
            if (freeLook != null)
            {
                freeLook.m_Transitions.m_OnCameraLive.RemoveListener(OnCameraLive);
                if (connect)
                    freeLook.m_Transitions.m_OnCameraLive.AddListener(OnCameraLive);
            }
        }

        void OnCameraLive(ICinemachineCamera vcamIn, ICinemachineCamera vcamOut)
        {
            enabled = true;
        }

        void Update()
        {
            var brain = CinemachineCore.Instance.FindPotentialTargetBrain(vcamBase);
            if (brain == null)
                enabled = false;
            else if (!brain.IsBlending)
            {
                if (brain.IsLive(vcamBase))
                    OnBlendFinished.Invoke(vcamBase);
                enabled = false;
            }
        }
    }
}