// Copyright Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace GalaxyExplorer
{
    public delegate void SpeechCallback(string keyword);

    public interface ISpeechInputManager
    {
        bool RegisterSpeechKeyword(string keyword);

        bool AddSpeechCallback(string keyword, SpeechCallback callback);

        bool RemoveSpeechCallback(string keyword, SpeechCallback callback);
    }
}