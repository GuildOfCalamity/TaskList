using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Task_List_App.Helpers;

/// <summary>
/// Defines our <see cref="ObjectEventArgs"/>-based event bus class.
/// </summary>
public class EventBus : IDisposable
{
    bool disposed = false;
    Dictionary<string, EventHandler<ObjectEventArgs>?> eventHandlers = new();

    /// <summary>
    /// Adds an event to the handler pool.
    /// </summary>
    public void Subscribe(string eventName, EventHandler<ObjectEventArgs>? handler)
    {
        if (string.IsNullOrEmpty(eventName) || handler == null)
        {
            Debug.WriteLine($"[WARNING]: \"{nameof(eventName)}\" or \"{nameof(handler)}\" is null.");
            return;
        }

        if (!eventHandlers.ContainsKey(eventName))
        {   // Create the key.
            eventHandlers[eventName] = null;
        }

        // Add the new handler for the key.
        eventHandlers[eventName] += handler;
    }

    /// <summary>
    /// Removes an event from the handler pool.
    /// </summary>
    public void Unsubscribe(string eventName, EventHandler<ObjectEventArgs>? handler)
    {
        if (!string.IsNullOrEmpty(eventName) && eventHandlers.ContainsKey(eventName) && handler != null)
        {   // Remove the existing handler.
            eventHandlers[eventName] -= handler;
        }
    }

    /// <summary>
    /// Calls <see cref="Unsubscribe(string, EventHandler{ObjectEventArgs})"/> and then <see cref="Subscribe(string, EventHandler{ObjectEventArgs})"/>.
    /// </summary>
    public void Resubscribe(string eventName, EventHandler<ObjectEventArgs>? handler)
    {
        if (string.IsNullOrEmpty(eventName) || handler == null)
        {
            Debug.WriteLine($"[WARNING]: \"{nameof(eventName)}\" or \"{nameof(handler)}\" is null.");
            return;
        }
        Unsubscribe(eventName, handler);
        Subscribe(eventName, handler);
    }

    /// <summary>
    /// Causes an event to be invoked through the handler pool.
    /// </summary>
    public void Publish(string eventName, object? message)
    {
        if (!string.IsNullOrEmpty(eventName) && eventHandlers.ContainsKey(eventName))
        {
            EventHandler<ObjectEventArgs>? handlers = eventHandlers[eventName];
            handlers?.Invoke(this, new ObjectEventArgs(message));
        }
        else
        {
            Debug.WriteLine($"[WARNING]: {nameof(eventName)} key \"{eventName}\" was not found. If you wish to use this you must first Subscribe it.");
        }
    }

    /// <summary>
    /// Reports if an event already resides in the handler pool.
    /// </summary>
    public bool IsSubscribed(string eventName)
    {
        return eventHandlers.ContainsKey(eventName);
    }

    #region [IDispose]
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
                eventHandlers.Clear(); // Cleanup managed resources.

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer for safety (if the Dispose method isn't explicitly called)
    /// </summary>
    ~EventBus() => Dispose();
    #endregion
}

#region [EventArg Models]
/// <summary>
/// Define our event args class.
/// This example uses an object value that could be switched
/// upon in the main UI update routine, but more complex object
/// types could be passed to encapsulate additional information.
/// </summary>
public class ObjectEventArgs : EventArgs
{
    public object? Payload { get; }

    public ObjectEventArgs(object? payload)
    {
        Payload = payload;
    }
}
#endregion
