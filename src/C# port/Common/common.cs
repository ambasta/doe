using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Doe
{
    public class common
    {
        public static object deserialise(byte[] arr)
        {
            MemoryStream ms = new MemoryStream(arr);
            BinaryFormatter bf = new BinaryFormatter();
            ms.Position = 0;

            return bf.Deserialize(ms);
        }

        public static byte[] serialize(object o)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, o);

            return ms.ToArray();
        }

        public static string code(byte[] message, int x, int readBytes)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            return encoder.GetString(message, x, readBytes);
        }

        public static byte[] code(string s)
        {
            ASCIIEncoding encoder = new ASCIIEncoding();
            return encoder.GetBytes(s);
        }
    }
}
