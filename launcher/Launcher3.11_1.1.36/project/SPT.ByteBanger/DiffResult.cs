namespace SPT.ByteBanger
{
    public class DiffResult
    {
        public DiffResultType Result { get; }
        public PatchInfo PatchInfo { get; }

        public DiffResult(DiffResultType result, PatchInfo patchInfo)
        {
            Result = result;
            PatchInfo = patchInfo;
        }
    }

    public enum DiffResultType
    {
        Success,
        OriginalFilePathInvalid,
        OriginalFileNotFound,
        OriginalFileReadFailed,

        PatchedFilePathInvalid,
        PatchedFileNotFound,
        PatchedFileReadFailed,

        FilesMatch
    }
}
