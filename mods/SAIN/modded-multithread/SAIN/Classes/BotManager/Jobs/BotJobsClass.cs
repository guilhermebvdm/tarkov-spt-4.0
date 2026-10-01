using SAIN.Components.BotControllerSpace.Classes.Raycasts;

namespace SAIN.Components;

public class BotJobsClass(BotManagerComponent botController) : BotManagerBase(botController)
{
    public DirectionDataJob PlayerDistancesJob { get; } = new DirectionDataJob(botController);
    public VisionRaycastJob VisionJob { get; } = new VisionRaycastJob(botController);
    public EnemyPlaceRaycastJob EnemyPlaceJob { get; } = new EnemyPlaceRaycastJob(botController);

    public void Dispose()
    {
        VisionJob.Dispose();
        EnemyPlaceJob.Dispose();
        PlayerDistancesJob.Dispose();
    }
}
