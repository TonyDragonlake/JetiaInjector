using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;

namespace JetiaInjector.Core.Certification
{
	public class CertificationManager
	{
		private static readonly DateTime s_unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		private readonly SecureRandom secureRandom;
		private AsymmetricCipherKeyPair asymmetricCipherKeyPair;
		private readonly X509Name issuer;
		private readonly X509Name subject;


		public SecureRandom SecureRandom => secureRandom;
		public X509Name Issuer => issuer;
		public X509Name Subject => subject;

		public CertificationManager(string issuer, string subject)
		{
			this.secureRandom = new SecureRandom();
			var generator = new RsaKeyPairGenerator();
			generator.Init(new KeyGenerationParameters(this.secureRandom, 4096));
			this.asymmetricCipherKeyPair = generator.GenerateKeyPair();
			this.issuer = new X509Name("CN=" + issuer);
			this.subject = new X509Name("CN=" + subject);
		}

		public CertificationInfo Build()
		{
			var now = DateTime.UtcNow;
			var certInfo = new CertificationInfo(this.issuer, this.subject, now.AddYears(-1), now.AddYears(10), BigInteger.ValueOf((long)((now - s_unixEpoch).TotalMilliseconds) / 1000));

			var info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(this.asymmetricCipherKeyPair.Private);
			certInfo.PrivateKeyData = new PemObject("PRIVATE KEY", info.ToAsn1Object().GetEncoded());

			var certificateGenerator = new X509V3CertificateGenerator();
			certificateGenerator.SetIssuerDN(certInfo.Issuer);
			certificateGenerator.SetSubjectDN(certInfo.Subject);
			certificateGenerator.SetNotBefore(certInfo.NotBefore);
			certificateGenerator.SetNotAfter(certInfo.NotAfter);
			certificateGenerator.SetSerialNumber(certInfo.SerialNumber);
			certificateGenerator.SetPublicKey(this.asymmetricCipherKeyPair.Public);
			var x509Certificate = certificateGenerator.Generate(new Asn1SignatureFactory("SHA256WITHRSA", this.asymmetricCipherKeyPair.Private, secureRandom));
			certInfo.CertificateData = new PemObject("CERTIFICATE", x509Certificate.GetEncoded());

			return certInfo;
		}

	}
}
