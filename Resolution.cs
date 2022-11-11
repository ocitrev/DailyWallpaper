using System.Runtime.InteropServices;
struct Resolution : IComparable
{
    public int Width;
    public int Height;

    public int CompareTo(object obj)
    {
        if (obj == null)
            return 1;

        if (obj is Resolution r)
        {
            int areaCompare = (Width * Height).CompareTo(r.Width * r.Height);

            if (areaCompare != 0)
                return areaCompare;

            int widthCompare = Width.CompareTo(r.Width);

            if (widthCompare != 0)
                return widthCompare;

            return Height.CompareTo(r.Height);
        }

        throw new ArgumentException();
    }

    public override string ToString()
    {
        return $"{Width}x{Height}";
    }

    // returns all monitor resolutions sorted from largest to smallest
    public static IEnumerable<Resolution> GetAllMonitorResolution()
    {
        List<Resolution> monitorResolutions = new();
        using (GCHandleProvider handle = new(monitorResolutions))
        {
            Native.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, new MonitorEnumProc(MonitorEnumCallback), handle.Pointer);
        }

        return monitorResolutions;
    }

    private static bool MonitorEnumCallback(IntPtr monitor, IntPtr hdc, [MarshalAs(UnmanagedType.Struct)] ref RECT lprcMonitor, IntPtr lparam)
    {
        GCHandle handle = GCHandle.FromIntPtr(lparam);
        List<Resolution> monitorResolutions = (List<Resolution>)handle.Target;
        monitorResolutions.Add(new Resolution { Width = lprcMonitor.Width, Height = lprcMonitor.Height });
        return true;
    }
}
