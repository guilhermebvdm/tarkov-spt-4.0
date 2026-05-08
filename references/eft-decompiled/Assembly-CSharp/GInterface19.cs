using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EFT;

public interface GInterface19
{
	bool Destroyed { get; }

	Task<GClass1408> GetTemplates(GClass1408 returnInCaseOfCache);

	void AddNotificationToHandle(string notificationType, Action<string> action);

	Task<Dictionary<MongoID, GClass3672>> GetAllCustomizations();
}
