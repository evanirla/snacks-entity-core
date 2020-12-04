using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Snacks.Entity.Core.Extensions
{
    static class ByteArrayExtensions
    {
        static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        public static T ToObject<T>(this byte[] bytes)
        {
            using MemoryStream stream = new MemoryStream(bytes);
            return (T)binaryFormatter.Deserialize(stream);
        }
    }
}
