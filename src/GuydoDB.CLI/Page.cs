namespace GuydoDB.CLI;

public readonly struct Page(byte[] data)
{
    private const int ItemIdSize = 4;
    public byte[] Data => data;

    public void Add(byte[] tuple)
    {
        var lower = BitConverter.ToUInt16(data, 0);
        var upper = BitConverter.ToUInt16(data, 2);

        var length = tuple.Length;
        var offset = upper - length;

        Buffer.BlockCopy(BitConverter.GetBytes(offset), 0, data, lower, sizeof(ushort));
        Buffer.BlockCopy(BitConverter.GetBytes(length), 0, data, lower + 2, sizeof(ushort));

        lower += ItemIdSize;
        upper = (ushort)offset;

        Buffer.BlockCopy(BitConverter.GetBytes(lower), 0, data, 0, sizeof(ushort));
        Buffer.BlockCopy(BitConverter.GetBytes(upper), 0, data, 2, sizeof(ushort));
        Buffer.BlockCopy(tuple, 0, data, offset, length);
    }

    public void ForEach(Action<byte[]> action)
    {
        var lower = BitConverter.ToUInt16(data, 0);
        var cursor = ItemIdSize;

        while (cursor < lower)
        {
            var offset = BitConverter.ToUInt16(data, cursor);
            var length = BitConverter.ToUInt16(data, cursor + 2);

            var tuple = data[offset..(offset + length)];
            action(tuple);
            cursor += ItemIdSize;
        }
    }

    public static Page Create()
    {
        const int lower = 4;
        const int upper = 8192;

        var data = new byte[upper];

        Buffer.BlockCopy(BitConverter.GetBytes(lower), 0, data, 0, sizeof(ushort));
        Buffer.BlockCopy(BitConverter.GetBytes(upper), 0, data, 2, sizeof(ushort));

        return new(data);
    }
}