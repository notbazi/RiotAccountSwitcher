using System.ComponentModel;
using System.Runtime.InteropServices;

namespace RiotSwitcherAccount;

internal static class WindowsDpapi
{
    private const int CRYPTPROTECT_UI_FORBIDDEN = 0x01;
    [StructLayout(LayoutKind.Sequential)] private struct Blob { public int Length; public IntPtr Data; }

    [DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CryptProtectData(ref Blob input, string? desc, IntPtr optionalEntropy, IntPtr reserved, IntPtr prompt, int flags, out Blob output);

    [DllImport("crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CryptUnprotectData(ref Blob input, out IntPtr description, IntPtr optionalEntropy, IntPtr reserved, IntPtr prompt, int flags, out Blob output);

    [DllImport("kernel32.dll", SetLastError = true)] private static extern IntPtr LocalFree(IntPtr memory);

    public static byte[] Encrypt(byte[] plaintext) => Apply(plaintext, false);
    public static byte[] Decrypt(byte[] ciphertext) => Apply(ciphertext, true);

    private static byte[] Apply(byte[] data, bool decrypt)
    {
        if (data.Length == 0) throw new ArgumentException("No se puede cifrar o descifrar un archivo vacío.");
        IntPtr pinned = Marshal.AllocHGlobal(data.Length);
        Blob input = new() { Length = data.Length, Data = pinned };
        Blob output = default;
        IntPtr description = IntPtr.Zero;
        try
        {
            Marshal.Copy(data, 0, pinned, data.Length);
            bool ok = decrypt
                ? CryptUnprotectData(ref input, out description, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CRYPTPROTECT_UI_FORBIDDEN, out output)
                : CryptProtectData(ref input, "RiotSwitcherAccount local session", IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, CRYPTPROTECT_UI_FORBIDDEN, out output);
            if (!ok) throw new Win32Exception(Marshal.GetLastWin32Error(), "No se pudo usar DPAPI de Windows.");
            var result = new byte[output.Length];
            Marshal.Copy(output.Data, result, 0, output.Length);
            return result;
        }
        finally
        {
            if (description != IntPtr.Zero) LocalFree(description);
            if (output.Data != IntPtr.Zero) LocalFree(output.Data);
            Marshal.FreeHGlobal(pinned);
        }
    }
}
