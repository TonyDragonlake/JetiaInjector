using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;

using JetiaInjector.Avalonia.ViewModels;
using JetiaInjector.Avalonia.Views;

namespace JetiaInjector.Avalonia;

public partial class App : Application
{
	public static TopLevel? CurrentTopLevel
	{
		get
		{
			IClassicDesktopStyleApplicationLifetime? desktop = Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
			if (desktop != null)
			{
				return TopLevel.GetTopLevel(desktop.MainWindow);
			}
			ISingleViewApplicationLifetime? singleViewPlatform = Current?.ApplicationLifetime as ISingleViewApplicationLifetime;
			if(singleViewPlatform != null)
			{
				return TopLevel.GetTopLevel(singleViewPlatform.MainView);
			}
			return null;
		}
	}

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		// Line below is needed to remove Avalonia data validation.
		// Without this line you will get duplicate validations from both Avalonia and CT
		BindingPlugins.DataValidators.RemoveAt(0);

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow
			{
				DataContext = new MainViewModel()
			};
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
		{
			singleViewPlatform.MainView = new MainView
			{
				DataContext = new MainViewModel()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}
}
