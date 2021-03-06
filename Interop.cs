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
}

[ComImport]
[Guid("B92B56A9-8B55-4E14-9A89-0199BBB6F93B")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IDesktopWallpaper
{
    HRESULT SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] string wallpaper);
    HRESULT GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string monitorID, [MarshalAs(UnmanagedType.LPWStr)] out string wallpaper);
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

internal enum SM
{
	CXSCREEN = 0,
	CYSCREEN = 1,
	CXVSCROLL = 2,
	CYHSCROLL = 3,
	CYCAPTION = 4,
	CXBORDER = 5,
	CYBORDER = 6,
	CXFIXEDFRAME = 7,
	CYFIXEDFRAME = 8,
	CYVTHUMB = 9,
	CXHTHUMB = 10,
	CXICON = 11,
	CYICON = 12,
	CXCURSOR = 13,
	CYCURSOR = 14,
	CYMENU = 0xF,
	CXFULLSCREEN = 0x10,
	CYFULLSCREEN = 17,
	CYKANJIWINDOW = 18,
	MOUSEPRESENT = 19,
	CYVSCROLL = 20,
	CXHSCROLL = 21,
	DEBUG = 22,
	SWAPBUTTON = 23,
	CXMIN = 28,
	CYMIN = 29,
	CXSIZE = 30,
	CYSIZE = 0x1F,
	CXFRAME = 0x20,
	CXSIZEFRAME = 0x20,
	CYFRAME = 33,
	CYSIZEFRAME = 33,
	CXMINTRACK = 34,
	CYMINTRACK = 35,
	CXDOUBLECLK = 36,
	CYDOUBLECLK = 37,
	CXICONSPACING = 38,
	CYICONSPACING = 39,
	MENUDROPALIGNMENT = 40,
	PENWINDOWS = 41,
	DBCSENABLED = 42,
	CMOUSEBUTTONS = 43,
	SECURE = 44,
	CXEDGE = 45,
	CYEDGE = 46,
	CXMINSPACING = 47,
	CYMINSPACING = 48,
	CXSMICON = 49,
	CYSMICON = 50,
	CYSMCAPTION = 51,
	CXSMSIZE = 52,
	CYSMSIZE = 53,
	CXMENUSIZE = 54,
	CYMENUSIZE = 55,
	ARRANGE = 56,
	CXMINIMIZED = 57,
	CYMINIMIZED = 58,
	CXMAXTRACK = 59,
	CYMAXTRACK = 60,
	CXMAXIMIZED = 61,
	CYMAXIMIZED = 62,
	NETWORK = 0x3F,
	CLEANBOOT = 67,
	CXDRAG = 68,
	CYDRAG = 69,
	SHOWSOUNDS = 70,
	CXMENUCHECK = 71,
	CYMENUCHECK = 72,
	SLOWMACHINE = 73,
	MIDEASTENABLED = 74,
	MOUSEWHEELPRESENT = 75,
	XVIRTUALSCREEN = 76,
	YVIRTUALSCREEN = 77,
	CXVIRTUALSCREEN = 78,
	CYVIRTUALSCREEN = 79,
	CMONITORS = 80,
	SAMEDISPLAYFORMAT = 81,
	IMMENABLED = 82,
	CXFOCUSBORDER = 83,
	CYFOCUSBORDER = 84,
	TABLETPC = 86,
	MEDIACENTER = 87,
	REMOTESESSION = 0x1000,
	REMOTECONTROL = 8193
}

static class Native
{
    [DllImport("user32.dll")]
    public static extern int GetSystemMetrics(SM nIndex);
}
