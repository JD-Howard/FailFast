using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis.FailFastInternal;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IntegrationTests
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
#if DEBUG
        private bool _isDebugBuild = true;
#else
        private bool _isDebugBuild = false;
#endif
        
        #if RELEASE
        private int stuff = 1;
        #endif

        public string Messages { get; set; } = String.Empty;
        public int LogCount { get; set; }
        //public string Expected => FailFast.Config.CanDebugBreak ? _canDebugTrue : _canDebugFalse;

#if DEBUG
        private string _canDebugTrue = ;
        private string _canDebugFalse = "[DEBUG] FailFast.Config.CanDebug=False\n\nShould produce log messages and no test should not crash or break the running application.";
#endif
        
#if RELEASE
        private string _canDebugTrue = "[DEBUG] FailFast.Config.CanDebug=False\n\nShould produce log messages and no test should not crash or break the running application.";
        private string _canDebugFalse = "";
#endif
        


        private void UpdateTitle()
        {
            var build = _isDebugBuild ? "YES" : "NO";
            var isAttached = Debugger.IsAttached ? "YES" : "NO";
            var canBreak = FailFast.CanDebugBreak ? "YES" : "NO";

            FailFast.When.Throws(() =>
            {

            }).On<DivideByZeroException>(zeroEx =>
            {
                Messages += $"\n{nameof(DivideByZeroException)} was specifically handled";
            }).On<ArgumentException>(argEx =>
            {
                Messages += $"\n{nameof(ArgumentException)} was specifically handled";
            }).OnException(_ =>
            {

            });
            
            Title = $"[FF State] Build:{build} | DebuggerAttached:{isAttached} | CanDebugBreak:{canBreak}";
        }

        private string GetExpectation()
        {
            if (_isDebugBuild && FailFast.CanDebugBreak && Debugger.IsAttached)
                return "Should produce log messages and all of these test should be capable of returning to MainWindow.xaml.cs with a single StepIn or StepOut.";

            return string.Empty;
        }


        #region Setup

        public MainWindow()
        {
#if RELEASE
            FailFast.BreakWhen.True(true);
#endif
            
            
            // Can/Should probably be in App.xaml.cs, but mostly needs initialization prior to use.
            FailFast.Initialize(FFBreakOption.InitFalse, TestCatcher, TestBreakLogging, TestThrowLogging);
            InitializeComponent();
            UpdateTitle();
        }
        
        private Exception? TestCatcher(Action work)
        {
            try { work(); return null; }
            catch (Exception e) { return e; }
        }

        private void TestThrowLogging(Exception error)
        {
            LogCount++;
            Messages += $"\n{error.Message}";
        }

        private void TestBreakLogging(string caller, string routing, object? context)
        {
            LogCount++;
            Messages += $"\n{caller}() => {routing} + {context}";
        }

        #endregion

        

        private void TestThrows_OnClick(object sender, RoutedEventArgs e)
        {
            int num1 = new Random().Next();
            int num2 = 2;

            FailFast.When.Throws(() =>
            {
                DoStuff();
            });

            FailFast.When.Throws(DoStuff);

        }

        private void TestNull_OnClick(object sender, RoutedEventArgs e)
        {
            FailFast.BreakWhen.True(1 == 0);
        }
        private void TestNotNull_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
        
        private void TestTrue_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
        
        private void TestNotTrue_OnClick(object sender, RoutedEventArgs e)
        {
            var info = new System.IO.FileInfo(@"C:\SomeDir\SomeFile.txt");
            if (FailFast.When.NotTrue(info.Exists))
                return;

            
        }

        public void DoStuff()
        {
            Divider(32, 0);
        }

        public int Divider(int @base, int by)
        {
            return @base / by;
        }

        
        
        
    }
}