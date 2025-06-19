namespace SmartFitnessApi.Models.enums
{
    public enum FitnessLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3,
        Expert = 4
    }

    public enum ActivityCategory
    {
        Cardio,
        Strength,
        Flexibility,
        Sports,
        Outdoor,
        MindBody,
        MartialArts,
        Dance,
        Other
    }

    public enum FitnessGoal
    {
        WeightLoss,
        MuscleGain,
        Endurance,
        Flexibility,
        GeneralFitness,
        SportPerformance,
        Rehabilitation,
        StressRelief
    }

    public enum TimeSlot
    {
        EarlyMorning,   // 5-7 AM
        Morning,        // 7-9 AM
        MidMorning,     // 9-11 AM
        Lunch,          // 11 AM-1 PM
        Afternoon,      // 1-4 PM
        Evening,        // 4-7 PM
        Night           // 7-10 PM
    }

    public enum GenderPreference
    {
        Any,
        Same,
        Different
    }

    public enum MatchStatus
    {
        Pending,
        Accepted,
        Rejected,
        Expired,
        Blocked
    }
}