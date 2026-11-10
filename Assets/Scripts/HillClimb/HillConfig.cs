using UnityEngine;

namespace PeakRover
{
    public enum HillState { Playing, GameOver }

    // All tuning values live here.
    public static class HillConfig
    {
        public const string SceneName = "HillClimb";

        // Terrain
        public const float SegmentLength = 1.5f;
        public const int SegmentsPerChunk = 20;
        public const float ChunkLength = SegmentLength * SegmentsPerChunk;
        public const float TerrainDepth = 6f;
        public const float GrassThickness = 0.6f;
        public const float TerrainBottomDrop = 25f;
        public const float MinWorldX = -30f;
        public const float GenerateAhead = 90f;
        public const float GenerateBehind = 40f;
        public const int ChunkPoolSize = 8;

        // Car
        public const float ChassisMass = 5f;
        public const float WheelMass = 1f;
        public const float WheelRadius = 0.45f;
        public const float WheelAnchorX = 0.95f;
        public const float WheelAnchorY = -0.35f;
        public const float SuspensionFrequency = 5f;
        public const float SuspensionDamping = 0.6f;
        public const float WheelFriction = 1.5f;
        public const float MotorSpeed = 2200f;
        public const float MotorTorque = 12f;
        public const float BrakeTorque = 18f;
        public const float ReverseSpeed = 900f;
        public const float ReverseTorque = 8f;
        public const float IdleTorque = 0.4f;
        public const float AirTorque = 12f;

        // Fuel
        public const float MaxFuel = 100f;
        public const float FuelDrainGas = 3.5f;
        public const float FuelDrainIdle = 0.6f;
        public const float FuelCanAmount = 45f;
        public const float StrandedTime = 1.5f;

        // Pickups
        public const int CoinPoolPrewarm = 40;
        public const int FuelPoolPrewarm = 4;
        public const float CoinPickupRadius = 1.1f;
        public const float FuelPickupRadius = 1.4f;
        public const float PickupBehind = 30f;
        public const float PickupAhead = 80f;

        // Camera
        public const float CameraFov = 45f;
        public const float CameraDistance = 18f;
        public const float CameraHeight = 2.2f;
        public const float CameraLead = 4f;
    }
}
