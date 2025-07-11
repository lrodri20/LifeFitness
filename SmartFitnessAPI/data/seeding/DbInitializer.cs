using Microsoft.EntityFrameworkCore;
using SmartFitnessApi.Data;
using SmartFitnessApi.Models;
using SmartFitnessApi.Models.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartFitnessApi.Services;

namespace SmartFitnessApi.Data.Seeding
{
    public static class DbInitializer
    {
        public static IAuthenticationService AuthenticationService { get; set; }
        public static async Task SeedAsync(SmartFitnessDbContext context, IAuthenticationService authService)
        {
            AuthenticationService = authService;
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();
            // Check if we already have data
            var userCount = await context.Users.CountAsync();
            if (userCount > 0)
            {
                return; // Database has been seeded
            }

            // Start transaction
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var profiles = await context.Profiles.ToListAsync();

                // Seed Activities first
                var activities = GetActivities();
                await context.Activities.AddRangeAsync(activities);
                await context.SaveChangesAsync();

                // Seed Users and Profiles
                var usersWithProfiles = GetUsersWithProfiles();
                await context.Users.AddRangeAsync(usersWithProfiles);
                await context.SaveChangesAsync();

                // Get the saved profiles for relationship setup
                //var profiles = await context.Profiles.ToListAsync();

                // Seed Profile Activities
                await SeedProfileActivities(context, profiles, activities.ToList());

                // Seed Profile Goals
                await SeedProfileGoals(context, profiles);

                // Seed Profile Schedules
                await SeedProfileSchedules(context, profiles);

                // Seed Matching Preferences
                await SeedMatchingPreferences(context, profiles);

                // Seed some sample matches
                await SeedMatches(context, profiles);
                // Seed Match Requests
                await SeedMatchRequests(context, profiles);

                // -- Messages for each match
                await SeedMessages(context);

                // -- User Refresh Tokens
                await SeedRefreshTokens(context);

                // -- Password Reset Tokens
                await SeedPasswordResetTokens(context);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static IEnumerable<Activity> GetActivities()
        {
            return new List<Activity>
            {
                // Cardio
                new Activity { Name = "Running", Description = "Outdoor or treadmill running", Category = ActivityCategory.Cardio, IconUrl = "🏃" },
                new Activity { Name = "Cycling", Description = "Road cycling or stationary bike", Category = ActivityCategory.Cardio, IconUrl = "🚴" },
                new Activity { Name = "Swimming", Description = "Pool or open water swimming", Category = ActivityCategory.Sports, IconUrl = "🏊" },
                new Activity { Name = "Walking", Description = "Brisk walking or hiking", Category = ActivityCategory.Cardio, IconUrl = "🚶" },
                new Activity { Name = "HIIT", Description = "High-intensity interval training", Category = ActivityCategory.Cardio, IconUrl = "💪" },
                
                // Strength
                new Activity { Name = "Weight Training", Description = "Free weights and machines", Category = ActivityCategory.Strength, IconUrl = "🏋️" },
                new Activity { Name = "CrossFit", Description = "Functional fitness training", Category = ActivityCategory.Strength, IconUrl = "💪" },
                new Activity { Name = "Bodyweight Training", Description = "Calisthenics and bodyweight exercises", Category = ActivityCategory.Strength, IconUrl = "🤸" },
                
                // Flexibility
                new Activity { Name = "Yoga", Description = "Various yoga styles", Category = ActivityCategory.Flexibility, IconUrl = "🧘" },
                new Activity { Name = "Pilates", Description = "Core strengthening and flexibility", Category = ActivityCategory.Flexibility, IconUrl = "🤸" },
                new Activity { Name = "Stretching", Description = "Flexibility and mobility work", Category = ActivityCategory.Flexibility, IconUrl = "🙆" },
                
                // Sports
                new Activity { Name = "Tennis", Description = "Singles or doubles tennis", Category = ActivityCategory.Sports, IconUrl = "🎾" },
                new Activity { Name = "Basketball", Description = "Pickup games or organized play", Category = ActivityCategory.Sports, IconUrl = "🏀" },
                new Activity { Name = "Soccer", Description = "Football/soccer games", Category = ActivityCategory.Sports, IconUrl = "⚽" },
                new Activity { Name = "Golf", Description = "18 holes or driving range", Category = ActivityCategory.Sports, IconUrl = "⛳" },
                new Activity { Name = "Rock Climbing", Description = "Indoor or outdoor climbing", Category = ActivityCategory.Sports, IconUrl = "🧗" },
                
                // Martial Arts
                new Activity { Name = "Boxing", Description = "Boxing training and sparring", Category = ActivityCategory.MartialArts, IconUrl = "🥊" },
                new Activity { Name = "MMA", Description = "Mixed martial arts training", Category = ActivityCategory.MartialArts, IconUrl = "🥋" },
                new Activity { Name = "Brazilian Jiu-Jitsu", Description = "BJJ training and rolling", Category = ActivityCategory.MartialArts, IconUrl = "🥋" },
                
                // Dance
                new Activity { Name = "Zumba", Description = "Dance fitness classes", Category = ActivityCategory.Dance, IconUrl = "💃" },
                new Activity { Name = "Dance", Description = "Various dance styles", Category = ActivityCategory.Dance, IconUrl = "🕺" },
                
                // Mind-Body
                new Activity { Name = "Meditation", Description = "Mindfulness and meditation", Category = ActivityCategory.MindBody, IconUrl = "🧘" },
                new Activity { Name = "Tai Chi", Description = "Moving meditation", Category = ActivityCategory.MindBody, IconUrl = "☯️" }
            };
        }

        private static IEnumerable<User> GetUsersWithProfiles()
        {
            var now = DateTime.UtcNow;
            var users = new List<User>();

            // Miami-based users with different fitness profiles
            var userData = new[]
            {
                new { Email = "sarah.johnson@email.com", UserName = "sarahjfit", FirstName = "Sarah", LastName = "Johnson",
                      Lat = 25.7617, Lng = -80.1918, Bio = "Marathon runner looking for training partners. Early morning runs are my therapy!",
                      FitnessLevel = FitnessLevel.Advanced, HasHomeGym = false },

                new { Email = "mike.rodriguez@email.com", UserName = "mikerod22", FirstName = "Mike", LastName = "Rodriguez",
                      Lat = 25.7743, Lng = -80.1937, Bio = "CrossFit enthusiast and weekend warrior. Let's crush some WODs together!",
                      FitnessLevel = FitnessLevel.Intermediate, HasHomeGym = true },

                new { Email = "emily.chen@email.com", UserName = "emilyc_yoga", FirstName = "Emily", LastName = "Chen",
                      Lat = 25.7553, Lng = -80.3747, Bio = "Yoga instructor and mindfulness advocate. Love beach yoga sessions at sunrise.",
                      FitnessLevel = FitnessLevel.Expert, HasHomeGym = false },

                new { Email = "david.smith@email.com", UserName = "davidlifts", FirstName = "David", LastName = "Smith",
                      Lat = 25.7907, Lng = -80.1300, Bio = "Powerlifter and personal trainer. Happy to help beginners with form and programming.",
                      FitnessLevel = FitnessLevel.Expert, HasHomeGym = true },

                new { Email = "lisa.thompson@email.com", UserName = "lisatruns", FirstName = "Lisa", LastName = "Thompson",
                      Lat = 25.6892, Lng = -80.3151, Bio = "New to fitness, looking for supportive workout buddies. Prefer outdoor activities!",
                      FitnessLevel = FitnessLevel.Beginner, HasHomeGym = false },

                new { Email = "alex.martinez@email.com", UserName = "alexmfit", FirstName = "Alex", LastName = "Martinez",
                      Lat = 25.8131, Lng = -80.1342, Bio = "Former athlete getting back in shape. Tennis, basketball, and gym sessions.",
                      FitnessLevel = FitnessLevel.Intermediate, HasHomeGym = false },

                new { Email = "jessica.wilson@email.com", UserName = "jessicaw", FirstName = "Jessica", LastName = "Wilson",
                      Lat = 25.7459, Lng = -80.2619, Bio = "Triathlete training for Ironman. Swimming, cycling, running - let's train!",
                      FitnessLevel = FitnessLevel.Advanced, HasHomeGym = true },

                new { Email = "ryan.taylor@email.com", UserName = "ryantaylor", FirstName = "Ryan", LastName = "Taylor",
                      Lat = 25.7781, Lng = -80.1874, Bio = "Boxing and MMA training. Always down for pad work or sparring sessions.",
                      FitnessLevel = FitnessLevel.Intermediate, HasHomeGym = false },

                new { Email = "maria.garcia@email.com", UserName = "mariagarcia", FirstName = "Maria", LastName = "Garcia",
                      Lat = 25.7287, Lng = -80.2544, Bio = "Zumba lover and dance fitness enthusiast. Let's make fitness fun!",
                      FitnessLevel = FitnessLevel.Intermediate, HasHomeGym = false },

                new { Email = "john.anderson@email.com", UserName = "johnanders", FirstName = "John", LastName = "Anderson",
                      Lat = 25.7989, Lng = -80.2089, Bio = "Rock climbing and outdoor adventures. Seeking climbing partners for weekends.",
                      FitnessLevel = FitnessLevel.Advanced, HasHomeGym = true },
                      new { Email = "olivia.brown@email.com",    UserName = "oliviab",      FirstName = "Olivia",  LastName = "Brown",
          Lat = 25.7840, Lng = -80.2100, Bio = "Pilates instructor who loves sculpting and core work. Always looking for a reformer buddy!",
          FitnessLevel = FitnessLevel.Intermediate, HasHomeGym = false },

    new { Email = "brandon.clark@email.com",   UserName = "brandonc",     FirstName = "Brandon", LastName = "Clark",
          Lat = 25.7700, Lng = -80.2000, Bio = "Soccer fanatic and weekend league player. Team drills and pick-up games welcome!",
          FitnessLevel = FitnessLevel.Intermediate, HasHomeGym = true },

    new { Email = "sophia.martinez@email.com", UserName = "sophiam",      FirstName = "Sophia",  LastName = "Martinez",
          Lat = 25.7600, Lng = -80.2200, Bio = "Dance fitness coach specializing in Zumba and cardio dance. Let's get moving!",
          FitnessLevel = FitnessLevel.Advanced,     HasHomeGym = false },

    new { Email = "ethan.wilson@email.com",    UserName = "ethanw",       FirstName = "Ethan",   LastName = "Wilson",
          Lat = 25.7850, Lng = -80.2300, Bio = "Nature hiker and trail runner. I’m in search of weekend adventure partners!",
          FitnessLevel = FitnessLevel.Beginner,     HasHomeGym = false },

    new { Email = "chloe.davis@email.com",     UserName = "chloed",       FirstName = "Chloe",   LastName = "Davis",
          Lat = 25.7900, Lng = -80.2400, Bio = "Bodybuilding enthusiast working on gaining mass. Spotters and gym buddies appreciated!",
          FitnessLevel = FitnessLevel.Advanced,     HasHomeGym = true }
            };

            var random = new Random();
            var baseDate = new DateTime(1980, 1, 1);

            foreach (var data in userData)
            {
                var user = new User
                {
                    Email = data.Email,
                    UserName = data.UserName,
                    PasswordHash = AuthenticationService.HashPassword("Password123!"), // Default password for all test users
                    IsEmailConfirmed = true,
                    CreatedAt = now.AddDays(-random.Next(30, 365)),
                    LastLoginAt = now.AddDays(-random.Next(0, 7)),
                    Profile = new Profile
                    {
                        FirstName = data.FirstName,
                        LastName = data.LastName,
                        DisplayName = $"{data.FirstName} {data.LastName[0]}.",
                        DateOfBirth = baseDate.AddYears(random.Next(10, 30)).AddDays(random.Next(0, 365)),
                        PhoneNumber = $"305-555-{random.Next(1000, 9999)}",
                        AddressLine1 = $"{random.Next(100, 9999)} {GetRandomStreet()} St",
                        City = "Miami",
                        State = "FL",
                        PostalCode = $"331{random.Next(10, 99)}",
                        Country = "USA",
                        Latitude = data.Lat + (random.NextDouble() - 0.5) * 0.01, // Small variation
                        Longitude = data.Lng + (random.NextDouble() - 0.5) * 0.01,
                        Bio = data.Bio,
                        FitnessLevel = data.FitnessLevel,
                        HasHomeGym = data.HasHomeGym,
                        CreatedAt = now.AddDays(-random.Next(30, 365)),
                        UpdatedAt = now.AddDays(-random.Next(0, 7))
                    }
                };

                users.Add(user);
            }

            return users;
        }

        private static string GetRandomStreet()
        {
            var streets = new[] { "Ocean", "Collins", "Washington", "Lincoln", "Alton", "Biscayne", "Coral", "Sunset", "Flagler", "Miracle" };
            return streets[new Random().Next(streets.Length)];
        }

        private static async Task SeedProfileActivities(SmartFitnessDbContext context, List<Profile> profiles, List<Activity> activities)
        {
            var profileActivities = new List<ProfileActivity>();
            var random = new Random();

            // Define activity preferences for each profile based on their bio
            var activityMap = new Dictionary<int, string[]>
            {
                { 0, new[] { "Running", "Cycling", "Swimming" } }, // Sarah - Marathon runner
                { 1, new[] { "CrossFit", "Weight Training", "HIIT" } }, // Mike - CrossFit
                { 2, new[] { "Yoga", "Pilates", "Meditation" } }, // Emily - Yoga instructor
                { 3, new[] { "Weight Training", "Bodyweight Training" } }, // David - Powerlifter
                { 4, new[] { "Walking", "Yoga", "Swimming" } }, // Lisa - Beginner
                { 5, new[] { "Tennis", "Basketball", "Weight Training" } }, // Alex - Former athlete
                { 6, new[] { "Swimming", "Cycling", "Running" } }, // Jessica - Triathlete
                { 7, new[] { "Boxing", "MMA", "Weight Training" } }, // Ryan - Boxing/MMA
                { 8, new[] { "Zumba", "Dance", "HIIT" } }, // Maria - Dance fitness
                { 9, new[] { "Rock Climbing", "Hiking", "Weight Training" } } // John - Rock climbing
            };

            for (int i = 0; i < profiles.Count && i < activityMap.Count; i++)
            {
                var profile = profiles[i];
                var preferredActivities = activityMap[i];
                var isPrimary = true;

                foreach (var activityName in preferredActivities)
                {
                    var activity = activities.FirstOrDefault(a => a.Name == activityName);
                    if (activity != null)
                    {
                        profileActivities.Add(new ProfileActivity
                        {
                            ProfileId = profile.Id,
                            ActivityId = activity.Id,
                            IsPrimary = isPrimary
                        });
                        isPrimary = false;
                    }
                }

                // Add 1-2 random secondary activities
                var additionalCount = random.Next(1, 3);
                var selectedActivities = preferredActivities.ToList();

                for (int j = 0; j < additionalCount; j++)
                {
                    var randomActivity = activities[random.Next(activities.Count)];
                    if (!selectedActivities.Contains(randomActivity.Name))
                    {
                        profileActivities.Add(new ProfileActivity
                        {
                            ProfileId = profile.Id,
                            ActivityId = randomActivity.Id,
                            IsPrimary = false
                        });
                        selectedActivities.Add(randomActivity.Name);
                    }
                }
            }

            await context.AddRangeAsync(profileActivities);
            await context.SaveChangesAsync();
        }

        private static async Task SeedProfileGoals(SmartFitnessDbContext context, List<Profile> profiles)
        {
            var profileGoals = new List<ProfileGoal>();
            var random = new Random();

            var goalMap = new Dictionary<int, FitnessGoal[]>
            {
                { 0, new[] { FitnessGoal.Endurance, FitnessGoal.SportPerformance } }, // Sarah
                { 1, new[] { FitnessGoal.MuscleGain, FitnessGoal.GeneralFitness } }, // Mike
                { 2, new[] { FitnessGoal.Flexibility, FitnessGoal.StressRelief } }, // Emily
                { 3, new[] { FitnessGoal.MuscleGain, FitnessGoal.SportPerformance } }, // David
                { 4, new[] { FitnessGoal.GeneralFitness, FitnessGoal.WeightLoss } }, // Lisa
                { 5, new[] { FitnessGoal.GeneralFitness, FitnessGoal.SportPerformance } }, // Alex
                { 6, new[] { FitnessGoal.Endurance, FitnessGoal.SportPerformance } }, // Jessica
                { 7, new[] { FitnessGoal.MuscleGain, FitnessGoal.SportPerformance } }, // Ryan
                { 8, new[] { FitnessGoal.WeightLoss, FitnessGoal.StressRelief } }, // Maria
                { 9, new[] { FitnessGoal.GeneralFitness, FitnessGoal.Endurance } } // John
            };

            for (int i = 0; i < profiles.Count && i < goalMap.Count; i++)
            {
                var profile = profiles[i];
                var goals = goalMap[i];
                var priority = 1;

                foreach (var goal in goals)
                {
                    profileGoals.Add(new ProfileGoal
                    {
                        ProfileId = profile.Id,
                        Goal = goal,
                        Priority = priority++
                    });
                }
            }

            await context.AddRangeAsync(profileGoals);
            await context.SaveChangesAsync();
        }

        private static async Task SeedProfileSchedules(SmartFitnessDbContext context, List<Profile> profiles)
        {
            var schedules = new List<ProfileSchedule>();
            var random = new Random();

            foreach (var profile in profiles)
            {
                // Weekday preferences
                var morningPerson = random.Next(2) == 0;
                var eveningPerson = !morningPerson || random.Next(3) == 0;

                for (int day = 1; day <= 5; day++) // Monday to Friday
                {
                    if (morningPerson)
                    {
                        schedules.Add(new ProfileSchedule
                        {
                            ProfileId = profile.Id,
                            DayOfWeek = (DayOfWeek)day,
                            TimeSlot = random.Next(2) == 0 ? TimeSlot.EarlyMorning : TimeSlot.Morning,
                            IsAvailable = true
                        });
                    }

                    if (eveningPerson)
                    {
                        schedules.Add(new ProfileSchedule
                        {
                            ProfileId = profile.Id,
                            DayOfWeek = (DayOfWeek)day,
                            TimeSlot = TimeSlot.Evening,
                            IsAvailable = true
                        });
                    }
                }

                // Weekend preferences - more flexible
                foreach (var day in new[] { DayOfWeek.Saturday, DayOfWeek.Sunday })
                {
                    var slots = new[] { TimeSlot.Morning, TimeSlot.MidMorning, TimeSlot.Afternoon };
                    var selectedSlots = slots.OrderBy(x => random.Next()).Take(random.Next(1, 3));

                    foreach (var slot in selectedSlots)
                    {
                        schedules.Add(new ProfileSchedule
                        {
                            ProfileId = profile.Id,
                            DayOfWeek = day,
                            TimeSlot = slot,
                            IsAvailable = true
                        });
                    }
                }
            }

            await context.AddRangeAsync(schedules);
            await context.SaveChangesAsync();
        }

        private static async Task SeedMatchingPreferences(SmartFitnessDbContext context, List<Profile> profiles)
        {
            var preferences = new List<MatchingPreference>();
            var random = new Random();

            foreach (var profile in profiles)
            {
                var ageRange = 10 + random.Next(10); // 10-20 year range
                var currentAge = DateTime.Today.Year - (profile.DateOfBirth?.Year ?? 1990);

                preferences.Add(new MatchingPreference
                {
                    ProfileId = profile.Id,
                    MaxDistanceMiles = random.Next(3, 15),
                    MinAge = Math.Max(18, currentAge - ageRange),
                    MaxAge = currentAge + ageRange,
                    GenderPreference = (GenderPreference)random.Next(3),
                    PreferSimilarFitnessLevel = random.Next(4) != 0, // 75% prefer similar
                    FitnessLevelTolerance = random.Next(1, 3),
                    PreferHomeGym = profile.HasHomeGym && random.Next(2) == 0,
                    PreferPublicGym = random.Next(4) != 0, // 75% prefer public gym
                    PreferOutdoor = random.Next(3) != 0, // 66% prefer outdoor
                    OpenToGroupWorkouts = random.Next(4) != 0, // 75% open to groups
                    MaxGroupSize = 2 + random.Next(2, 6)
                });
            }

            await context.AddRangeAsync(preferences);
            await context.SaveChangesAsync();
        }
        private static async Task SeedMatchRequests(
                    SmartFitnessDbContext context,
                    List<Profile> profiles)
        {
            var random = new Random();
            var now = DateTime.UtcNow;
            var requests = new List<MatchRequest>();

            // create a handful of accepted requests
            var pairs = new[] { (0, 6), (1, 3), (2, 8), (4, 2), (5, 7) };
            foreach (var (i, j) in pairs)
            {
                if (i < profiles.Count && j < profiles.Count)
                {
                    requests.Add(new MatchRequest
                    {
                        RequesterId = profiles[i].Id,
                        RequesteeId = profiles[j].Id,
                        Status = MatchStatus.Accepted,
                        CompatibilityScore = 70 + random.Next(30),
                        CreatedAt = now.AddDays(-random.Next(10, 30)),
                        RespondedAt = now.AddDays(-random.Next(1, 5)),
                        LastInteractionAt = now.AddDays(-random.Next(0, 2)),
                        InitialMessage = "Hey, looks like we share some interests—want to connect?",
                        SharedActivitiesJson = "[\"Running\",\"Cycling\"]"
                    });
                }
            }

            // plus a few pending
            for (int k = 0; k < 5; k++)
            {
                var a = random.Next(profiles.Count);
                var b = random.Next(profiles.Count);
                if (a == b ||
                    requests.Any(r => (r.RequesterId == profiles[a].Id && r.RequesteeId == profiles[b].Id)))
                    continue;

                requests.Add(new MatchRequest
                {
                    RequesterId = profiles[a].Id,
                    RequesteeId = profiles[b].Id,
                    Status = MatchStatus.Pending,
                    CompatibilityScore = 60 + random.Next(20),
                    CreatedAt = now.AddDays(-random.Next(0, 5)),
                    InitialMessage = "Hi! Looking for a workout partner—are you interested?",
                    SharedActivitiesJson = "[\"Weight Training\"]"
                });
            }

            await context.MatchRequests.AddRangeAsync(requests);
            await context.SaveChangesAsync();
        }


        private static async Task SeedMessages(SmartFitnessDbContext context)
        {
            var now = DateTime.UtcNow;
            var rnd = new Random();
            var allMatches = await context.Matches.ToListAsync();
            var messages = new List<Message>();

            foreach (var m in allMatches)
            {
                // 1–3 messages per match
                int count = rnd.Next(1, 4);
                for (int i = 0; i < count; i++)
                {
                    messages.Add(new Message
                    {
                        MatchId = m.Id,
                        SenderId = (i % 2 == 0 ? m.User1Id : m.User2Id),
                        Content = $"Sample message #{i + 1}",
                        SentAt = now.AddMinutes(-rnd.Next(0, 120)),
                        IsRead = rnd.Next(2) == 0
                    });
                }
            }

            await context.Messages.AddRangeAsync(messages);
            await context.SaveChangesAsync();
        }

        private static async Task SeedRefreshTokens(SmartFitnessDbContext context)
        {
            var now = DateTime.UtcNow;
            var users = await context.Users.ToListAsync();
            var tokens = users.Select(u => new UserRefreshToken
            {
                UserId = u.Id,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = now.AddDays(7),
                CreatedAt = now,
                RevokedAt = null,
                RevokedReason = null
            }).ToList();

            await context.UserRefreshTokens.AddRangeAsync(tokens);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPasswordResetTokens(SmartFitnessDbContext context)
        {
            var now = DateTime.UtcNow;
            var users = await context.Users.ToListAsync();
            var tokens = users.Select(u => new PasswordResetToken
            {
                UserId = u.Id,
                Token = Guid.NewGuid().ToString("N"),
                ExpiresAt = now.AddHours(1),
                Used = false
            }).ToList();

            await context.PasswordResetTokens.AddRangeAsync(tokens);
            await context.SaveChangesAsync();
        }
        private static async Task SeedMatches(
    SmartFitnessDbContext context,
    List<Profile> profiles)
        {
            var now = DateTime.UtcNow;
            var rnd = new Random();
            var requests = new List<MatchRequest>();

            // ─── 1) Build a few accepted match‐requests ───
            var acceptedPairs = new[]
            {
        (0, 6), // Sarah & Jessica
        (1, 3), // Mike & David
        (2, 8), // Emily & Maria
        (4, 2), // Lisa & Emily
        (5, 7), // Alex & Ryan
    };

            foreach (var (i, j) in acceptedPairs)
            {
                if (i < profiles.Count && j < profiles.Count)
                {
                    requests.Add(new MatchRequest
                    {
                        RequesterId = profiles[i].Id,
                        RequesteeId = profiles[j].Id,
                        Status = MatchStatus.Accepted,
                        CompatibilityScore = 75 + rnd.Next(20),
                        CreatedAt = now.AddDays(-rnd.Next(7, 30)),
                        RespondedAt = now.AddDays(-rnd.Next(1, 6)),
                        LastInteractionAt = now.AddDays(-rnd.Next(0, 3)),
                        InitialMessage = "Hey! I noticed we both enjoy similar activities. Want to train together?",
                        SharedActivitiesJson = "[\"Running\",\"Cycling\"]"
                    });
                }
            }

            // ─── 2) Build a few pending match‐requests ───
            for (int k = 0; k < 5; k++)
            {
                var a = profiles[rnd.Next(profiles.Count)];
                var b = profiles[rnd.Next(profiles.Count)];

                if (a.Id == b.Id ||
                    requests.Any(r =>
                       (r.RequesterId == a.Id && r.RequesteeId == b.Id) ||
                       (r.RequesterId == b.Id && r.RequesteeId == a.Id)))
                    continue;

                requests.Add(new MatchRequest
                {
                    RequesterId = a.Id,
                    RequesteeId = b.Id,
                    Status = MatchStatus.Pending,
                    CompatibilityScore = 60 + rnd.Next(30),
                    CreatedAt = now.AddDays(-rnd.Next(0, 3)),
                    InitialMessage = "Hi! Looking for a workout partner. Interested?",
                    SharedActivitiesJson = "[\"Weight Training\"]"
                });
            }

            // ─── 3) Persist all match‐requests ───
            await context.MatchRequests.AddRangeAsync(requests);
            await context.SaveChangesAsync();

            // ─── 4) Project accepted requests into Matches ───
            var matches = requests
                .Where(r => r.Status == MatchStatus.Accepted)
                .Select(r => new Match
                {
                    User1Id = r.RequesterId,
                    User2Id = r.RequesteeId,
                    CreatedAt = r.RespondedAt ?? r.CreatedAt
                })
                .ToList();

            // ─── 5) Persist actual Matches ───
            await context.Matches.AddRangeAsync(matches);
            await context.SaveChangesAsync();
        }

    }
}