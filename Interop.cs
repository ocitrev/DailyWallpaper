using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;

    public RECT(int left, int top, int right, int bottom)
    {
        this.left = left;
        this.top = top;
        this.right = right;
        this.bottom = bottom;
    }

    public int Width
    {
        get { return right - left; }
    }

    public int Height
    {
        get { return bottom - top; }
    }
}

[ComImport]
[Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IDesktopWallpaper
{
    HRESULT SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string? monitorID, [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);
    HRESULT GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string? monitorID, [MarshalAs(UnmanagedType.LPWStr)] out string wallpaper);
    HRESULT GetMonitorDevicePathAt(uint monitorIndex, [MarshalAs(UnmanagedType.LPWStr)] ref string monitorID);
    HRESULT GetMonitorDevicePathCount(ref uint count);
    HRESULT GetMonitorRECT([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.Struct)] ref RECT displayRect);
    HRESULT SetBackgroundColor(uint color);
    HRESULT GetBackgroundColor(ref uint color);
    HRESULT SetPosition(DESKTOP_WALLPAPER_POSITION position);
    HRESULT GetPosition(ref DESKTOP_WALLPAPER_POSITION position);
    HRESULT SetSlideshow(IShellItemArray items);
    HRESULT GetSlideshow(ref IShellItemArray items);
    HRESULT SetSlideshowOptions(DESKTOP_SLIDESHOW_OPTIONS options, uint slideshowTick);
    [PreserveSig]
    HRESULT GetSlideshowOptions(out DESKTOP_SLIDESHOW_OPTIONS options, out uint slideshowTick);
    HRESULT AdvanceSlideshow([MarshalAs(UnmanagedType.LPWStr)] string monitorID, DESKTOP_SLIDESHOW_DIRECTION direction);
    HRESULT GetStatus(ref DESKTOP_SLIDESHOW_STATE state);
    HRESULT Enable(bool benable);
}


public enum DESKTOP_WALLPAPER_POSITION
{
    DWPOS_CENTER = 0,
    DWPOS_TILE = 1,
    DWPOS_STRETCH = 2,
    DWPOS_FIT = 3,
    DWPOS_FILL = 4,
    DWPOS_SPAN = 5
}

public enum DESKTOP_SLIDESHOW_OPTIONS
{
    DSO_SHUFFLEIMAGES = 0x1
}

public enum DESKTOP_SLIDESHOW_STATE
{
    DSS_ENABLED = 0x1,
    DSS_SLIDESHOW = 0x2,
    DSS_DISABLED_BY_REMOTE_SESSION = 0x4
}

public enum DESKTOP_SLIDESHOW_DIRECTION
{
    DSD_FORWARD = 0,
    DSD_BACKWARD = 1
}


[ComImport, Guid("C2CF3110-460E-4fc1-B9D0-8A1C0C9CC4BD")]
public class DesktopWallpaperClass
{
}

public enum HRESULT : int
{
    S_OK = 0,
    S_FALSE = 1,
    E_NOINTERFACE = unchecked((int)0x80004002),
    E_NOTIMPL = unchecked((int)0x80004001),
    E_FAIL = unchecked((int)0x80004005)
}

public enum SIATTRIBFLAGS
{
    SIATTRIBFLAGS_AND = 0x1,
    SIATTRIBFLAGS_OR = 0x2,
    SIATTRIBFLAGS_APPCOMPAT = 0x3,
    SIATTRIBFLAGS_MASK = 0x3,
    SIATTRIBFLAGS_ALLITEMS = 0x4000
}

[ComImport()]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("b63ea76d-1f85-456f-a19c-48159efa858b")]
public interface IShellItemArray
{
    HRESULT BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, ref IntPtr ppvOut);
    HRESULT GetPropertyStore(GETPROPERTYSTOREFLAGS flags, ref Guid riid, ref IntPtr ppv);
    HRESULT GetPropertyDescriptionList(REFPROPERTYKEY keyType, ref Guid riid, ref IntPtr ppv);
    HRESULT GetAttributes(SIATTRIBFLAGS AttribFlags, int sfgaoMask, ref int psfgaoAttribs);
    HRESULT GetCount(ref int pdwNumItems);
    HRESULT GetItemAt(int dwIndex, ref IShellItem ppsi);
    HRESULT EnumItems(ref IntPtr ppenumShellItems);
}

[ComImport()]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
[Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
public interface IShellItem
{
    [PreserveSig()]
    HRESULT BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, ref IntPtr ppv);
    HRESULT GetParent(ref IShellItem ppsi);
    HRESULT GetDisplayName(SIGDN sigdnName, ref System.Text.StringBuilder ppszName);
    HRESULT GetAttributes(uint sfgaoMask, ref uint psfgaoAttribs);
    HRESULT Compare(IShellItem psi, uint hint, ref int piOrder);
}

public enum SIGDN : int
{
    SIGDN_NORMALDISPLAY = 0x0,
    SIGDN_PARENTRELATIVEPARSING = unchecked((int)0x80018001),
    SIGDN_DESKTOPABSOLUTEPARSING = unchecked((int)0x80028000),
    SIGDN_PARENTRELATIVEEDITING = unchecked((int)0x80031001),
    SIGDN_DESKTOPABSOLUTEEDITING = unchecked((int)0x8004C000),
    SIGDN_FILESYSPATH = unchecked((int)0x80058000),
    SIGDN_URL = unchecked((int)0x80068000),
    SIGDN_PARENTRELATIVEFORADDRESSBAR = unchecked((int)0x8007C001),
    SIGDN_PARENTRELATIVE = unchecked((int)0x80080001)
}

public enum GETPROPERTYSTOREFLAGS
{
    GPS_DEFAULT = 0,
    GPS_HANDLERPROPERTIESONLY = 0x1,
    GPS_READWRITE = 0x2,
    GPS_TEMPORARY = 0x4,
    GPS_FASTPROPERTIESONLY = 0x8,
    GPS_OPENSLOWITEM = 0x10,
    GPS_DELAYCREATION = 0x20,
    GPS_BESTEFFORT = 0x40,
    GPS_NO_OPLOCK = 0x80,
    GPS_PREFERQUERYPROPERTIES = 0x100,
    GPS_EXTRINSICPROPERTIES = 0x200,
    GPS_EXTRINSICPROPERTIESONLY = 0x400,
    GPS_MASK_VALID = 0x7FF
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct REFPROPERTYKEY
{
    private Guid fmtid;
    private int pid;
    public Guid FormatId
    {
        get
        {
            return this.fmtid;
        }
    }
    public int PropertyId
    {
        get
        {
            return this.pid;
        }
    }
    public REFPROPERTYKEY(Guid formatId, int propertyId)
    {
        this.fmtid = formatId;
        this.pid = propertyId;
    }
    public static readonly REFPROPERTYKEY PKEY_DateCreated = new REFPROPERTYKEY(new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 15);
}

public delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, [MarshalAs(UnmanagedType.Struct)] ref RECT lprcMonitor, IntPtr lParam);

static class Native
{
    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr rcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);
}

public class GCHandleProvider : IDisposable
{
    public GCHandleProvider(object target)
    {
        if (target == null)
            throw new ArgumentNullException("target");

        Handle = GCHandle.Alloc(target);
    }

    public IntPtr Pointer => GCHandle.ToIntPtr(Handle);

    public GCHandle Handle { get; }

    private void ReleaseUnmanagedResources()
    {
        if (Handle.IsAllocated)
        {
            Handle.Free();
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~GCHandleProvider()
    {
        ReleaseUnmanagedResources();
    }
}
