using System.Linq;
using ShellExtensions;
using ShellExtensions.Helpers;
using Misend;
namespace ExplorerCommandHandler
{
    public class ContextMenu : ExplorerCommand
    {
        public readonly string icon;

        public ContextMenu()
        {
            this.icon = @"C:\Program Files\MI\XiaomiPCManager\5.3.0.328\Assets\midrop_logo.ico";
        }

        public override string? GetIcon(ShellItemArray shellItems)
        {
            return this.icon;
        }

        public override string? GetTitle(ShellItemArray shellItems)
        {
            return "使用小米互传发送";
        }

        public override ExplorerCommandState GetState(ShellItemArray shellItems, bool fOkToBeSlow, out bool pending)
        {
            pending = false;
            if (this.ServiceProvider.GetService<ShellExtensions.IContextMenuTypeAccessor>() is { } accessor)
            {
                if (accessor.ContextMenuType == ContextMenuType.ModernContextMenu
                    || accessor.ContextMenuType == ContextMenuType.Unknown)
                {
                    return ExplorerCommandState.ECS_ENABLED;
                }
            }

            return ExplorerCommandState.ECS_HIDDEN;
        }

        public override unsafe void Invoke(ExplorerCommandInvokeEventArgs args)
        {
            var files = args.ShellItems.Select(c => c.FullPath).ToArray();
            Misend.DllMain.SendToXiaomiPcManager(files);
        }

    }
}