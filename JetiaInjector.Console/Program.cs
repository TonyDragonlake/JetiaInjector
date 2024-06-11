using SConsole = System.Console;
using JetiaInjector.Core.License;
using JetiaInjector.Core.Certification;
namespace JetiaInjector.Console
{
	internal class Program
	{
		public static readonly string LICENSEE_NAME = "fantazia.org";
		public static readonly string JET_PROFILE_CA_NAME = "JetProfile CA";
		static void Main(string[] args)
		{
			SConsole.WriteLine("Hello, World!");

			string key = "F:\\jetbra\\cert\\ca.key";
			string cert = "F:\\jetbra\\cert\\ca.crt";
			string config = "F:\\jetbra\\config-jetbrains\\power.conf";
			var certificationManager = new CertificationManager(JET_PROFILE_CA_NAME, LICENSEE_NAME);
			var certInfo = certificationManager.Build();
			var keyData = certInfo.PrivateKeyData;
			var certData = certInfo.CertificateData;
			CertificationHelper.ToFile(keyData, key);
			CertificationHelper.ToFile(certData, cert);
			var privateKey = CertificationHelper.ReadAsPrivateKeyFromFile(key);
			var certObject = CertificationHelper.ReadPemObjectFromFile(cert);
			string configContent = PowerConfigHelper.BuildConfig(certObject);
			WriteStringToFile(config, configContent);
			LicenseManager licenseManager = new LicenseManager(LICENSEE_NAME);
			licenseManager.ActivateAll(privateKey, certObject);
			var code = licenseManager.ActivationCode;
			SConsole.Write("Activation Code = \n{0}\n", code);
		}

		static void WriteStringToFile(string filePath, string data)
		{
			using (var fileWriter = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
			{
				fileWriter.Seek(0, SeekOrigin.Begin);
				fileWriter.SetLength(0);
				using (var streamWriter = new StreamWriter(fileWriter))
				{
					streamWriter.Write(data);
				}
			}
		}
	}
}
