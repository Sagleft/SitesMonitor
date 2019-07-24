using System;

namespace SitesMonitor.Containers
{
	public class SiteData
	{
		public string url = "http://example.com";
		public string name = "Unknown";
	}
	
	public class SitesContainer
	{
		public SiteData[] sites;
	}
}
