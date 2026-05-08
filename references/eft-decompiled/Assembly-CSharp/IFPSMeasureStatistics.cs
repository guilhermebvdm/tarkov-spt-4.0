public interface IFPSMeasureStatistics
{
	string Name { get; }

	double Average { get; }

	double LastValue { get; }

	void AddMeasurement(GStruct132 measurementData);
}
