namespace GuydoDB.CLI;

public static class ByteExtensions
{
    public static unsafe bool UnsafeEquals(this byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        fixed (byte* ptr1 = a, ptr2 = b)
        {
            var p1 = ptr1;
            var p2 = ptr2;

            for (var i = 0; i < b.Length; i++)
            {
                if (*p1 != *p2)
                {
                    return false;
                }

                p1++;
                p2++;
            }
        }

        return true;
    }
}