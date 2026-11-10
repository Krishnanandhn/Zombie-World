namespace Project.Core
{
    // Commands the UI can issue without knowing anything about gameplay internals.
    public interface IGameFlow
    {
        GameState State { get; }
        void StartRun();
        void Pause();
        void Resume();
        void Restart();
        void ReturnToMenu();
        void ChooseUpgrade(string upgradeId);
        void Quit();
    }
}
