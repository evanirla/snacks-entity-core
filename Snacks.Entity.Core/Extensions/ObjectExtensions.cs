using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Snacks.Entity.Core.Extensions
{
    static class ObjectExtensions
    {
        static readonly BinaryFormatter binaryFormatter = new BinaryFormatter();

        public static byte[] ToByteArray(this object @object)
        {
            using MemoryStream memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, @object);

            return memoryStream.ToArray();
        }
    }
}
