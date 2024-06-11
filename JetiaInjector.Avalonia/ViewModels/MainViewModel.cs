using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using JetiaInjector.Core.Certification;
using JetiaInjector.Core.License;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;

namespace JetiaInjector.Avalonia.ViewModels;

public struct TaskResult
{
	public bool Success { get; set; }
	public string Message { get; set; }
}


public partial class MainViewModel : ViewModelBase
{
	public static readonly string JET_PROFILE_CA_NAME = "JetProfile CA";
	public static readonly string LICENSEE_NAME = "fantazia.org";
	private DispatcherTimer timer;

	private string message;

	private bool isInputEnabled;
	private int currentAction;

	private readonly string certIssuer;
	private string certSubject;
	private string certDir;
	private readonly AsyncRelayCommand selectCertDirCmd;
	private readonly AsyncRelayCommand genCertCmd;
	private string keyFilePath;
	private string certFilePath;
	private readonly AsyncRelayCommand selectKeyFileCmd;
	private readonly AsyncRelayCommand selectCertFileCmd;
	private string configDir;
	private readonly AsyncRelayCommand selectConfigDirCmd;

	private string activationCode;
	private string configPath;
	private readonly AsyncRelayCommand genActivationCmd;

	private readonly AsyncRelayCommand copyActivationCmd;

	public MainViewModel()
	{
		this.timer = new DispatcherTimer() { 
			Interval = TimeSpan.FromSeconds(5),
		};
		this.message = "就绪";
		this.timer.Tick += OnTimerTick;
		this.isInputEnabled = true;
		this.certIssuer = JET_PROFILE_CA_NAME;
		this.certSubject = LICENSEE_NAME;
		this.selectCertDirCmd = new AsyncRelayCommand(DoSelectCertificationDirectory);
		this.genCertCmd = new AsyncRelayCommand(DoGenerateCertification);
		this.selectKeyFileCmd = new AsyncRelayCommand(DoSelectPrivateKeyFile);
		this.selectCertFileCmd = new AsyncRelayCommand(DoSelectCertificationFile);
		this.selectConfigDirCmd = new AsyncRelayCommand(DoSelectPowerConfigDirectory);
		this.genActivationCmd = new AsyncRelayCommand(DoGenerateActivationCode);
		this.copyActivationCmd = new AsyncRelayCommand(DoCopyActivationCode);

	}

	private Task DoGenerateActivationCode()
	{
		if (!Path.Exists(this.keyFilePath))
		{
			Message = "ERROR: 密钥文件路径不存在";
			this.timer.Start();
			return Task.CompletedTask;
		}
		if (!Path.Exists(this.certFilePath))
		{
			Message = "ERROR: 证书文件路径不存在";
			this.timer.Start();
			return Task.CompletedTask;
		}
		if (!CheckFilePath(this.configDir))
		{
			Message = "ERROR: 配置文件路径不合法";
			this.timer.Start();
			return Task.CompletedTask;
		}
		IsInputEnabled = false;
		Message = "正在生成配置文件和激活码...";
		return Task.Run(DoPowerConfigAndActivationCodeGeneration)
			.ContinueWith(t =>
			{
				IsInputEnabled = true;
				Message = t.Result.Success
					? "已生成配置文件和激活码"
					: $"ERROR: {t.Result.Message}";
				this.timer.Start();
			});
	}

	private TaskResult DoPowerConfigAndActivationCodeGeneration()
	{
		try
		{
			var privateKey = CertificationHelper.ReadAsPrivateKeyFromFile(this.keyFilePath);
			var certObject = CertificationHelper.ReadPemObjectFromFile(this.certFilePath);
			var dirInfo = Directory.CreateDirectory(Path.Combine(this.configDir, "config-jetbrains"));
			string configFile = Path.Combine(dirInfo.FullName, "power.conf");
			PowerConfigHelper.BuildConfigToFile(certObject, configFile);
			PowerConfigFilePath = configFile;
			LicenseManager licenseManager = new LicenseManager(this.certSubject);
			licenseManager.ActivateAll(privateKey, certObject);
			ActivationCode = licenseManager.ActivationCode;
			return new TaskResult() { Success = true };
		}
		catch (Exception e)
		{
			return new TaskResult() { Success = false, Message = e.Message };
		}
	}

	private Task DoCopyActivationCode()
	{
		if (string.IsNullOrWhiteSpace(activationCode)) { return Task.CompletedTask; }
		var clipBoard = App.CurrentTopLevel?.Clipboard;
		if (clipBoard == null) { return Task.CompletedTask; }
		return clipBoard.SetTextAsync(activationCode)
			.ContinueWith(t =>
			{
				Message = "已复制激活码到剪贴板";
				this.timer.Start();
			});
	}

	private Task DoSelectPowerConfigDirectory()
	{
		var topLevel = App.CurrentTopLevel;
		if (topLevel == null) { return Task.CompletedTask; }
		var storage = topLevel.StorageProvider;
		return storage.OpenFolderPickerAsync(new FolderPickerOpenOptions())
			.ContinueWith(t =>
			{
				var storageFiles = t.Result;
				if (storageFiles != null && storageFiles.Count > 0)
				{
					var folder = storageFiles[0];
					var localPath = folder.TryGetLocalPath();
					if (localPath != null)
					{
						PowerConfigDirectory = localPath;
					}
				}
			});
	}

	private Task DoSelectPrivateKeyFile()
	{
		var topLevel = App.CurrentTopLevel;
		if (topLevel == null) { return Task.CompletedTask; }
		var storage = topLevel.StorageProvider;
		return storage.OpenFilePickerAsync(new FilePickerOpenOptions()
		{
			FileTypeFilter = new List<FilePickerFileType>()
			{ CertificationFileTypes.PrivateKey,
				FilePickerFileTypes.TextPlain,
				FilePickerFileTypes.All
			}
		})
		.ContinueWith(t =>
		{
			var storageFiles = t.Result;
			if (storageFiles != null && storageFiles.Count > 0)
			{
				var file = storageFiles[0];
				var localPath = file.TryGetLocalPath();
				if (localPath != null)
				{
					PrivateKeyFilePath = localPath;
				}
			}
		});
	}

	private Task DoSelectCertificationFile()
	{
		var topLevel = App.CurrentTopLevel;
		if (topLevel == null) { return Task.CompletedTask; }
		var storage = topLevel.StorageProvider;
		return storage.OpenFilePickerAsync(new FilePickerOpenOptions()
		{
			FileTypeFilter = new List<FilePickerFileType>()
			{ CertificationFileTypes.Certification,
				FilePickerFileTypes.TextPlain,
				FilePickerFileTypes.All
			}
		})
		.ContinueWith(t =>
		{
			var storageFiles = t.Result;
			if (storageFiles != null && storageFiles.Count > 0)
			{
				var file = storageFiles[0];
				var localPath = file.TryGetLocalPath();
				if (localPath != null)
				{
					CertificationFilePath = localPath;
				}
			}
		});
	}

	private void OnTimerTick(object? sender, EventArgs e)
	{
		Message = "就绪";
	}

	private Task DoGenerateCertification()
	{
		var ok = CheckFilePath(this.certDir);
		if (!ok)
		{
			Message = "ERROR: 文件路径不合法";
			this.timer.Start();
			return Task.CompletedTask;
		}
		IsInputEnabled = false;
		Message = "正在生成CA证书...";
		return Task.Run(DoCACertificationGeneration)
			.ContinueWith(t =>
			{
				IsInputEnabled = true;
				Message = t.Result.Success 
					? "已生成CA证书" 
					: $"ERROR: {t.Result.Message}";
				this.timer.Start();
			});
	}

	private TaskResult DoCACertificationGeneration()
	{
		try
		{
			var dirInfo = Directory.CreateDirectory(Path.Combine(this.certDir, "cert"));
			string key = Path.Combine(dirInfo.FullName, "ca.key");
			string cert = Path.Combine(dirInfo.FullName, "ca.crt");
			var certificationManager = new CertificationManager(this.certIssuer, this.certSubject);
			var certInfo = certificationManager.Build();
			var keyData = certInfo.PrivateKeyData;
			var certData = certInfo.CertificateData;
			CertificationHelper.ToFile(keyData, key);
			CertificationHelper.ToFile(certData, cert);
			return new TaskResult() { Success = true};
		}
		catch (Exception e)
		{
			return new TaskResult() { Success = false, Message = e.Message };
		}
	}

	private static bool CheckFilePath(string filePath)
	{
		if (string.IsNullOrWhiteSpace(filePath))
		{
			return false;
		}

		if (Path.IsPathFullyQualified(filePath))
		{
			try
			{
				var fullPath = Path.GetFullPath(filePath);
				return string.Equals(fullPath, filePath, StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				return false;
			}
		}
		return false;
	}

	private Task DoSelectCertificationDirectory()
	{
		var topLevel = App.CurrentTopLevel;
		if (topLevel == null) { return Task.CompletedTask; }
		var storage = topLevel.StorageProvider;
		return storage.OpenFolderPickerAsync(new FolderPickerOpenOptions())
			.ContinueWith(t =>
			{
				var storageFiles = t.Result;
				if (storageFiles != null && storageFiles.Count > 0)
				{
					var folder = storageFiles[0];
					var localPath = folder.TryGetLocalPath();
					if (localPath != null)
					{
						CertificationDirectory = localPath;
					}
				}
			});
	}

	public string[] InjectorActions => new string[] { "认证", "激活" };

	public string Message
	{
		get => message;
		set
		{
			if (message != value)
			{
				message = value;
				OnPropertyChanged();
			}
		}
	}

	public bool IsInputEnabled
	{
		get => isInputEnabled; set
		{
			if (isInputEnabled != value)
			{
				isInputEnabled = value;
				OnPropertyChanged();
			}
		}
	}

	public int CurrentAction
	{
		get => currentAction;
		set
		{
			if (currentAction != value)
			{
				currentAction = value;
				OnPropertyChanged();
			}
		}
	}


	public string CertificationIssuer => certIssuer;

	public string CertificationSubject 
	{
		get => certSubject;
		set
		{
			if (certSubject != value)
			{
				certSubject = value;
				OnPropertyChanged();
			}
		}
	}

	public string CertificationDirectory
	{
		get => certDir;
		set
		{
			if (certDir != value)
			{
				certDir = value;
				OnPropertyChanged();
			}
		}
	}
	
	public AsyncRelayCommand SelectCertificationDirectoryCommand => selectCertDirCmd;

	public AsyncRelayCommand GenerateCertificationCommand => genCertCmd;


	public string PrivateKeyFilePath
	{
		get => keyFilePath; 
		set
		{
			if (keyFilePath != value)
			{
				keyFilePath = value;
				OnPropertyChanged();
			}
		}
	}

	public string CertificationFilePath
	{
		get => certFilePath; 
		set
		{
			if (certFilePath != value)
			{
				certFilePath = value;
				OnPropertyChanged();
			}
		}
	}

	public AsyncRelayCommand SelectPrivateKeyFileCommand => selectKeyFileCmd;

	public AsyncRelayCommand SelectCertificationFileCommand => selectCertFileCmd;

	public string PowerConfigDirectory
	{
		get => configDir;
		set
		{
			if (configDir != value)
			{
				configDir = value;
				OnPropertyChanged();
			}
		}
	}

	public AsyncRelayCommand SelectPowerConfigDirectoryCommand => selectConfigDirCmd;

	public string ActivationCode
	{
		get => activationCode;
		private set
		{
			activationCode = value;
			OnPropertyChanged();
		}
	}

	public AsyncRelayCommand GenerateActivationCodeCommand => genActivationCmd;

	public AsyncRelayCommand CopyActivationCodeCommand => copyActivationCmd;

	public string PowerConfigFilePath
	{
		get => configPath; 
		private set
		{
			configPath = value;
			OnPropertyChanged();
		}
	}
}
