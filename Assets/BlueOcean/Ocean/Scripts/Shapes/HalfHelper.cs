using System;

//namespace Ocean
//{
    public static class HalfHelper
    {
        private static ushort[] baseTable = GenerateBaseTable();
        private static sbyte[] shiftTable = GenerateShiftTable();

        private static ushort[] GenerateBaseTable()
        {
            ushort[] baseTable = new ushort[512];
            for (int i = 0; i < 256; ++i)
            {
                sbyte e = (sbyte)(127 - i);
                if (e > 24)
                { // Very small numbers map to zero
                    baseTable[i | 0x000] = 0x0000;
                    baseTable[i | 0x100] = 0x8000;
                }
                else if (e > 14)
                { // Small numbers map to denorms
                    baseTable[i | 0x000] = (ushort)(0x0400 >> (18 + e));
                    baseTable[i | 0x100] = (ushort)((0x0400 >> (18 + e)) | 0x8000);
                }
                else if (e >= -15)
                { // Normal numbers just lose precision
                    baseTable[i | 0x000] = (ushort)((15 - e) << 10);
                    baseTable[i | 0x100] = (ushort)(((15 - e) << 10) | 0x8000);
                }
                else if (e > -128)
                { // Large numbers map to Infinity
                    baseTable[i | 0x000] = 0x7c00;
                    baseTable[i | 0x100] = 0xfc00;
                }
                else
                { // Infinity and NaN's stay Infinity and NaN's
                    baseTable[i | 0x000] = 0x7c00;
                    baseTable[i | 0x100] = 0xfc00;
                }
            }

            return baseTable;
        }

        private static sbyte[] GenerateShiftTable()
        {
            sbyte[] shiftTable = new sbyte[512];
            for (int i = 0; i < 256; ++i)
            {
                sbyte e = (sbyte)(127 - i);
                if (e > 24)
                { // Very small numbers map to zero
                    shiftTable[i | 0x000] = 24;
                    shiftTable[i | 0x100] = 24;
                }
                else if (e > 14)
                { // Small numbers map to denorms
                    shiftTable[i | 0x000] = (sbyte)(e - 1);
                    shiftTable[i | 0x100] = (sbyte)(e - 1);
                }
                else if (e >= -15)
                { // Normal numbers just lose precision
                    shiftTable[i | 0x000] = 13;
                    shiftTable[i | 0x100] = 13;
                }
                else if (e > -128)
                { // Large numbers map to Infinity
                    shiftTable[i | 0x000] = 24;
                    shiftTable[i | 0x100] = 24;
                }
                else
                { // Infinity and NaN's stay Infinity and NaN's
                    shiftTable[i | 0x000] = 13;
                    shiftTable[i | 0x100] = 13;
                }
            }

            return shiftTable;
        }

        public static void SingleToHalf(float single, byte[] data, uint offset)
        {
            uint value = BitConverter.ToUInt32(BitConverter.GetBytes(single), 0);
            ushort half = (ushort)(baseTable[(value >> 23) & 0x1ff] + ((value & 0x007fffff) >> shiftTable[value >> 23]));
            byte[] halfBytes = BitConverter.GetBytes(half);
            data[offset + 0] = halfBytes[BitConverter.IsLittleEndian ? 0 : 1];
            data[offset + 1] = halfBytes[BitConverter.IsLittleEndian ? 1 : 0];
        }

        public static void SingleToBytes(float single, byte[] data, uint offset)
        {
            byte[] floatBytes = BitConverter.GetBytes(single);
            data[offset + 0] = floatBytes[BitConverter.IsLittleEndian ? 0 : 3];
            data[offset + 1] = floatBytes[BitConverter.IsLittleEndian ? 1 : 2];
            data[offset + 2] = floatBytes[BitConverter.IsLittleEndian ? 2 : 1];
            data[offset + 3] = floatBytes[BitConverter.IsLittleEndian ? 3 : 0];
        }

        public static uint Reverse(uint x)
        {
            uint y = 0;
            for (int i = 0; i < 32; ++i)
            {
                y <<= 1;
                y |= (x & 1);
                x >>= 1;
            }
            return y;
        }
}
//}