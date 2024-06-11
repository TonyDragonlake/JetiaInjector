using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.X509;
using System.Text;
using System.Text.Json;

namespace JetiaInjector.Core.License
{
	public class LicenseManager
	{
		public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

		private static readonly DateTime s_defaultExpireDate = new DateTime(2099, 09, 14);

		public static string NewRandomLicenseId(int length = 10)
		{
			if (length <= 0) return string.Empty;
			return new string(Random.Shared.GetItems<char>("ABCDEFGHIJKLMNOPQRSTUVWXYZ", length));
		}

		private readonly string licenseId;
		private readonly string licenseeName;
		private DateTime expireDate = s_defaultExpireDate;
		private string licenseCode;
		private string activationCode;
		public string LicenseId => licenseId;
		public string LicenseeName => licenseeName;
		public DateTime ExpireDate { get => expireDate; set => expireDate = value; }

		public string LicenseCode { get => licenseCode; }
		public string ActivationCode { get => activationCode; }

		public LicenseManager(string licenseId, string licenseeName)
		{
			if (string.IsNullOrWhiteSpace(licenseId))
			{
				throw new ArgumentException($"{nameof(licenseId)} cannot be null or whitespace", nameof(licenseId));
			}

			if (string.IsNullOrWhiteSpace(licenseeName))
			{
				throw new ArgumentException($"{nameof(licenseeName)} cannot be null or whitespace", nameof(licenseeName));
			}

			this.licenseId = licenseId;
			this.licenseeName = licenseeName;
		}
		
		public LicenseManager(string licenseeName) : this (NewRandomLicenseId(), licenseeName)
		{
		}

		private void GenerateLicenseCode(IReadOnlyList<string> products)
		{
			if (products is null)
			{
				products = ProductAuthentication.AllProductCodes;
			}

			LicenseEntity licenseEntity = new LicenseEntity();
			licenseEntity.LicenseId = this.LicenseId;
			licenseEntity.LicenseeName = this.LicenseeName;
			licenseEntity.Metadata = "0120230914PSAX000005";
			licenseEntity.Hash = "TRIAL:-1635216578";
			licenseEntity.GracePeriodDays = 7;
			if (licenseEntity.Products.Count > 0)
			{
				licenseEntity.Products.Clear();
			}
			string expDate = expireDate.ToString("yyyy-MM-dd");
			foreach (var product in products)
			{
				licenseEntity.Products.Add(new ProductAuthentication()
				{
					Code = product,
					Extend = true,
					FallbackDate = expDate,
					PaidUpTo = expDate,
				});
			}
			this.licenseCode = JsonSerializer.Serialize(licenseEntity, SerializerOptions);
		}

		private void GenerateActivateCode(AsymmetricKeyParameter privateKey, PemObject certObject)
		{
			var publicKey = new X509Certificate(certObject.Content).GetPublicKey();
			var licenseCodeBytes = Encoding.UTF8.GetBytes(this.licenseCode);

			var privateKeySigner = new RsaDigestSigner(new Sha1Digest());
			privateKeySigner.Init(true, privateKey);
			privateKeySigner.BlockUpdate(licenseCodeBytes);
			byte[] signature = privateKeySigner.GenerateSignature();

			var publicKeySigner = new RsaDigestSigner(new Sha1Digest());
			publicKeySigner.Init(false, publicKey);
			publicKeySigner.BlockUpdate(licenseCodeBytes);
			if (!publicKeySigner.VerifySignature(signature)) { throw new ArgumentException("signature verify error"); }

			this.activationCode = this.licenseId
				+ "-"
				+ Convert.ToBase64String(licenseCodeBytes)
				+ "-"
				+ Convert.ToBase64String(signature)
				+ "-"
				+ Convert.ToBase64String(certObject.Content);
		}

		public void ActivateAll(AsymmetricKeyParameter privateKey, PemObject certObject)
		{
			GenerateLicenseCode(ProductAuthentication.AllProductCodes);
			GenerateActivateCode(privateKey, certObject);
		}
	}
}
