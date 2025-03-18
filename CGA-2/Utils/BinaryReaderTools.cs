using System.Text;
using System.IO;

namespace CGA2.Utils
{
    public class BinaryReaderTools
    {
        public static string ReadLine(BinaryReader reader)
        {
            StringBuilder sb = new();

            while (reader.ReadChar() is char c && c != '\n')
                sb.Append(c);

            return sb.ToString();
        }
    }
}
