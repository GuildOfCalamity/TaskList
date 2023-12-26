using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Task_List_App;

/// <summary>
/// Helpers for working with tasks.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Gets the result of a <see cref="Task"/> if available, or <see langword="null"/> otherwise.
    /// </summary>
    /// <param name="task">The input <see cref="Task"/> instance to get the result for.</param>
    /// <returns>The result of <paramref name="task"/> if completed successfully, or <see langword="default"/> otherwise.</returns>
    /// <remarks>
    /// This method does not block if <paramref name="task"/> has not completed yet. Furthermore, it is not generic
    /// and uses reflection to access the <see cref="Task{TResult}.Result"/> property and boxes the result if it's
    /// a value type, which adds overhead. It should only be used when using generics is not possible.
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static object? GetResultOrDefault(this Task task)
    {
        // Check if the instance is a completed Task
        if (
#if NETSTANDARD2_1
            task.IsCompletedSuccessfully
#else
            task.Status == TaskStatus.RanToCompletion
#endif
        )
        {
            // We need an explicit check to ensure the input task is not the cached
            // Task.CompletedTask instance, because that can internally be stored as
            // a Task<T> for some given T (eg. on .NET 5 it's VoidTaskResult), which
            // would cause the following code to return that result instead of null.
            if (task != Task.CompletedTask)
            {
                // Try to get the Task<T>.Result property. This method would've
                // been called anyway after the type checks, but using that to
                // validate the input type saves some additional reflection calls.
                // Furthermore, doing this also makes the method flexible enough to
                // cases whether the input Task<T> is actually an instance of some
                // runtime-specific type that inherits from Task<T>.
                PropertyInfo? propertyInfo =
#if NETSTANDARD1_4
                    task.GetType().GetRuntimeProperty(nameof(Task<object>.Result));
#else
                    task.GetType().GetProperty(nameof(Task<object>.Result));
#endif

                // Return the result, if possible
                return propertyInfo?.GetValue(task);
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the result of a <see cref="Task{TResult}"/> if available, or <see langword="default"/> otherwise.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Task{TResult}"/> to get the result for.</typeparam>
    /// <param name="task">The input <see cref="Task{TResult}"/> instance to get the result for.</param>
    /// <returns>The result of <paramref name="task"/> if completed successfully, or <see langword="default"/> otherwise.</returns>
    /// <remarks>This method does not block if <paramref name="task"/> has not completed yet.</remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetResultOrDefault<T>(this Task<T?> task)
    {
#if NETSTANDARD2_1
        return task.IsCompletedSuccessfully ? task.Result : default;
#else
        return task.Status == TaskStatus.RanToCompletion ? task.Result : default;
#endif
    }
}

#region Windows TaskBar Progress
/// <summary>
/// Helper class to set taskbar progress on Windows 7+ systems.
/// </summary>
public static class TaskbarProgress
{
    /// <summary>
    /// Available taskbar progress states
    /// </summary>
    public enum TaskbarStates
    {
        /// <summary>No progress displayed</summary>
        NoProgress = 0,
        /// <summary>Indeterminate</summary>
        Indeterminate = 0x1,
        /// <summary>Normal</summary>
        Normal = 0x2,
        /// <summary>Error</summary>
        Error = 0x4,
        /// <summary>Paused</summary>
        Paused = 0x8
    }

    [ComImportAttribute()]
    [GuidAttribute("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ITaskbarList3
    {
        // ITaskbarList
        [PreserveSig]
        void HrInit();
        [PreserveSig]
        void AddTab(IntPtr hwnd);
        [PreserveSig]
        void DeleteTab(IntPtr hwnd);
        [PreserveSig]
        void ActivateTab(IntPtr hwnd);
        [PreserveSig]
        void SetActiveAlt(IntPtr hwnd);

        // ITaskbarList2
        [PreserveSig]
        void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

        // ITaskbarList3
        [PreserveSig]
        void SetProgressValue(IntPtr hwnd, UInt64 ullCompleted, UInt64 ullTotal);
        [PreserveSig]
        void SetProgressState(IntPtr hwnd, TaskbarStates state);
    }

    [GuidAttribute("56FDF344-FD6D-11d0-958A-006097C9A090")]
    [ClassInterfaceAttribute(ClassInterfaceType.None)]
    [ComImportAttribute()]
    private class TaskbarInstance
    {
    }

    private static readonly bool taskbarSupported = IsWindows7OrLater;
    private static readonly ITaskbarList3? taskbarInstance = taskbarSupported ? (ITaskbarList3)new TaskbarInstance() : null;

    /// <summary>
    /// Sets the state of the taskbar progress.
    /// </summary>
    /// <param name="windowHandle">current form handle</param>
    /// <param name="taskbarState">desired state</param>
    public static void SetState(IntPtr windowHandle, TaskbarStates taskbarState)
    {
        if (taskbarSupported)
        {
            taskbarInstance?.SetProgressState(windowHandle, taskbarState);
        }
    }

    /// <summary>
    /// Sets the value of the taskbar progress.
    /// </summary>
    /// <param name="windowHandle">current form handle</param>
    /// <param name="progressValue">desired progress value</param>
    /// <param name="progressMax">maximum progress value</param>
    public static void SetValue(IntPtr windowHandle, double progressValue, double progressMax)
    {
        if (taskbarSupported)
        {
            taskbarInstance?.SetProgressValue(windowHandle, (ulong)progressValue, (ulong)progressMax);
        }
    }

    /// <summary>
    /// Determines if current operating system is Windows 7+.
    /// </summary>
    public static bool IsWindows7OrLater => Environment.OSVersion.Version >= new Version(6, 1);

    /// <summary>
    /// Determines if current operating system is Vista+.
    /// </summary>
    public static bool IsWindowsVistaOrLater => Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(6, 0, 6000);
}
#endregion
