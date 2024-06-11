using Avalonia.Platform.Storage;

namespace JetiaInjector.Avalonia.ViewModels
{
	public static class CertificationFileTypes
	{
		public static FilePickerFileType PrivateKey { get; } = new FilePickerFileType("Private Key")
		{
			Patterns = new string[]
			{
			"*.key"
			},
			MimeTypes = new string[]
			{
			"application/pgp-keys"
			}
		};
		public static FilePickerFileType Certification { get; } = new FilePickerFileType("Certification")
		{
			Patterns = new string[]
			{
			"*.crt"
			},
			MimeTypes = new string[]
			{
			"application/x-x509-ca-cert",
			"application/x-x509-ca-ra-cert",
			"application/x-x509-next-ca-cert"
			}
		};
	}
}