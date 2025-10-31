using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DotnetGUI
{
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true; // 阻止崩溃
            HandleException(e.Exception);
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            e.SetObserved(); // 标记已处理
            HandleException(e.Exception);
        }

        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true; // 阻止崩溃
            HandleException(e.Exception);
        }

        private void HandleException(Exception exception)
        {
            MessageBox.Show($"发生了未捕获的错误\n\n=================================\n\n{exception}", "发生错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}