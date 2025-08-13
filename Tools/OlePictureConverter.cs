using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using stdole;

namespace QuickJump2022.Tools;

public static class OlePictureConverter {
    public static BitmapSource ConvertStdPictureToBitmapSource(StdPicture stdPictureObject) {
        if (stdPictureObject == null) {
            return null;
        }

        // Cast to IPictureDisp to access the Handle
        stdole.IPictureDisp pictureDisp = (stdole.IPictureDisp)stdPictureObject;

        // Create a GDI+ Bitmap from the HBITMAP handle
        Bitmap gdiBitmap = null;
        try {
            gdiBitmap = Bitmap.FromHbitmap(new IntPtr(pictureDisp.Handle));

            // Convert GDI+ Bitmap to WPF BitmapSource
            using (MemoryStream ms = new MemoryStream()) {
                gdiBitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // Save as PNG or another format
                ms.Position = 0;

                BitmapImage bitmapSource = new BitmapImage();
                bitmapSource.BeginInit();
                bitmapSource.StreamSource = ms;
                bitmapSource.CacheOption = BitmapCacheOption.OnLoad; // Cache the image data
                bitmapSource.EndInit();
                bitmapSource.Freeze(); // Make the BitmapSource immutable for better performance

                return bitmapSource;
            }
        }
        finally {
            if (gdiBitmap != null) {
                gdiBitmap.Dispose(); // Dispose the GDI+ Bitmap
            }
        }
    }

    public static Icon StdPictureToIcon(stdole.StdPicture picture) {
        Bitmap bitmap = PictureDispConverter.ToBitmap(picture);
        if (bitmap == null) {
            return null;
        }
        return Icon.FromHandle(bitmap.GetHicon());
    }

    // Helper class for conversions
    private static class PictureDispConverter {
        [DllImport("OleAut32.dll", EntryPoint = "OleCreatePictureIndirect", ExactSpelling = true, PreserveSig = false)]
        private static extern IPictureDisp OleCreatePictureIndirect(
            [In, MarshalAs(UnmanagedType.AsAny)] object picdesc,
            [In] ref Guid iid,
            [In] bool fOwn);

        private static Guid iPictureDispGuid = typeof(IPictureDisp).GUID;

        [StructLayout(LayoutKind.Sequential)]
        private class PICTDESC {
            internal int cbSize = Marshal.SizeOf(typeof(PICTDESC));
            internal int picType;
            internal IntPtr hPal = IntPtr.Zero;
            internal int xExt;
            internal int yExt;
            internal IntPtr hBitmap = IntPtr.Zero;
        }

        private const short PICTYPE_BITMAP = 1;

        public static Bitmap ToBitmap(stdole.StdPicture picture) {
            if (picture == null) {
                return null;
            }
            var picdesc = new PICTDESC();
            picdesc.picType = PICTYPE_BITMAP;
            picdesc.hBitmap = (IntPtr)picture.Handle;
            picdesc.hPal = (IntPtr)picture.hPal;
            picdesc.xExt = picture.Width;
            picdesc.yExt = picture.Height;
            var iPic = OleCreatePictureIndirect(picdesc, ref iPictureDispGuid, true);
            var picHandle = new IntPtr(iPic.Handle);
            return Bitmap.FromHbitmap(picHandle);
        }
    }
}