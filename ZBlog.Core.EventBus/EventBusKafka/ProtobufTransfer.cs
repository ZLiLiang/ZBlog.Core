using ProtoBuf;

namespace ZBlog.Core.EventBus.EventBusKafka
{
    public class ProtobufTransfer
    {
        /// <summary>
        /// Protobuf 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Deserialize<T>(ReadOnlySpan<byte> data)
        {
            Stream stream = new MemoryStream(data.ToArray());
            var info = Serializer.Deserialize<T>(stream);

            return info;
        }

        /// <summary>
        /// 通过Protobuf 转字节
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(T data)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, data);

                return stream.ToArray();
            }
        }
    }
}
