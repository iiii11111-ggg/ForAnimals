// InteractionEvents.cs
using System;

public static class InteractionEvents
{
    public static event Action OnShowPromptRequest;
    public static event Action OnHidePromptRequest;

    public static void RequestShowPrompt()
    {
        OnShowPromptRequest?.Invoke();
    }

    public static void RequestHidePrompt()
    {
        OnHidePromptRequest?.Invoke();
    }
}