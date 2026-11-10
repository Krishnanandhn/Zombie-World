using UnityEngine;

namespace PeakRover
{
    public sealed class HillGame : MonoBehaviour
    {
        const string BestKey = "PeakRoverBest";
        const float RestartDelay = 0.8f;

        float _overTime;

        public CarController Car { get; private set; }
        public TerrainManager Terrain { get; private set; }
        public PickupManager Pickups { get; private set; }
        public HillInput Input { get; } = new HillInput();

        public HillState State { get; private set; }
        public int Coins { get; private set; }
        public int DistanceM { get; private set; }
        public int BestM { get; private set; }
        public string Reason { get; private set; } = string.Empty;
        public float RunStartTime { get; private set; }

        public event System.Action RunStarted;

        public void Init(CarController car, TerrainManager terrain, PickupManager pickups)
        {
            Car = car;
            Terrain = terrain;
            Pickups = pickups;
            BestM = PlayerPrefs.GetInt(BestKey, 0);
            Car.Init(this);
            StartRun();
        }

        public void AddCoin()
        {
            Coins++;
        }

        public void OnCrash(string reason)
        {
            if (State != HillState.Playing) return;

            State = HillState.GameOver;
            Reason = reason;
            _overTime = Time.time;
            if (DistanceM > BestM)
            {
                BestM = DistanceM;
                PlayerPrefs.SetInt(BestKey, BestM);
            }
        }

        void StartRun()
        {
            TerrainGen.Seed = Random.Range(0f, 100f);
            Terrain.ResetAll();
            Terrain.Refresh(0f);
            Pickups.ResetAll();
            Car.ResetCar(new Vector2(0f, TerrainGen.Height(0f) + 1.1f));
            Coins = 0;
            DistanceM = 0;
            Reason = string.Empty;
            RunStartTime = Time.time;
            State = HillState.Playing;
            RunStarted?.Invoke();
        }

        void Update()
        {
            if (State == HillState.Playing)
            {
                DistanceM = Mathf.Max(DistanceM, Mathf.FloorToInt(Car.Position.x));
            }
            else if (Time.time - _overTime > RestartDelay && Input.AnyPressed)
            {
                StartRun();
            }
        }
    }
}
