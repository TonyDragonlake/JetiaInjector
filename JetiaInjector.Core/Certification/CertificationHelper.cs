using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;

namespace JetiaInjector.Core.Certification
{
	public static class CertificationHelper
	{
		public static PemObject ReadPemObjectFromFile(string filePath)
		{
			if (!File.Exists(filePath)) { return null; }
			using (var fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var streamReader = new StreamReader(fileReader))
			using (var pemReader = new PemReader(streamReader))
			{
				return pemReader.ReadPemObject();
			}
		}

		public static AsymmetricKeyParameter ReadAsPrivateKeyFromFile(string filePath)
		{
			if (!File.Exists(filePath)) { return null; }
			using (var fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			using (var streamReader = new StreamReader(fileReader))
			using (var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(streamReader))
			{
				var pemObj = pemReader.ReadObject();
				return pemObj as AsymmetricKeyParameter;
			}
		}

		public static X509Certificate ReadAsX509CertificateFromFile(string filePath)
		{
			var certObject = ReadPemObjectFromFile(filePath);
			if (certObject is null) { return null; }
			return new X509Certificate(certObject.Content);
		}

		public static void ToFile(PemObject pemObject, string filePath)
		{
			using (var fileWriter = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
			{
				fileWriter.Seek(0, SeekOrigin.Begin);
				fileWriter.SetLength(0);
				using (var streamWriter = new StreamWriter(fileWriter))
				using (var pemWriter = new PemWriter(streamWriter))
				{
					pemWriter.WriteObject(pemObject);
				}
			}

		}
	}
}
