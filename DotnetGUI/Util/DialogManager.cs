﻿using ModernWpf.Controls;
using System;
using System.Threading.Tasks;

namespace DotnetGUI.Util
{
    public class DialogManager
    {
        private static bool isDialogShow = false;

        public static async Task ShowDialogAsync(ContentDialog dialog, Action? primaryCallback = null, Action? secondaryCallback = null, Action? closeCallback = null)
        {
            // 等待直到没有对话框显示
            await WaitForDialogToCloseAsync();

            isDialogShow = true;
            var result = await dialog.ShowAsync();
            isDialogShow = false;

            HandleDialogResult(result, primaryCallback, secondaryCallback, closeCallback);
        }

        private static async Task WaitForDialogToCloseAsync()
        {
            while (isDialogShow)
            {
                await Task.Delay(100);
            }
        }

        private static void HandleDialogResult(ContentDialogResult result, Action? primaryCallback, Action? secondaryCallback = null, Action? closeCallback = null)
        {
            if (result == ContentDialogResult.Primary)
                primaryCallback();
            else if (result == ContentDialogResult.Secondary)
                secondaryCallback();
            else
                closeCallback();
        }
    }
}
