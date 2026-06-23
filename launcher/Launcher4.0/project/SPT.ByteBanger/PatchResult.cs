namespace SPT.ByteBanger
{
    public class PatchResult
    {
        public PatchResultType Result { get; }
        public byte[] PatchedData { get; }

        public PatchResult(PatchResultType result, byte[] patchedData)
        {
            Result = result;
            PatchedData = patchedData;
        }
    }

    public enum PatchResultType
    {
        Success,

        InputLengthMismatch,
        InputChecksumMismatch,

        AlreadyPatched,

        OutputChecksumMismatch
    }
}
