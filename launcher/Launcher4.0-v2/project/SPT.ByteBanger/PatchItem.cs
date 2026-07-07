/* PatchItem.cs
 * License: NCSA Open Source License
 * 
 * Copyright: SPT
 * AUTHORS:
 * Basuro
 */


using System.IO;

namespace SPT.ByteBanger
{
    public class PatchItem
    {
        public int Offset { get; set; }
        public byte[] Data { get; set; }

        public static PatchItem FromReader(BinaryReader br)
        {
            int offset = br.ReadInt32();
            int dataLength = br.ReadInt32();
            byte[] data = br.ReadBytes(dataLength);

            return new PatchItem
            {
                Offset = offset,
                Data = data
            };
        }

        internal void ToWriter(BinaryWriter bw)
        {
            // offset // 4B
            bw.Write(Offset);

            // length // 4B
            bw.Write(Data.Length);

            // data // xB
            bw.Write(Data, 0, Data.Length);
        }
    }
}
