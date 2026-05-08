using System;

public class GClass639
{
	public string CommitHash = "8ce8af8a9f1970f44154ebab58a485cc3c794c11";

	public DateTime CommitDate = DateTime.FromOADate(45931.4843981481);

	public string CommitSubject = "Merge branch 'bugfixes/EFT-000_patch_unity' into 'master'";

	public string CommitBranch = "master";

	public override string ToString()
	{
		return $"CommitHash: '{CommitHash}' CommitDate: '{CommitDate}' CommitBranch: '{CommitBranch}' CommitSubject: '{CommitSubject}'";
	}

	public void Serialize(EFTWriterClass writer)
	{
		GClass1290.WriteString(writer, CommitHash);
		GClass1290.WriteDouble(writer, CommitDate.ToOADate());
		GClass1290.WriteString(writer, CommitSubject);
		GClass1290.WriteString(writer, CommitBranch);
	}

	public static GClass639 Deserialize(EFTReaderClass reader)
	{
		return new GClass639
		{
			CommitHash = GClass1285.ReadString(reader),
			CommitDate = DateTime.FromOADate(GClass1285.ReadDouble(reader)),
			CommitSubject = GClass1285.ReadString(reader),
			CommitBranch = GClass1285.ReadString(reader)
		};
	}
}
