using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExplorerCommandHandler
{
    public static class DllMain
    {
        private static readonly Guid PackagedClsid = new Guid("D32D42C1-682D-440F-A323-82D5B5D84B67");

#pragma warning disable CA2255
        [ModuleInitializer]
#pragma warning restore CA2255
        public static void _DllMain()
        {

            //ShellExtensions.ShellExtensionsClassFactory.RegisterInProcess(PackagedClsid, () => new ContextMenu(Path.Combine(path, "Assets", "Main.ico")));
            ShellExtensions.ShellExtensionsClassFactory.RegisterInProcess(PackagedClsid, () => new ContextMenu());
        }

        [UnmanagedCallersOnly(EntryPoint = "DllCanUnloadNow")]
        private static int DllCanUnloadNow() => ShellExtensions.ShellExtensionsClassFactory.DllCanUnloadNow();

        [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
        private unsafe static int DllGetClassObject(Guid* clsid, Guid* riid, void** ppv) => ShellExtensions.ShellExtensionsClassFactory.DllGetClassObject(clsid, riid, ppv);
    }
}
