public class LevelCompleteMessage
{
	public readonly GameCampaignPhase CompletedPhase;

	public LevelCompleteMessage(GameCampaignPhase completedPhase)
	{
		this.CompletedPhase = completedPhase;
	}
}
