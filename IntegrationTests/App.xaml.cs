using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis.FailFastInternal;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IntegrationTests;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static int LogCount { get; set; }
    
    private Exception? Catcher(Action exp)
    {
        try
        {
            exp();
            return null;
        }
        catch (Exception e)
        {
            return e;
        }   
    }
        
    public App()
    {
        FailFast.Initialize(FFBreakOption.InitTrue, Catcher);
    }

    
}