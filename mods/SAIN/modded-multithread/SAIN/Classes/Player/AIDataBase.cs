using SAIN.SAINComponent.Classes.Info;

namespace SAIN.Components.PlayerComponentSpace.Classes;

public abstract class AIDataBase
{
    public AIDataBase(SAINAIData aidata)
    {
        AIData = aidata;
    }

    protected readonly SAINAIData AIData;
    protected GearInfo GearInfo
    {
        get { return AIData.PlayerComponent.Equipment.GearInfo; }
    }

    protected bool IsAI
    {
        get { return AIData.IsAI; }
    }
}
