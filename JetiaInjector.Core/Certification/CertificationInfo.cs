using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace JetiaInjector.Core.Certification
{
	public class CertificationInfo
	{
		private readonly X509Name issuer;
		private readonly X509Name subject;
		private readonly DateTime notBefore;
		private readonly DateTime notAfter;
		private readonly BigInteger serialNumber;
		private PemObject certificateData;
		private PemObject keyData;

		public CertificationInfo(X509Name issuer, X509Name subject, DateTime notBefore, DateTime notAfter, BigInteger serialNumber)
		{
			this.issuer = issuer;
			this.subject = subject;
			this.notBefore = notBefore;
			this.notAfter = notAfter;
			this.serialNumber = serialNumber;
		}

		public X509Name Issuer => issuer;

		public X509Name Subject => subject;

		public DateTime NotBefore => notBefore;

		public DateTime NotAfter => notAfter;

		public BigInteger SerialNumber => serialNumber;

		public PemObject CertificateData { get => certificateData; internal set => certificateData = value; }

		public PemObject PrivateKeyData { get => keyData; internal set => keyData = value; }
	}
}
