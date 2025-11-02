using DotnetGUI.Class;
using ModernWpf.Controls;
using System;
using System.Threading.Tasks;

namespace DotnetGUI.Util
{
    public class DialogManager
    {
        private static bool isDialogShow = false;

        public static async Task ShowDialogAsync(ContentDialog dialog, Action? primaryCallback = null, Action? secondaryCallback = null, Action? closeCallback = null)
        {
            Globals.logger.Info($"[对话框管理器]: 添加队列(Title: {dialog.Title}, Content: {dialog.Content})");
            // 等待直到没有对话框显示
            await WaitForDialogToCloseAsync();

            isDialogShow = true;

            Globals.logger.Info($"[对话框管理器]: 显示对话框");
            
            var result = await dialog.ShowAsync();
            
            Globals.logger.Info($"[对话框管理器]: 对话框关闭, 用户选择: {result}");
            
            isDialogShow = false;

            Globals.logger.Info($"[对话框管理器]: 处理选择回调");
            HandleDialogResult(result, primaryCallback, secondaryCallback, closeCallback);
        }

        private static async Task WaitForDialogToCloseAsync()
        {
            while (isDialogShow)
            {
                Globals.logger.Info($"等待当前对话框退出...");
                await Task.Delay(100);
            }
        }

        private static void HandleDialogResult(ContentDialogResult result, Action? primaryCallback, Action? secondaryCallback = null, Action? closeCallback = null)
        {
            if (result == ContentDialogResult.Primary)
                primaryCallback?.Invoke();
            else if (result == ContentDialogResult.Secondary)
                secondaryCallback?.Invoke();
            else
                closeCallback?.Invoke();
        }
    }
}
