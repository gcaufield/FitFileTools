#region Copyright
/////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2023 Garmin International, Inc.
// Licensed under the Flexible and Interoperable Data Transfer (FIT) Protocol License; you
// may not use this file except in compliance with the Flexible and Interoperable Data
// Transfer (FIT) Protocol License.
/////////////////////////////////////////////////////////////////////////////////////////////
// ****WARNING****  This file is auto-generated!  Do NOT edit this file.
// Profile Version = 21.105Release
// Tag = production/release/21.105.00-0-gdc65d24
/////////////////////////////////////////////////////////////////////////////////////////////

#endregion

namespace Dynastream.Fit
{
    /// <summary>
    /// Implements the profile WorkoutCapabilities type as a class
    /// </summary>
    public static class WorkoutCapabilities 
    {
        public const uint Interval = 0x00000001;
        public const uint Custom = 0x00000002;
        public const uint FitnessEquipment = 0x00000004;
        public const uint Firstbeat = 0x00000008;
        public const uint NewLeaf = 0x00000010;
        public const uint Tcx = 0x00000020; // For backwards compatibility. Watch should add missing id fields then clear flag.
        public const uint Speed = 0x00000080; // Speed source required for workout step.
        public const uint HeartRate = 0x00000100; // Heart rate source required for workout step.
        public const uint Distance = 0x00000200; // Distance source required for workout step.
        public const uint Cadence = 0x00000400; // Cadence source required for workout step.
        public const uint Power = 0x00000800; // Power source required for workout step.
        public const uint Grade = 0x00001000; // Grade source required for workout step.
        public const uint Resistance = 0x00002000; // Resistance source required for workout step.
        public const uint Protected = 0x00004000;
        public const uint Invalid = (uint)0x00000000;


    }
}

