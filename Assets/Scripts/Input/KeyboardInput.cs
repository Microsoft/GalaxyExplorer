// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace GalaxyExplorer
{
    public class KeyboardInput : SingleInstance<KeyboardInput>
    {
        // Different events for keys
        public enum KeyEvent
        {
            KeyPressed = 0, // When a key is pressed down
            KeyDown, // When a key is held down
            KeyReleased // When a key is released
        }

        // A structure that pairs a keycode with an event of either pressed, unpressed,
        // or down. 
        public struct KeyCodeEventPair
        {
            public KeyCode keyCode;
            public KeyEvent keyEvent;

            public KeyCodeEventPair(KeyCode inCode, KeyEvent inEvent)
            {
                keyCode = inCode;
                keyEvent = inEvent;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is KeyCodeEventPair))
                {
                    return false;
                }

                KeyCodeEventPair compare = (KeyCodeEventPair)obj;

                return keyCode == compare.keyCode && keyEvent == compare.keyEvent;
            }

            public override int GetHashCode()
            {
                int hashCode;

                // Multiply our keycode hash to make it more unique
                hashCode = keyCode.GetHashCode() * 100;
                hashCode += keyEvent.GetHashCode();

                return hashCode;
            }
        }

        // Callback delegate for when a specified key is pressed
        public delegate void InputKeyCallback(KeyCodeEventPair keyCodeEvent);

        // The list of the different callbacks based on the keycode event
        private Dictionary<KeyCodeEventPair, List<InputKeyCallback>> keyCallbacks = new Dictionary<KeyCodeEventPair, List<InputKeyCallback>>();

        // A queue of the different keycode/events that were actioned this frame
        private Queue<KeyCodeEventPair> frameEvents = new Queue<KeyCodeEventPair>();

        private void Update()
        {
            // Check for all keys that are registered for events
            foreach (KeyCodeEventPair keyCheck in keyCallbacks.Keys)
            {
                bool eventTriggered = false;

                switch (keyCheck.keyEvent)
                {
                    case KeyEvent.KeyDown:
                        eventTriggered = Input.GetKey(keyCheck.keyCode);
                        break;
                    case KeyEvent.KeyPressed:
                        eventTriggered = Input.GetKeyDown(keyCheck.keyCode);
                        break;
                    case KeyEvent.KeyReleased:
                        eventTriggered = Input.GetKeyUp(keyCheck.keyCode);
                        break;
                }

                if (eventTriggered)
                {
                    frameEvents.Enqueue(keyCheck);
                }
            }

            while (frameEvents.Count > 0)
            {
                HandleKeyEvent(frameEvents.Dequeue());
            }
        }

        #region Public Functions
        public void RegisterKeyEvent(KeyCodeEventPair keycodeEvent, InputKeyCallback callback)
        {
            if (!keyCallbacks.ContainsKey(keycodeEvent))
            {
                keyCallbacks.Add(keycodeEvent, new List<InputKeyCallback>());
            }

            // Check to see if callback exists
            for (int i = 0; i < keyCallbacks[keycodeEvent].Count; i++)
            {
                if (keyCallbacks[keycodeEvent][i] == callback)
                {
                    // Duplicate
                    return;
                }
            }

            keyCallbacks[keycodeEvent].Add(callback);
        }

        public void UnregisterKeyEvent(KeyCodeEventPair keycodeEvent, InputKeyCallback callback)
        {
            if (keyCallbacks.ContainsKey(keycodeEvent))
            {
                for (int i = 0; i < keyCallbacks[keycodeEvent].Count; i++)
                {
                    if (keyCallbacks[keycodeEvent][i] == callback)
                    {
                        keyCallbacks[keycodeEvent].RemoveAt(i);
                        break;
                    }
                }

                // If no more callbacks, remove this entry in the list
                if (keyCallbacks[keycodeEvent].Count == 0)
                {
                    keyCallbacks.Remove(keycodeEvent);
                }
            }
        }

        public void ProcessProxyKeyboardEvent(KeyCode keyCode, KeyEvent keyEvent)
        {
            HandleKeyEvent(new KeyCodeEventPair(keyCode, keyEvent));
        }
        #endregion

        private void HandleKeyEvent(KeyCodeEventPair keyEventPair)
        {
            // Safety check
            if (keyCallbacks.ContainsKey(keyEventPair))
            {
                // Create a copy of the list in case it gets changed in the callback by a 
                // listener (unregistering).
                List<InputKeyCallback> listCopy = new List<InputKeyCallback>(keyCallbacks[keyEventPair]);
                foreach (InputKeyCallback callback in listCopy)
                {
                    callback(keyEventPair);
                }
            }
        }
    }
}