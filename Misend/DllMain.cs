using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Misend
{
    public static class DllMain
    {
        public static unsafe void SendToXiaomiPcManager(string[] files)
        {
            const string WindowClassName = "XiaomiPCManager";

            if (files != null && files.Length > 0)
            {
                fixed (char* pClassName = WindowClassName)
                {
                    var hWnd = FindWindowW(pClassName, null);

                    if (hWnd != 0)
                    {
                        var sb = new StringBuilder();
                        for (int i = 0; i < files.Length; i++)
                        {
                            var path = files[i];

                            if (!string.IsNullOrEmpty(path)
                                && Path.IsPathRooted(path))
                            {
                                if (i != 0) sb.Append('|');
                                sb.Append(path);
                            }
                        }

                        sb.Append('\0');
                        var file = sb.ToString();

                        fixed (char* ptr = file)
                        {
                            var s = default(COPYDATASTRUCT);
                            s.dwData = 0;
                            s.cbData = (file.Length) * 2;
                            s.lpData = (nint)ptr;

                            SendMessageW(hWnd, 74, (IntPtr)1, (nint)(&s));
                        }
                    }
                }
            }
        }

        [DllImport("user32.dll")]
        private static unsafe extern nint SendMessageW(nint hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static unsafe extern nint FindWindowW(char* lpClassName, char* lpWindowName);

        private struct COPYDATASTRUCT
        {
            public nint dwData;
            public int cbData;
            public nint lpData;
        }
    }
}