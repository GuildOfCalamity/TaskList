﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_List_App.Helpers;

/// <summary>
/// Implements a weak event listener that allows the owner to be garbage
/// collected if its only remaining link is an event handler.
/// </summary>
/// <typeparam name="TInstance">Type of instance listening for the event.</typeparam>
/// <typeparam name="TSource">Type of source for the event.</typeparam>
/// <typeparam name="TEventArgs">Type of event arguments for the event.</typeparam>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class WeakEventListener<TInstance, TSource, TEventArgs> where TInstance : class
{
    /// <summary>
    /// WeakReference to the instance listening for the event.
    /// </summary>
    private readonly WeakReference _weakInstance;

    /// <summary>
    /// Gets or sets the method to call when the event fires.
    /// </summary>
    public Action<TInstance, TSource, TEventArgs>? OnEventAction { get; set; }

    /// <summary>
    /// Gets or sets the method to call when detaching from the event.
    /// </summary>
    public Action<WeakEventListener<TInstance, TSource, TEventArgs>>? OnDetachAction { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakEventListener{TInstance, TSource, TEventArgs}"/> class.
    /// </summary>
    /// <param name="instance">Instance subscribing to the event.</param>
    public WeakEventListener(TInstance instance)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        _weakInstance = new WeakReference(instance);
    }

    /// <summary>
    /// Handler for the subscribed event calls OnEventAction to handle it.
    /// </summary>
    /// <param name="source">Event source.</param>
    /// <param name="eventArgs">Event arguments.</param>
    public void OnEvent(TSource source, TEventArgs eventArgs)
    {
        var target = (TInstance)_weakInstance.Target;
        if (target != null)
        {   // Call registered action
            OnEventAction?.Invoke(target, source, eventArgs);
        }
        else
        {   // Detach from event
            Detach();
        }
    }

    /// <summary>
    /// Detaches from the subscribed event.
    /// </summary>
    public void Detach()
    {
        OnDetachAction?.Invoke(this);
        OnDetachAction = null;
    }
}
