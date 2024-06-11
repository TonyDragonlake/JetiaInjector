namespace JetiaInjector.Core.License
{
	public class LicenseEntity
	{
		public string AssigneeEmail { get; set; } = string.Empty;
		public string AssigneeName {  get; set; } = string.Empty;
		public bool AutoProlongated { get; set; }
		public bool CheckConcurrentUse { get; set; }
		public int GracePeriodDays { get; set; }
		public string Hash { get; set; } = string.Empty;
		public string LicenseId { get; set; } = string.Empty;
		public string LicenseRestriction { get; set; } = string.Empty;
		public string LicenseeName { get; set; } = string.Empty;
		public string Metadata { get; set; } = string.Empty;
		public List<ProductAuthentication> Products { get; private set; } = new List<ProductAuthentication>();

	}
}
