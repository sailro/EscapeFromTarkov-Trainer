using System.Globalization;
using System.IO;
using System.Text;

namespace Installer;

internal sealed class UTF8StringWriter(StringBuilder stringBuilder) : StringWriter(stringBuilder, CultureInfo.InvariantCulture)
{
	public override Encoding Encoding => new UTF8Encoding();
}
