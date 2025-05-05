using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Windows;

namespace IntegrationTests
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotify implementation

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private bool SetField<T>(ref T field, T value, Action<string> thenDo, [CallerMemberName] string? propertyName = null)
        {
            SetField(ref field, value, propertyName);
            thenDo(string.Empty);
            return true;
        }

        #endregion

        
        private int _logHits = 0;
        public int LogCount { get => _logHits; set => SetField(ref _logHits, value, UpdateTitle); } 
        
        private void UpdateTitle(string suffix = "")
        {
            var hasDebugDirective = false;
            UpdateDebugStatus(ref hasDebugDirective);
            Title = $"{hasDebugDirective} -- Log Hit Count: {LogCount} {suffix}".Trim();
        }


        public bool IsStatic => FailFast.Configure.BreakBehavior >= FailFast.FFBreakOption.StaticFalse;
        
        
        public string BreakStateName { get; set; } = "InitFalse";
        
        
        private bool? _break = false; // field for a 3-state checkbox
        public bool? CanBreak 
        { 
            get => _break;
            set
            {
                _break = IsStatic ? null : value;
                if (value == false)
                    FailFast.Configure.BreakBehavior = FailFast.FFBreakOption.InitFalse;
                else if (value == true)
                    FailFast.Configure.BreakBehavior = FailFast.FFBreakOption.InitTrue;
                else
                    FailFast.Configure.BreakBehavior = FailFast.FFBreakOption.StaticTrue;

                BreakStateName = FailFast.Configure.BreakBehavior.ToString();;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BreakStateName));
                OnPropertyChanged(nameof(IsStatic));
            } 
        }
 
        

        #region Setup
        
        private void AddLogHandlers_OnClick(object sender, RoutedEventArgs e)
        {
            FailFast.Configure.BreakLogHandler = TestBreakLogging;
            FailFast.Configure.ThrowsLogHandler = TestThrowLogging;
        }

        public MainWindow()
        {
#if !DEBUG
            // FailFast should cause compile errors whenever a Non-DEBUG configuration is built
            // using the FailFast.BreakWhen path and even runtime errors if for some reason the
            // compiler directives were bypassed. The BreakWhen is specifically for JIT purposes
            // where a specific block of code is being worked on and an IDE breakpoint condition
            // would likely cause significantly more overhead than a temporary BreakWhen condition.
            FailFast.BreakWhen.True(true);
#endif
            
            // Should probably be in App.xaml.cs in normal WPF implementations.
            FailFast.Configure.BreakBehavior = FailFast.FFBreakOption.InitFalse;
            
            InitializeComponent();
            UpdateTitle();
        }
        
        

        private void TestThrowLogging(string caller, ExceptionDispatchInfo error) 
            => LogCount++;
        
        private void TestBreakLogging(string caller, string routing, object? context) 
            => LogCount++;

        #endregion

        

        


        #region FailFast.Configure authorized break tests
        
        public void HasErrorAction1() 
            => throw new DivideByZeroException();
        
        public void HasErrorAction2() 
            => throw new DataMisalignedException();
        
        public void ErrorFreeAction() 
            => UpdateLayout();
        
        private void TestThrows_OnClick(object sender, RoutedEventArgs e)
        {
            // UpdateTitle() should not be run during this first FF.Throws chain 
            FailFast.When.Throws(ErrorFreeAction)
                         .OnException(ex => UpdateTitle(ex.GetType().Name));
            
            // If Logging is added, then UpdateTitle() should run for Throws() and in OnException.
            // However, UpdateTitle() should only run in OnException if log handlers are not used.
            FailFast.When.Throws(HasErrorAction2)
                         .OnException(ex => UpdateTitle(ex.GetType().Name));
        }
        
        private void TestThrowsOnT_OnClick(object sender, RoutedEventArgs e)
        {
            // In this situation the OnException should not execute UpdateTitle() because
            // the DivideByZero was the exception that HasErrorAction1 threw.
            FailFast.When.Throws(HasErrorAction1)
                         .On<DivideByZeroException>(ex => UpdateTitle($"On<{ex.GetType().Name}>"))
                         .OnException(ex => UpdateTitle($"On<{ex.GetType().Name}>"));
        }

        private void TestNull_OnClick(object sender, RoutedEventArgs e)
            => FailFast.When.Null(null);

        private void TestNotNull_OnClick(object sender, RoutedEventArgs e) 
            => FailFast.When.NotNull(new object());

        private void TestTrue_OnClick(object sender, RoutedEventArgs e)
            => FailFast.When.True(true);

        private void TestNotTrue_OnClick(object sender, RoutedEventArgs e) 
            => FailFast.When.NotTrue(false);

        #endregion



        #region Explicit (ignores FailFast.Configure) break tests

        #if DEBUG // BreakWhen usage intentionally causes build errors in non-Debug builds
        private void ExplicitTestNull_OnClick(object sender, RoutedEventArgs e) 
            => FailFast.BreakWhen.Null(null);
        
        private void ExplicitTestNotNull_OnClick(object sender, RoutedEventArgs e) 
            => FailFast.BreakWhen.Null(new object());
        
        private void ExplicitTestTrue_OnClick(object sender, RoutedEventArgs e) 
            => FailFast.BreakWhen.True(true);
        
        private void ExplicitTestNotTrue_OnClick(object sender, RoutedEventArgs e) 
            => FailFast.BreakWhen.True(false);
        
        #else // These empty explicits fulfill the xaml contract for non-Debug builds
        
        private void ExplicitTestNull_OnClick(object sender, RoutedEventArgs e) {}
        private void ExplicitTestNotNull_OnClick(object sender, RoutedEventArgs e) {}
        private void ExplicitTestTrue_OnClick(object sender, RoutedEventArgs e) {}
        private void ExplicitTestNotTrue_OnClick(object sender, RoutedEventArgs e) {}
        
        #endif
        
        #endregion
        
        
        

        
        
        [Conditional("DEBUG")] // Only used to verify some FailFast runtime assumptions 
        private static void UpdateDebugStatus(ref bool isDebugMode) => isDebugMode = true;
        

        
    }
}