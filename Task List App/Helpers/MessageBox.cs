using System.Runtime.InteropServices;

namespace Task_List_App;

public static class MessageBox
{
    #region [MessageBox]
    // ANSI version (current codepage will be used)
    [DllImport("User32.dll", EntryPoint = "MessageBox", CharSet = CharSet.Auto)]
    public static extern int Show(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    // Unicode version (UTF-8)
    [DllImport("User32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
    public static extern int ShowUTF(IntPtr hWnd, string lpText, string lpCaption, uint uType);

    // https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-messagebox

    // Button options
    public const uint MB_OK = 0x00000000;
    public const uint MB_OKCANCEL = 0x00000001;
    public const uint MB_ABORTRETRYIGNORE = 0x00000002;
    public const uint MB_YESNOCANCEL = 0x00000003;
    public const uint MB_RETRYCANCEL = 0x00000005;
    public const uint MB_CANCELTRYCONTINUE = 0x00000006;
    public const uint MB_YESNO = 0x00000004;
    public const uint MB_HELP = 0x00004000;
    // Icons
    public const uint MB_ICONERROR = 0x00000010;
    public const uint MB_ICONQUESTION = 0x00000020;
    public const uint MB_ICONWARNING = 0x00000030;
    public const uint MB_ICONINFORMATION = 0x00000040;
    // Dialog Results
    public const uint MB_IDOK = 1;        // The OK button was selected.
    public const uint MB_IDCANCEL = 2;    // The Cancel button was selected.
    public const uint MB_IDABORT = 3;     // The Abort button was selected.
    public const uint MB_IDRETRY = 4;     // The Retry button was selected.
    public const uint MB_IDIGNORE = 5;    // The Ignore button was selected.
    public const uint MB_IDYES = 6;       // The Yes button was selected.
    public const uint MB_IDNO = 7;        // The No button was selected.
    public const uint MB_IDTRYAGAIN = 10; // The Try Again button was selected.
    public const uint MB_IDCONTINUE = 11; // The Continue button was selected.
    // Button Defaults
    public const uint MB_DEFBUTTON1 = 0x00000000; // The first button is the default button. 
    public const uint MB_DEFBUTTON2 = 0x00000100; // The second button is the default button.
    public const uint MB_DEFBUTTON3 = 0x00000200; // The third button is the default button.
    public const uint MB_DEFBUTTON4 = 0x00000300; // The fourth button is the default button.
    // Modals
    public const uint MB_APPLMODAL = 0x00000000;   // The user must respond to the message box before continuing work in the window identified by the hWnd parameter. However, the user can move to the windows of other threads and work in those windows. 
    public const uint MB_SYSTEMMODAL = 0x00001000; // Same as MB_APPLMODAL except that the message box has the WS_EX_TOPMOST style. This flag has no effect on the user's ability to interact with windows other than those associated with hWnd. If hWnd is supplied, then clicking on the message dialog will also bring the hWnd into focus.
    public const uint MB_TASKMODAL = 0x00002000;   // Same as MB_APPLMODAL except that all the top-level windows belonging to the current thread are disabled if the hWnd parameter is NULL.
    // Miscellaneous
    public const uint MB_RIGHT = 0x00080000; // The text is right-justified.
    public const uint MB_TOPMOST = 0x00040000; // The message box is created with the WS_EX_TOPMOST window style.
    public const uint MB_SETFOREGROUND = 0x00010000; // The message box becomes the foreground window.Internally, the system calls the SetForegroundWindow function for the message box.
    public const uint MB_DEFAULT_DESKTOP_ONLY = 0x00020000; // Same as desktop of the interactive window station.For more information, see Window Stations. If the current input desktop is not the default desktop, MessageBox does not return until the user switches to the default desktop.
    public const uint MB_SERVICE_NOTIFICATION = 0x00200000; // If this flag is set, the hWnd parameter must be NULL. This is so that the message box can appear on a desktop other than the desktop corresponding to the hWnd. The caller is a service notifying the user of an event. The function displays a message box on the current active desktop, even if there is no user logged on to the computer.
    #endregion
}
