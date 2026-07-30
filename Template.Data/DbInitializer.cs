using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Template.Common.Static;
using Template.Data.Configurations;
using Template.Data.Entities;

namespace Template.Data;

public static class DbInitializer
{
    private const string DefaultPassword = "Admin@123";

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        await context.Database.MigrateAsync();

        foreach (var roleName in new[] { "Admin", "Staff", "InnovationTeam" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }

        await SeedRolePermissionsAsync(roleManager, "Admin", GetAllPermissions());

        await SeedUserAsync(
            userManager,
            userName: "admin",
            email: "admin@bou.or.ug",
            roleName: "Admin",
            fullName: "System Administrator",
            firstName: "System",
            lastName: "Administrator",
            title: "Administrator",
            businessUnit: "ICT",
            jobTitle: "System Administrator",
            station: "Head Office");

        await SeedUserAsync(
            userManager,
            userName: "staff",
            email: "staff@bou.or.ug",
            roleName: "Staff",
            fullName: "Test Staff User",
            firstName: "Test",
            lastName: "Staff",
            title: "Officer",
            businessUnit: "Operations",
            jobTitle: "Innovation Officer",
            station: "Head Office");

        await SeedUserAsync(
            userManager,
            userName: "innovation",
            email: "innovation@bou.or.ug",
            roleName: "InnovationTeam",
            fullName: "Innovation Team User",
            firstName: "Innovation",
            lastName: "Reviewer",
            title: "Reviewer",
            businessUnit: "Strategy",
            jobTitle: "Innovation Team Lead",
            station: "Head Office");

        await ResetLoggedInStateAsync(userManager);
        await SeedCategoriesAsync(context);
        await SeedReferenceDataAsync(context);
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string userName,
        string email,
        string roleName,
        string fullName,
        string firstName,
        string lastName,
        string title,
        string businessUnit,
        string jobTitle,
        string station)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user != null)
        {
            if (!await userManager.IsInRoleAsync(user, roleName))
            {
                await userManager.AddToRoleAsync(user, roleName);
            }

            return;
        }

        user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            FullName = fullName,
            FirstName = firstName,
            LastName = lastName,
            Title = title,
            BusinessUnit = businessUnit,
            JobTitle = jobTitle,
            Station = station,
            AgeBracket = "35-44",
            Gender = "Unspecified",
            IsActive = true,
            EmailConfirmed = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = Guid.Empty,
            LastActivity = DateTime.UtcNow.AddHours(-1),
            IsLoggedIn = false
        };

        var result = await userManager.CreateAsync(user, DefaultPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, roleName);
        }
    }

    private static async Task SeedRolePermissionsAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        string roleName,
        IEnumerable<string> permissions)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            return;
        }

        var existingClaims = await roleManager.GetClaimsAsync(role);
        var existingPermissions = existingClaims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var permission in permissions)
        {
            if (!existingPermissions.Contains(permission))
            {
                await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
            }
        }
    }

    private static List<string> GetAllPermissions()
    {
        var permissions = new List<string>();
        var nestedTypes = typeof(SystemPermissions).GetNestedTypes();

        foreach (var type in nestedTypes)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string));

            foreach (var field in fields)
            {
                if (field.GetValue(null) is string permissionValue)
                {
                    permissions.Add(permissionValue);
                }
            }
        }

        return permissions;
    }

   private static async Task ResetLoggedInStateAsync(UserManager<ApplicationUser> userManager)
{
    var loggedInUsers = await userManager.Users.Where(u => u.IsLoggedIn).ToListAsync();
    foreach (var user in loggedInUsers)
    {
        user.IsLoggedIn = false;
        await userManager.UpdateAsync(user);
    }
}
    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        var defaults = new[]
        {
            ("Product/Service", "New or significantly improved products or services"),
            ("Process", "Workflow and operational improvements"),
            ("Business Model", "New approaches to delivering the Bank's mandate"),
            ("Policy/Governance", "Policies, governance structures and institutional frameworks"),
            ("Technology/System", "Technology-driven ideas and digital solutions"),
            ("Sustainability", "Environmental, social impact and financial inclusion innovations"),
            ("Organisational/Institutional", "Internal innovations that enhance agility, capacity and collaboration")
        };
        var existing = await context.Categories.Select(c => c.Name).ToListAsync();
        foreach (var item in defaults.Where(d => !existing.Contains(d.Item1)))
            context.Categories.Add(new Category { Name = item.Item1, Description = item.Item2, IsActive = true });

        await context.SaveChangesAsync();
    }

    private static async Task SeedReferenceDataAsync(ApplicationDbContext context)
    {
        // Business Units
        var businessUnits = new[] {
            "Accounts", "Administrative Services", "Agricultural Credit Facility & Other Credit Schemes",
            "Banking", "Bank Resolution", "Board Affairs", "Commercial Banking", "Communications",
            "Compliance", "Corporate Services", "Currency", "Economics", "Finance", "Financial Markets",
            "Financial Stability", "Governor's Office", "Human Resources", "ICT", "Internal Audit",
            "Legal Services", "Management Information Systems", "Operations", "Planning & Budgeting",
            "Procurement & Disposal", "Research", "Risk Management", "Secretariat",
            "Strategy & Innovation", "Supervision"
        };
        var existingBUs = await context.BusinessUnits.Select(b => b.Name).ToListAsync();
        foreach (var name in businessUnits.Where(n => !existingBUs.Contains(n)))
            context.BusinessUnits.Add(new BusinessUnit { Name = name, IsActive = true });

        // Stations
        var stations = new[] {
            "Headquarters", "BOU Clinic / William Street", "Arua", "Gulu", "Mbale",
            "Masaka", "Mbarara", "Fort Portal", "Jinja", "Soroti", "Lira"
        };
        var existingStations = await context.Stations.Select(s => s.Name).ToListAsync();
        foreach (var name in stations.Where(n => !existingStations.Contains(n)))
            context.Stations.Add(new Station { Name = name, IsActive = true });

        // Ranks
        var ranks = new[] {
            "Executive Management (Governor, Deputy Governor, Executive Directors)",
            "Senior Management (Directors and Deputy Directors)",
            "Middle Management (Team Leaders, SPBOs, and PBOs)",
            "Operational Management (SBOIs, SBOIIs, BOIs, BOIIs)",
            "Administrative Assistant", "Clerical Staff"
        };
        var existingRanks = await context.Ranks.Select(r => r.Name).ToListAsync();
        foreach (var name in ranks.Where(n => !existingRanks.Contains(n)))
            context.Ranks.Add(new Rank { Name = name, IsActive = true });

        // Age Brackets
        var ageBrackets = new[] { "18 - 24", "25 - 34", "35 - 44", "45 - 54", "55 and above" };
        var existingAges = await context.AgeBrackets.Select(a => a.Name).ToListAsync();
        for (int i = 0; i < ageBrackets.Length; i++)
            if (!existingAges.Contains(ageBrackets[i]))
                context.AgeBrackets.Add(new AgeBracket { Name = ageBrackets[i], DisplayOrder = i + 1, IsActive = true });

        // Genders
        var genders = new[] { "Male", "Female" };
        var existingGenders = await context.Genders.Select(g => g.Name).ToListAsync();
        foreach (var name in genders.Where(n => !existingGenders.Contains(n)))
            context.Genders.Add(new Gender { Name = name, IsActive = true });

        // Types of Innovation
        var innovationTypes = new Dictionary<string, string>
        {
            { "Product / Service", "New or enhanced offerings that transform stakeholder or customer experiences." },
            { "Process", "Innovative methods or workflows that improve efficiency, quality, or speed." },
            { "Business Model", "Novel approaches to fulfilling BoU's mandate or generating value." },
            { "Policy / Governance", "Changing or improving the rules, standards, or oversight mechanisms." },
            { "Technology / System", "Introduction or application of new technological capabilities." },
            { "Sustainability", "Innovative initiatives that strengthen public trust and long-term viability." },
            { "Organisational / Institutional", "Internal innovations that enhance agility, capacity and collaboration." }
        };
        var existingTypes = await context.TypesOfInnovation.Select(t => t.Name).ToListAsync();
        foreach (var kvp in innovationTypes.Where(kv => !existingTypes.Contains(kv.Key)))
            context.TypesOfInnovation.Add(new TypeOfInnovation { Name = kvp.Key, Description = kvp.Value, IsActive = true });

        // Strategic Alignments
        var strategicAlignments = new[] {
            "Enhance Stakeholder Confidence", "Enhance Price Stability",
            "Enhance Financial System Soundness and Resilience",
            "Enhance Financial System Development", "Enhance Financial Performance",
            "Improve Efficiency and Reliability of Bank Processes",
            "Improve Employee Competence", "Improve Organisational Systems and Infrastructure",
            "Enhance Organisational Culture"
        };
        var existingAlignments = await context.StrategicAlignments.Select(s => s.Name).ToListAsync();
        foreach (var name in strategicAlignments.Where(n => !existingAlignments.Contains(n)))
            context.StrategicAlignments.Add(new StrategicAlignment { Name = name, IsActive = true });

        // Innovation Priority Areas
        var priorityAreas = new[] {
            "Digital Transformation", "Process Optimization", "Customer/Stakeholder Experience",
            "Financial Inclusion", "Risk Management", "Data Analytics & Insights",
            "Sustainability & ESG", "Regulatory Innovation", "Employee Engagement", "Other"
        };
        var existingAreas = await context.InnovationPriorityAreas.Select(p => p.Name).ToListAsync();
        foreach (var name in priorityAreas.Where(n => !existingAreas.Contains(n)))
            context.InnovationPriorityAreas.Add(new InnovationPriorityArea { Name = name, IsActive = true });

        // Expected Timelines
        var timelines = new[] { "0-3 Months", "3-6 Months", "6-12 Months", "12-24 Months", "24+ Months" };
        var existingTimelines = await context.ExpectedTimelines.Select(t => t.Name).ToListAsync();
        for (int i = 0; i < timelines.Length; i++)
            if (!existingTimelines.Contains(timelines[i]))
                context.ExpectedTimelines.Add(new ExpectedTimeline { Name = timelines[i], DisplayOrder = i + 1, IsActive = true });

        // Estimated Budget Ranges
        var budgetRanges = new[] { "Below 10M UGX", "10M - 50M UGX", "50M - 100M UGX", "100M - 500M UGX", "Above 500M UGX", "Not Applicable" };
        var existingBudgets = await context.EstimatedBudgetRanges.Select(b => b.Name).ToListAsync();
        for (int i = 0; i < budgetRanges.Length; i++)
            if (!existingBudgets.Contains(budgetRanges[i]))
                context.EstimatedBudgetRanges.Add(new EstimatedBudgetRange { Name = budgetRanges[i], DisplayOrder = i + 1, IsActive = true });

        // Idea Status Options (for filter dropdowns)
        var statuses = new[] { "Submitted", "Under Review", "Pending Information", "Approved", "Declined" };
        var existingStatuses = await context.IdeaStatusOptions.Select(s => s.Name).ToListAsync();
        for (int i = 0; i < statuses.Length; i++)
            if (!existingStatuses.Contains(statuses[i]))
                context.IdeaStatusOptions.Add(new IdeaStatusOption { Name = statuses[i], DisplayOrder = i + 1 });

        await context.SaveChangesAsync();
    }
}
