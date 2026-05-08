using System.Collections.Generic;
using NLog.Config;
using UnityEngine;

[CreateAssetMenu]
public class LogConfigurationScriptableObject : ScriptableObject
{
	public List<LoggingRuleConfig> RuleConfigs = new List<LoggingRuleConfig>();

	public LoggingConfiguration MergeWith(LoggingConfiguration configuration)
	{
		if (RuleConfigs.Count != configuration.LoggingRules.Count)
		{
			Reset(configuration);
		}
		for (int i = 0; i < RuleConfigs.Count; i++)
		{
			RuleConfigs[i].Apply(configuration.LoggingRules[i]);
		}
		return configuration;
	}

	public void Reset(LoggingConfiguration configuration)
	{
		RuleConfigs.Clear();
		foreach (LoggingRule loggingRule in configuration.LoggingRules)
		{
			RuleConfigs.Add(new LoggingRuleConfig(loggingRule));
		}
	}
}
