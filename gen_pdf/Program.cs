using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace gen_pdf
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			string file = args[0];
			byte[] data = File.ReadAllBytes(file);

			var assembly = Assembly.GetExecutingAssembly();
			var resourceName = "gen_pdf.300dpiBW-1-Halftone.txt";

			string template = string.Empty;
			using (Stream stream = assembly.GetManifestResourceStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
				template = reader.ReadToEnd();

			data = data.Skip(8).ToArray(); //header info

			byte[] seg0 = data.Take(16).ToArray();

			string globalhex = string.Concat(
				seg0.Select(b => b.ToString("X2")).ToArray()).ToLower() + ">";
			template = template.Replace("{GLOBAL_HEX}", globalhex);
			template = template.Replace("{GLOBAL_LENGTH}", globalhex.Length.ToString());

			data = data.Skip(16).ToArray();

			string hex = string.Concat(data.Select(b => b.ToString("X2")).ToArray()).ToLower() + ">";
			template = template.Replace("{JBIG_LENGTH}", hex.Length.ToString());
			template = template.Replace("{JBIG_IMAGE}", hex);

			int idx = template.IndexOf("10 0 obj");
			string sidx = idx.ToString();
			sidx = sidx.PadLeft(10, '0');
			template = template.Replace("{10_OBJ_OFFSET}", sidx);

			idx = template.IndexOf("11 0 obj");
			sidx = idx.ToString();
			sidx = sidx.PadLeft(10, '0');
			template = template.Replace("{11_OBJ_OFFSET}", sidx);

			idx = template.IndexOf("xref");
			template = template.Replace("{XREF_OFFSET}", idx.ToString());

			string name = Guid.NewGuid().ToString();
			File.WriteAllText("/tmp/" + name + ".pdf", template);
			Console.WriteLine("File written to /tmp/" + name + ".pdf");
		}

	}
}
