// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace GalaxyExplorer
{
    public interface IGazeSelectionTarget
    {
        void OnGazeSelect();

        void OnGazeDeselect();
    }

    public class GazeSelectionTarget : MonoBehaviour, IGazeSelectionTarget
    {
        public string[] VoiceCommands;

        public virtual void OnGazeSelect()
        {
        }

        public virtual void OnGazeDeselect()
        {
        }

        public virtual bool OnNavigationStarted(InteractionSourceKind source, Vector3 relativePosition, Ray headRay)
        {
            return false;
        }

        public virtual bool OnNavigationUpdated(InteractionSourceKind source, Vector3 relativePosition, Ray headRay)
        {
            return false;
        }

        public virtual bool OnNavigationCompleted(InteractionSourceKind source, Vector3 relativePosition, Ray headRay)
        {
            return false;
        }

        public virtual bool OnNavigationCanceled(InteractionSourceKind source)
        {
            return false;
        }

        public virtual bool OnTapped()
        {
            return false;
        }

        public void RegisterVoiceCommands()
        {
            foreach (string command in VoiceCommands)
            {
                PlayerInputManager.Instance.AddSpeechCallback(command, VoiceCommandCallback);
            }
        }

        protected void UnregisterVoiceCommands()
        {
            foreach (string command in VoiceCommands)
            {
                PlayerInputManager.Instance.RemoveSpeechCallback(command, VoiceCommandCallback);
            }
        }

        protected virtual void VoiceCommandCallback(string command)
        {
        }
    }
}