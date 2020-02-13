using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace WebServices
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = new WebHostBuilder()

				.UseKestrel(options =>
				{
					options.Listen(IPAddress.Any, 80);
					options.Listen(IPAddress.Any, 443, listenOptions =>
					{
						listenOptions.UseHttps("cert.pfx", "0000");
					});
				})
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>()
				.ConfigureLogging(x =>
				{
					x.AddDebug();
					x.AddConsole();
				})
				.Build();

			host.Run();
		}
	}
}