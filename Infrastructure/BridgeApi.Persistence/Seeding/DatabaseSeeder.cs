using Bogus;
using BridgeApi.Domain.Entities;
using BridgeApi.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BridgeApi.Persistence.Seeding;

public static class DatabaseSeeder
{
    private const string DefaultPassword = "Bridge123!";
    private const int BoguseSeed = 12345;

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        if (await context.UserProfiles.AnyAsync())
        {
            logger.LogInformation("Seed data already exists, skipping");
            return;
        }

        logger.LogInformation("Seeding development data...");

        var users = await SeedUsersAsync(userManager);
        var intents = await SeedIntentsAsync(context);
        await SeedUserProfilesAsync(context, users);
        await SeedUserIntentsAsync(context, users, intents);
        var connections = await SeedConnectionsAsync(context, users, intents);
        await SeedMessagesAsync(context, users, connections);
        var posts = await SeedPostsAsync(context, users);
        await SeedPostLikesAndCommentsAsync(context, users, posts);
        await SeedFollowsAsync(context, users);

        logger.LogInformation("Seed data created successfully");
    }

    public static async Task ClearAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        logger.LogInformation("Clearing seed data...");

        await context.Follows.ExecuteDeleteAsync();
        await context.PostComments.ExecuteDeleteAsync();
        await context.PostLikes.ExecuteDeleteAsync();
        await context.Posts.ExecuteDeleteAsync();
        await context.Messages.ExecuteDeleteAsync();
        await context.Connections.ExecuteDeleteAsync();
        await context.UserIntents.ExecuteDeleteAsync();
        await context.UserProfiles.ExecuteDeleteAsync();
        await context.Intents.ExecuteDeleteAsync();

        var users = await userManager.Users.ToListAsync();
        foreach (var user in users)
            await userManager.DeleteAsync(user);

        logger.LogInformation("Seed data cleared successfully");
    }

    private static async Task<List<AppUser>> SeedUsersAsync(UserManager<AppUser> userManager)
    {
        var userDefs = new[]
        {
            ("founder1", "founder1@bridge.dev", "Founder"),
            ("founder2", "founder2@bridge.dev", "Founder"),
            ("founder3", "founder3@bridge.dev", "Founder"),
            ("investor1", "investor1@bridge.dev", "Investor"),
            ("investor2", "investor2@bridge.dev", "Investor"),
            ("investor3", "investor3@bridge.dev", "Investor"),
            ("talent1", "talent1@bridge.dev", "Talent"),
            ("talent2", "talent2@bridge.dev", "Talent"),
            ("talent3", "talent3@bridge.dev", "Talent"),
            ("admin1", "admin1@bridge.dev", "Admin"),
        };

        var users = new List<AppUser>();

        foreach (var (username, email, role) in userDefs)
        {
            var user = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = username,
                Email = email,
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(user, DefaultPassword);
            await userManager.AddToRoleAsync(user, role);
            users.Add(user);
        }

        return users;
    }

    private static async Task<List<Intent>> SeedIntentsAsync(ApplicationDbContext context)
    {
        var intents = new List<Intent>
        {
            new() { Title = "Actively Hiring", Description = "Looking for talented people to join the team" },
            new() { Title = "Looking for Investment", Description = "Seeking funding for the venture" },
            new() { Title = "Open to Collaborate", Description = "Open to partnership and collaboration opportunities" },
            new() { Title = "Raising Fund", Description = "Currently in a fundraising round" },
            new() { Title = "Seeking Co-Founder", Description = "Looking for a co-founder to build together" },
            new() { Title = "Mentoring", Description = "Available to mentor early-stage founders and talents" },
        };

        context.Intents.AddRange(intents);
        await context.SaveChangesAsync();
        return intents;
    }

    private static async Task SeedUserProfilesAsync(ApplicationDbContext context, List<AppUser> users)
    {
        Randomizer.Seed = new Random(BoguseSeed);

        var titles = new[]
        {
            "CEO & Founder", "CTO", "Angel Investor", "Full-Stack Developer",
            "Product Manager", "VP of Engineering", "Managing Partner",
            "Software Engineer", "UX Designer", "Growth Lead"
        };

        var locations = new[]
        {
            "Istanbul, Turkey", "San Francisco, USA", "London, UK",
            "Berlin, Germany", "Dubai, UAE", "Singapore",
            "Amsterdam, Netherlands", "Tel Aviv, Israel", "Ankara, Turkey", "New York, USA"
        };

        var faker = new Faker("en");

        var profiles = users.Select((user, i) => new UserProfile
        {
            UserId = user.Id,
            Name = faker.Name.FirstName(),
            Surname = faker.Name.LastName(),
            Title = titles[i],
            Bio = faker.Lorem.Paragraph(),
            Location = locations[i],
            PhoneNumber = faker.Phone.PhoneNumber("+90 5## ### ## ##"),
            LinkedInUrl = $"https://linkedin.com/in/{user.UserName}",
            GitHubUrl = i < 7 ? $"https://github.com/{user.UserName}" : null,
            WebsiteUrl = i < 5 ? faker.Internet.Url() : null,
        }).ToList();

        context.UserProfiles.AddRange(profiles);
        await context.SaveChangesAsync();
    }

    private static async Task SeedUserIntentsAsync(ApplicationDbContext context, List<AppUser> users, List<Intent> intents)
    {
        // Founders: Actively Hiring, Seeking Co-Founder, Raising Fund
        // Investors: Looking for Investment, Mentoring
        // Talents: Open to Collaborate
        var userIntents = new List<UserIntent>
        {
            // Founders
            new() { UserId = users[0].Id, IntentId = intents[0].Id }, // founder1 → Actively Hiring
            new() { UserId = users[0].Id, IntentId = intents[4].Id }, // founder1 → Seeking Co-Founder
            new() { UserId = users[1].Id, IntentId = intents[3].Id }, // founder2 → Raising Fund
            new() { UserId = users[1].Id, IntentId = intents[0].Id }, // founder2 → Actively Hiring
            new() { UserId = users[2].Id, IntentId = intents[4].Id }, // founder3 → Seeking Co-Founder
            new() { UserId = users[2].Id, IntentId = intents[2].Id }, // founder3 → Open to Collaborate
            // Investors
            new() { UserId = users[3].Id, IntentId = intents[1].Id }, // investor1 → Looking for Investment
            new() { UserId = users[3].Id, IntentId = intents[5].Id }, // investor1 → Mentoring
            new() { UserId = users[4].Id, IntentId = intents[1].Id }, // investor2 → Looking for Investment
            new() { UserId = users[5].Id, IntentId = intents[5].Id }, // investor3 → Mentoring
            new() { UserId = users[5].Id, IntentId = intents[1].Id }, // investor3 → Looking for Investment
            // Talents
            new() { UserId = users[6].Id, IntentId = intents[2].Id }, // talent1 → Open to Collaborate
            new() { UserId = users[7].Id, IntentId = intents[2].Id }, // talent2 → Open to Collaborate
            new() { UserId = users[7].Id, IntentId = intents[0].Id }, // talent2 → Actively Hiring
            new() { UserId = users[8].Id, IntentId = intents[2].Id }, // talent3 → Open to Collaborate
        };

        context.UserIntents.AddRange(userIntents);
        await context.SaveChangesAsync();
    }

    private static async Task<List<Connection>> SeedConnectionsAsync(
        ApplicationDbContext context, List<AppUser> users, List<Intent> intents)
    {
        var connections = new List<Connection>
        {
            // Accepted connections (messages can be sent)
            new() { SenderId = users[0].Id, ReceiverId = users[3].Id, IntentId = intents[1].Id, Note = "Would love to discuss investment opportunities", Status = 1 },
            new() { SenderId = users[1].Id, ReceiverId = users[4].Id, IntentId = intents[3].Id, Note = "Currently raising our Series A", Status = 1 },
            new() { SenderId = users[6].Id, ReceiverId = users[0].Id, IntentId = intents[0].Id, Note = "Interested in the open position", Status = 1 },
            new() { SenderId = users[2].Id, ReceiverId = users[7].Id, IntentId = intents[2].Id, Note = "Let's build something together", Status = 1 },
            // Pending
            new() { SenderId = users[8].Id, ReceiverId = users[1].Id, IntentId = intents[0].Id, Note = "Saw your hiring post", Status = 0 },
            new() { SenderId = users[5].Id, ReceiverId = users[2].Id, IntentId = intents[5].Id, Note = "Happy to mentor", Status = 0 },
            // Rejected
            new() { SenderId = users[3].Id, ReceiverId = users[8].Id, IntentId = intents[1].Id, Note = "Looking for backend devs", Status = 2 },
            new() { SenderId = users[4].Id, ReceiverId = users[0].Id, IntentId = intents[1].Id, Note = "Not the right fit at this time", Status = 2 },
        };

        context.Connections.AddRange(connections);
        await context.SaveChangesAsync();
        return connections;
    }

    private static async Task SeedMessagesAsync(
        ApplicationDbContext context, List<AppUser> users, List<Connection> connections)
    {
        Randomizer.Seed = new Random(BoguseSeed + 1);
        var faker = new Faker("en");

        var acceptedConnections = connections.Where(c => c.Status == 1).ToList();
        var messages = new List<Message>();

        foreach (var conn in acceptedConnections)
        {
            var participants = new[] { conn.SenderId, conn.ReceiverId };

            // 4-6 messages per accepted connection
            var count = faker.Random.Int(4, 6);
            for (var i = 0; i < count; i++)
            {
                messages.Add(new Message
                {
                    ConnectionId = conn.Id,
                    SenderId = participants[i % 2],
                    Content = faker.Lorem.Sentence(),
                    IsRead = i < count - 1, // last message unread
                });
            }
        }

        context.Messages.AddRange(messages);
        await context.SaveChangesAsync();
    }

    private static async Task<List<Post>> SeedPostsAsync(ApplicationDbContext context, List<AppUser> users)
    {
        Randomizer.Seed = new Random(BoguseSeed + 2);
        var faker = new Faker("en");

        var postContents = new[]
        {
            "Excited to announce we just closed our seed round! Looking for talented engineers to join us.",
            "The startup ecosystem in Istanbul is booming. Incredible energy at today's meetup.",
            "Just shipped a new feature that reduces onboarding time by 40%. Small wins matter.",
            "Looking for a technical co-founder for an AI-powered HR platform. DM me!",
            "Three lessons I learned from failing my first startup: 1) Talk to users first 2) Ship fast 3) Stay lean",
            "We're hiring! Full-stack developers, product designers, and growth marketers. Remote-first.",
            "Had an amazing mentoring session today. Giving back to the community is incredibly rewarding.",
            "Hot take: Most startups fail not because of bad ideas, but because of bad execution.",
            "Just published my thoughts on intent-based networking. Link in comments.",
            "Proud to share that our portfolio company just reached 1M users!",
            "The future of professional networking is intent-based. Know what you want, find who can help.",
            "Great panel discussion on venture capital trends in emerging markets today.",
            "Building in public: Week 12 update. Revenue up 25%, churn down 10%.",
            "Seeking beta testers for our new collaboration tool. Founders and investors welcome!",
            "Networking tip: Don't just connect, collaborate. Shared intent creates stronger bonds.",
        };

        var posts = new List<Post>();
        for (var i = 0; i < postContents.Length; i++)
        {
            posts.Add(new Post
            {
                UserId = users[i % users.Count].Id,
                Content = postContents[i],
            });
        }

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();
        return posts;
    }

    private static async Task SeedPostLikesAndCommentsAsync(
        ApplicationDbContext context, List<AppUser> users, List<Post> posts)
    {
        Randomizer.Seed = new Random(BoguseSeed + 3);
        var faker = new Faker("en");

        var likes = new List<PostLike>();
        var comments = new List<PostComment>();
        var usedLikes = new HashSet<(Guid PostId, string UserId)>();

        foreach (var post in posts)
        {
            // 1-3 likes per post from random users (not the author)
            var likers = faker.PickRandom(users.Where(u => u.Id != post.UserId), faker.Random.Int(1, 3)).ToList();
            foreach (var liker in likers)
            {
                if (usedLikes.Add((post.Id, liker.Id)))
                {
                    likes.Add(new PostLike { PostId = post.Id, UserId = liker.Id });
                    post.LikeCount++;
                }
            }

            // 0-2 comments per post
            var commentCount = faker.Random.Int(0, 2);
            for (var j = 0; j < commentCount; j++)
            {
                var commenter = faker.PickRandom(users.Where(u => u.Id != post.UserId));
                comments.Add(new PostComment
                {
                    PostId = post.Id,
                    UserId = commenter.Id,
                    CommentText = faker.Lorem.Sentence(),
                });
                post.CommentCount++;
            }
        }

        context.PostLikes.AddRange(likes);
        context.PostComments.AddRange(comments);
        context.Posts.UpdateRange(posts);
        await context.SaveChangesAsync();
    }

    private static async Task SeedFollowsAsync(ApplicationDbContext context, List<AppUser> users)
    {
        // Cross-role follows
        var follows = new List<Follow>
        {
            new() { FollowerId = users[0].Id, FollowingId = users[3].Id }, // founder1 → investor1
            new() { FollowerId = users[0].Id, FollowingId = users[6].Id }, // founder1 → talent1
            new() { FollowerId = users[3].Id, FollowingId = users[0].Id }, // investor1 → founder1
            new() { FollowerId = users[3].Id, FollowingId = users[1].Id }, // investor1 → founder2
            new() { FollowerId = users[6].Id, FollowingId = users[0].Id }, // talent1 → founder1
            new() { FollowerId = users[7].Id, FollowingId = users[2].Id }, // talent2 → founder3
            new() { FollowerId = users[1].Id, FollowingId = users[4].Id }, // founder2 → investor2
            new() { FollowerId = users[4].Id, FollowingId = users[1].Id }, // investor2 → founder2
            new() { FollowerId = users[8].Id, FollowingId = users[5].Id }, // talent3 → investor3
            new() { FollowerId = users[2].Id, FollowingId = users[9].Id }, // founder3 → admin1
        };

        context.Follows.AddRange(follows);
        await context.SaveChangesAsync();
    }
}
