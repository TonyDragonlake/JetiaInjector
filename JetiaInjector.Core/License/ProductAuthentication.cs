namespace JetiaInjector.Core.License
{
	public class ProductAuthentication
	{
		private static readonly string[] s_productCodes = {
			"II",// "Idea"
			"CL",// "Clion"
			"PS",// "PhpStorm"
			"GO",// "Goland"
			"PC",// "Pycharm"
			"WS",// "Webstorm"
			"RD",// "Rider"
			"DB",// "Datagrip"
			"RM",// "Rubymine"
			"AC",// "Appcode"
			"DS",// "Dataspell"
		};

		public static IReadOnlyList<string> AllProductCodes => s_productCodes;

		public string Code { get; set; } = string.Empty;
		public bool Extend { get; set; }
		public string FallbackDate { get; set; } = string.Empty;
		public string PaidUpTo { get; set; } = string.Empty;
	}
}
