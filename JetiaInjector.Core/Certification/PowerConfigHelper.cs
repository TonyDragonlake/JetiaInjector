using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Text;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.IO.Pem;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JetiaInjector.Core.Certification
{
	public class PowerConfigHelper
	{
		private const string JET_PRIVATE_KEY
			= "860106576952879101192782278876319243486072481962999610484027161162448933268423045647258145695082284265933019120714643752088997312766689988016808929265129401027490891810902278465065056686129972085119605237470899952751915070244375173428976413406363879128531449407795115913715863867259163957682164040613505040314747660800424242248055421184038777878268502955477482203711835548014501087778959157112423823275878824729132393281517778742463067583320091009916141454657614089600126948087954465055321987012989937065785013284988096504657892738536613208311013047138019418152103262155848541574327484510025594166239784429845180875774012229784878903603491426732347994359380330103328705981064044872334790365894924494923595382470094461546336020961505275530597716457288511366082299255537762891238136381924520749228412559219346777184174219999640906007205260040707839706131662149325151230558316068068139406816080119906833578907759960298749494098180107991752250725928647349597506532778539709852254478061194098069801549845163358315116260915270480057699929968468068015735162890213859113563672040630687357054902747438421559817252127187138838514773245413540030800888215961904267348727206110582505606182944023582459006406137831940959195566364811905585377246353";

		private static void BuildConfigCore(X509Certificate x509Certificate, out string configContent)
		{
			if (x509Certificate is null) { throw new ArgumentNullException(nameof(x509Certificate)); }
			var publicKey = x509Certificate.GetPublicKey() as RsaKeyParameters;
			if (publicKey is null) { throw new InvalidCastException("X509Certificate.PublicKey is not using RsaKeyParameters"); }
			var bytes = EncodeSignature(x509Certificate.GetTbsCertificate(), publicKey.Modulus.BitLength);
			StringBuilder builder = new StringBuilder("[Result]\nEQUAL,");
			builder.AppendJoin(',',
				new BigInteger(x509Certificate.GetSignature()).ToString(10),
				publicKey.Exponent.ToString(10),
				$"{JET_PRIVATE_KEY}->{new BigInteger(1, bytes).ToString(10)}"
				);
			configContent = builder.ToString();
		}

		private static byte[] EncodeSignature(byte[] values, int keySize)
		{
			return DoRsaPaddingBlockType1(
				DoSHA256Signature(
					DoSHA256Digest(values)),
				keySize);
		}

		private static byte[] DoSHA256Digest(byte[] bytes)
		{
			Sha256Digest sha256Digest = new Sha256Digest();
			sha256Digest.BlockUpdate(bytes);
			var result = new byte[sha256Digest.GetDigestSize()];
			sha256Digest.DoFinal(result);
			return result;
		}

		private static byte[] DoSHA256Signature(byte[] bytes)
		{
			using (var memoryStream = new MemoryStream())
			{
				var outputStream = Asn1OutputStream.Create(memoryStream, Asn1Encodable.Der);
				var derSeq = new DerSequence(
					new AlgorithmIdentifier(
						new DerObjectIdentifier("2.16.840.1.101.3.4.2.1"),
						DerNull.Instance),
					new DerOctetString(bytes));
				outputStream.WriteObject(derSeq);
				return memoryStream.ToArray();
			}
		}

		private static byte[] DoRsaPaddingBlockType1(byte[] bytes, int keySize)
		{
			int paddedSize = keySize + 7 >> 3;
			if (paddedSize < 64)
			{
				throw new InvalidKeyException("Padded size must be at least 64");
			}
			int maxDataSize = paddedSize - 11;
			var len = bytes.Length;
			if (len > maxDataSize)
			{
				throw new ArgumentException("BadPadding");
			}
			byte[] padded = new byte[paddedSize];
			Array.Copy(bytes, 0, padded, paddedSize - len, len);
			int psSize = paddedSize - 3 - len;
			int k = 0;
			padded[k++] = 0;
			padded[k++] = 1;
			while (psSize-- > 0)
			{
				padded[k++] = 0xff;
			}
			return padded;
		}

		public static string BuildConfig(X509Certificate x509Certificate)
		{
			BuildConfigCore(x509Certificate, out string configContent);
			return configContent;
		}

		public static string BuildConfig(PemObject pemObject)
		{
			X509Certificate x509Certificate = new X509Certificate(pemObject.Content);
			BuildConfigCore(x509Certificate, out string configContent);
			return configContent;
		}

		public static void BuildConfigToFile(PemObject pemObject, string filePath)
		{
			X509Certificate x509Certificate = new X509Certificate(pemObject.Content);
			BuildConfigCore(x509Certificate, out string configContent);
			using (var fileWriter = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
			{
				fileWriter.Seek(0, SeekOrigin.Begin);
				fileWriter.SetLength(0);
				using (var streamWriter = new StreamWriter(fileWriter))
				{
					streamWriter.Write(configContent);
				}
			}
		}
	}
}
