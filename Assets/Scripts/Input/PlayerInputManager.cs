// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace GalaxyExplorer
{
    public sealed class PlayerInputManager : SingleInstance<PlayerInputManager>, ISpeechInputManager
    {
        public event Action TapPressAction;

        public event Action TapReleaseAction;

        private Dictionary<string, List<SpeechCallback>> speechCallbacks;
        private KeywordRecognizer keywordRecognizer = null;

        private void Awake()
        {
            speechCallbacks = new Dictionary<string, List<SpeechCallback>>();
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            if (speechCallbacks.Keys.Count > 0)
            {
                string[] keywords = new string[speechCallbacks.Keys.Count];
                speechCallbacks.Keys.CopyTo(keywords, 0);
                try
                {
                    keywordRecognizer = new KeywordRecognizer(keywords);
                    keywordRecognizer.OnPhraseRecognized += SpeechCallbackHandler;
                    keywordRecognizer.Start();
                }
                catch
                {
                    Debug.Log("Failed to start the keywordRecognizer.");
                    if (keywordRecognizer != null)
                    {
                        keywordRecognizer.OnPhraseRecognized -= SpeechCallbackHandler;
                    }
                    keywordRecognizer = null;
                }
            }
        }

        private void OnDispose()
        {
            if (keywordRecognizer != null)
            {
                keywordRecognizer.Stop();
                keywordRecognizer.OnPhraseRecognized -= SpeechCallbackHandler;
                keywordRecognizer.Dispose();
            }
        }

        private void SpeechCallbackHandler(PhraseRecognizedEventArgs eventArgs)
        {
            TriggerSpeechCommand(eventArgs.text);
        }

        public void TriggerTapPress()
        {
            if (TapPressAction != null)
            {
                TapPressAction();
            }
        }

        public void TriggerTapRelease()
        {
            if (TapReleaseAction != null)
            {
                TapReleaseAction();
            }
        }

        // This must be called from Start(). Use this to register a speech keyword, so callbacks can be added and removed to it later.
        public bool RegisterSpeechKeyword(string keyword)
        {
            if (keywordRecognizer != null)
            {
                Debug.LogError("Attempting to add a speech keyword, " + keyword + ", but the keyword has not been registered in a Start() callback");
                return false;
            }

            if (speechCallbacks.ContainsKey(keyword))
            {
                return true;
            }

            speechCallbacks.Add(keyword, new List<SpeechCallback>());
            return true;
        }

        // If this is not called from Start(), call RegisterSpeechKeyword() from Start() to register the keyword for use before the callback is added; otherwise,
        // calling this function from start will register the keyword and add the callback.
        public bool AddSpeechCallback(string keyword, SpeechCallback callback)
        {
            if (RegisterSpeechKeyword(keyword))
            {
                // Check to see if the callback has already been added.
                for (int i = 0; i < speechCallbacks[keyword].Count; i++)
                {
                    if (speechCallbacks[keyword][i] == callback)
                    {
                        Debug.LogWarning("Attempting to add same callback for keyword");
                        return false;
                    }
                }

                speechCallbacks[keyword].Add(callback);
                return true;
            }

            return false;
        }

        public bool RemoveSpeechCallback(string keyword, SpeechCallback callback)
        {
            if (!speechCallbacks.ContainsKey(keyword))
            {
                Debug.LogWarning("Attempting to remove callback for unregistered keyword, " + keyword + "!");
                return false;
            }

            for (int i = 0; i < speechCallbacks[keyword].Count; i++)
            {
                if (speechCallbacks[keyword][i] == callback)
                {
                    speechCallbacks[keyword].RemoveAt(i);
                    return true;
                }
            }

            // callback not found for the keyword
            return false;
        }

        public void TriggerSpeechCommand(string keyword)
        {
            if (speechCallbacks.ContainsKey(keyword))
            {
                for (int i = 0; i < speechCallbacks[keyword].Count; i++)
                {
                    if (speechCallbacks[keyword][i] != null)
                    {
                        speechCallbacks[keyword][i](keyword);
                    }
                    else
                    {
                        speechCallbacks[keyword].RemoveAt(i--);
                    }
                }
            }
        }
    }
}