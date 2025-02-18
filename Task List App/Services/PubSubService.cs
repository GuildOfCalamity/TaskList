using System;
using System.Collections.Concurrent;

namespace Task_List_App.Services;

/// <summary>
/// Similar to the <see cref="EventBus"/> model.
/// </summary>
public class PubSubBasic<T>
{
    readonly ConcurrentQueue<T>? _messageQueue;
    static readonly Lazy<PubSubBasic<T>> _instance = new(() => new PubSubBasic<T>());

    /// <summary>
    /// Constructor will only be called when the Instance property is accessed.
    /// </summary>
    public static PubSubBasic<T> Instance => _instance.Value;

    /// <summary>
    /// Event triggered when a new message is received
    /// </summary>
    public event Action<T>? MessageReceived;

    /// <summary>
    /// Private constructor to prevent external instantiation.
    /// </summary>
    private PubSubBasic()
    {
        _messageQueue = new ConcurrentQueue<T>();
    }

    /// <summary>
    /// Sends a message to all subscribed consumers.
    /// </summary>
    public void SendMessage(T message)
    {
        if (message is null) { return; }
        _messageQueue?.Enqueue(message);
        MessageReceived?.Invoke(message);
    }

    /// <summary>
    /// Subscribes a consumer to listen for messages.
    /// </summary>
    public void Subscribe(Action<T> listener)
    {
        MessageReceived += listener;
    }

    /// <summary>
    /// Unsubscribes a consumer from message notifications.
    /// </summary>
    public void Unsubscribe(Action<T> listener)
    {
        MessageReceived -= listener;
    }
}

/// <summary>
///   The disposable version of the <see cref="PubSubBasic{T}"/> class.
///   This version includes a cleanup method to remove all event handlers 
///   when disposing, and checks for over-subscription scenarios.
/// </summary>
/// <remarks>
///   Since PubSubEnhanced<T> uses Lazy<T> it's a singleton and stays 
///   alive for the application’s lifetime. In most cases, you don’t need to 
///   implement IDisposable because Lazy<T> ensures a single instance that 
///   never gets GC'd until the application closes.
/// </remarks>
public class PubSubEnhanced<T> : IDisposable
{
    bool _disposed = false;
    readonly ConcurrentQueue<T>? _messageQueue;
    readonly ConcurrentDictionary<Action<T>, bool> _subscribers = new(); // Track subscriptions
    static readonly Lazy<PubSubEnhanced<T>> _instance = new(() => new PubSubEnhanced<T>());
    public event Action<T>? MessageReceived;

    /// <summary>
    /// Constructor will only be called when the Instance property is accessed.
    /// </summary>
    public static PubSubEnhanced<T> Instance => _instance.Value;

    /// <summary>
    /// Private constructor to prevent external instantiation.
    /// </summary>
    private PubSubEnhanced()
    {
        _messageQueue = new ConcurrentQueue<T>();
    }

    /// <summary>
    /// Sends a message to all subscribed consumers.
    /// </summary>
    public void SendMessage(T message)
    {
        if (message is null) { return; }
        _messageQueue?.Enqueue(message);
        MessageReceived?.Invoke(message);
    }

    /// <summary>
    /// Subscribes a consumer to listen for messages.
    /// Duplicate subscriptions are prevented.
    /// </summary>
    public void Subscribe(Action<T> listener)
    {
        if (!_subscribers.ContainsKey(listener))
        {
            _subscribers[listener] = true;
            MessageReceived += listener;
        }
    }

    /// <summary>
    /// Unsubscribes a consumer from message notifications.
    /// </summary>
    public void Unsubscribe(Action<T> listener)
    {
        if (_subscribers.TryRemove(listener, out _))
        {
            MessageReceived -= listener;
        }
    }

    #region [IDispose]
    /// <summary>
    /// Cleanup: Unsubscribes all listeners when disposing.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) { return; }
        // Remove all event handlers
        foreach (var subscriber in _subscribers.Keys)
        {
            MessageReceived -= subscriber;
        }
        _subscribers.Clear();
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer for safety (if the Dispose method isn't explicitly called)
    /// </summary>
    ~PubSubEnhanced() => Dispose();
    #endregion
}

/// <summary>
///   The time-based version of the <see cref="PubSubEnhanced{T}"/> class.
///   This version includes a cleanup method to remove stale messages.
/// </summary>
/// <remarks>
///   Since PubSubWithEviction<T> uses Lazy<T> it's a singleton and stays 
///   alive for the application’s lifetime. In most cases, you don’t need to 
///   implement IDisposable because Lazy<T> ensures a single instance that 
///   never gets GC'd until the application closes.
/// </remarks>
public class PubSubWithEviction<T> : IDisposable
{
    bool _disposed = false;
    double _evictionMinutes = 1.0;
    readonly ConcurrentQueue<(T message, DateTime timestamp)> _messageQueue;
    readonly ConcurrentDictionary<Action<T>, bool> _subscribers = new();
    static readonly Lazy<PubSubWithEviction<T>> _instance = new(() => new PubSubWithEviction<T>());
    readonly TimeSpan _messageExpiry;
    readonly CancellationTokenSource _cleanupTokenSource;
    public event Action<T>? MessageReceived;

    /// <summary>
    /// Constructor will only be called when the Instance property is accessed.
    /// </summary>
    public static PubSubWithEviction<T> Instance => _instance.Value;

    /// <summary>
    /// Private constructor to prevent external instantiation.
    /// </summary>
    private PubSubWithEviction(TimeSpan messageExpiry = default)
    {
        _messageQueue = new ConcurrentQueue<(T, DateTime)>();
        _messageExpiry = messageExpiry == default ? TimeSpan.FromMinutes(_evictionMinutes) : messageExpiry;
        _cleanupTokenSource = new CancellationTokenSource();
        Task.Run(() => CheckForExpiredAsync(_messageExpiry.TotalMinutes, _cleanupTokenSource.Token));
    }

    /// <summary>
    /// Sends a message to all subscribed consumers.
    /// </summary>
    public void SendMessage(T message)
    {
        if (message is null) return;
        _messageQueue.Enqueue((message, DateTime.UtcNow));
        MessageReceived?.Invoke(message);
    }

    /// <summary>
    /// Subscribes a consumer to listen for messages.
    /// Duplicate subscriptions are prevented.
    /// </summary>
    public void Subscribe(Action<T> listener)
    {
        if (!_subscribers.ContainsKey(listener))
        {
            _subscribers[listener] = true;
            MessageReceived += listener;
            // Send non-expired messages to new subscriber
            foreach (var (message, timestamp) in _messageQueue)
            {
                if (DateTime.UtcNow - timestamp < _messageExpiry)
                {
                    listener(message);
                }
            }
        }
    }

    /// <summary>
    /// Unsubscribes a consumer from message notifications.
    /// </summary>
    public void Unsubscribe(Action<T> listener)
    {
        if (_subscribers.TryRemove(listener, out _))
        {
            MessageReceived -= listener;
        }
    }

    /// <summary>
    /// Periodically removes expired messages.
    /// </summary>
    async Task CheckForExpiredAsync(double minutes, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(minutes), token);
            var cutoff = DateTime.UtcNow - _messageExpiry;
            while (_messageQueue.TryPeek(out var oldest) && oldest.timestamp < cutoff)
            {
                _messageQueue.TryDequeue(out _);
            }
        }
    }

    #region [IDispose]
    /// <summary>
    /// Cleanup: Unsubscribes all listeners when disposing.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        _cleanupTokenSource.Cancel();
        _cleanupTokenSource.Dispose();
        foreach (var subscriber in _subscribers.Keys)
        {
            MessageReceived -= subscriber;
        }
        _subscribers.Clear();
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer for safety (if the Dispose method isn't explicitly called)
    /// </summary>
    ~PubSubWithEviction() => Dispose();
    #endregion
}

#region [Supporting Cast]
public class ApplicationMessage
{
    public ModuleId Module { get; set; }
    public string? MessageText { get; set; }
    public Type? MessageType { get; set; }
    public object? MessagePayload { get; set; }
    public DateTime MessageTime { get; set; } = DateTime.Now;
}

public enum ModuleId
{
    App = 0,
    MainWindow = 1,
    TasksPage = 2,
    LoginPage = 3,
    NotesPage = 4,
    SettingsPage = 5,
    ControlsPage = 6,
}
#endregion