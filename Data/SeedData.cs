using HomeNursingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeNursingSystem.Data;

public static class SeedData
{
    public static async Task EnsureSeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        foreach (var role in new[] { AppRoles.Admin, AppRoles.Patient, AppRoles.Nurse, AppRoles.ClinicOwner })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (!await context.Services.AnyAsync())
        {
            var servicesList = new[]
            {
                new MedicalService { ServiceName = "Injection", Description = "إعطاء الحقن", IconClass = "fa-syringe", BasePrice = 35 },
                new MedicalService { ServiceName = "IV Drip Setup", Description = "تركيب المحاليل الوريدية", IconClass = "fa-droplet", BasePrice = 120 },
                new MedicalService { ServiceName = "Wound Dressing", Description = "تغيير ضمادات الجروح", IconClass = "fa-bandage", BasePrice = 55 },
                new MedicalService { ServiceName = "Blood Pressure Monitoring", Description = "قياس ضغط الدم", IconClass = "fa-heart-pulse", BasePrice = 30 },
                new MedicalService { ServiceName = "Blood Sugar Test", Description = "فحص السكر", IconClass = "fa-vial", BasePrice = 25 },
                new MedicalService { ServiceName = "Minor Surgical Dressing", Description = "تضميد جراحي بسيط", IconClass = "fa-user-doctor", BasePrice = 80 },
                new MedicalService { ServiceName = "Post-Surgery Care", Description = "رعاية ما بعد الجراحة", IconClass = "fa-hospital", BasePrice = 150 },
                new MedicalService { ServiceName = "Elderly Care Visit", Description = "زيارة رعاية مسنين", IconClass = "fa-hand-holding-heart", BasePrice = 90 },
            };
            context.Services.AddRange(servicesList);
            await context.SaveChangesAsync();
        }

        ApplicationUser? admin = await userManager.FindByEmailAsync("admin@system.com");
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin@system.com",
                Email = "admin@system.com",
                EmailConfirmed = true,
                FullName = "مدير النظام",
                PhoneNumber = "0500000000",
                Role = AppRoles.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                City = "الرياض",
                Neighborhood = "العليا"
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }

        if (!await context.NurseProfiles.AnyAsync())
        {
            var svcIds = await context.Services.OrderBy(s => s.ServiceId).Select(s => s.ServiceId).ToListAsync();

            async Task<(ApplicationUser user, NurseProfile profile)> CreateNurseAsync(
                string email, string name, string spec, string bio, int years)
            {
                var u = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = name,
                    PhoneNumber = "0501111111",
                    Role = AppRoles.Nurse,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    City = "الرياض",
                    Neighborhood = "الملز",
                    Street = "شارع الأمير محمد"
                };
                await userManager.CreateAsync(u, "Demo@123");
                await userManager.AddToRoleAsync(u, AppRoles.Nurse);
                var np = new NurseProfile
                {
                    UserId = u.Id,
                    Specialization = spec,
                    YearsOfExperience = years,
                    Bio = bio,
                    IsVerified = true,
                    IsAvailable = true,
                    AverageRating = 4.5m,
                    TotalReviews = 2,
                    CreatedAt = DateTime.UtcNow
                };
                context.NurseProfiles.Add(np);
                await context.SaveChangesAsync();
                return (u, np);
            }

            var (n1, p1) = await CreateNurseAsync("nurse1@demo.com", "ممرضة فاطمة الأحمد", "تمريض منزلي",
                "ممرضة معتمدة مع خبرة في الرعاية المنزلية وإدارة الأدوية.", 8);
            var (n2, p2) = await CreateNurseAsync("nurse2@demo.com", "ممرض خالد السالم", "عناية جروح",
                "متخصص في تغيير الضمادات ومتابعة ما بعد العمليات.", 5);

            if (svcIds.Count >= 4)
            {
                context.NurseServices.AddRange(
                    new NurseServiceLink { NurseProfileId = p1.NurseProfileId, ServiceId = svcIds[0], CustomPrice = 40 },
                    new NurseServiceLink { NurseProfileId = p1.NurseProfileId, ServiceId = svcIds[2], CustomPrice = 60 },
                    new NurseServiceLink { NurseProfileId = p1.NurseProfileId, ServiceId = svcIds[3], CustomPrice = 35 },
                    new NurseServiceLink { NurseProfileId = p2.NurseProfileId, ServiceId = svcIds[2], CustomPrice = 55 },
                    new NurseServiceLink { NurseProfileId = p2.NurseProfileId, ServiceId = svcIds[5], CustomPrice = 90 }
                );
            }

            await context.SaveChangesAsync();
        }

        if (!await context.Clinics.AnyAsync(c => c.ClinicName == "عيادة النور"))
        {
            var owner1 = new ApplicationUser
            {
                UserName = "clinic1@demo.com",
                Email = "clinic1@demo.com",
                EmailConfirmed = true,
                FullName = "د. سارة المطيري",
                PhoneNumber = "0502222222",
                Role = AppRoles.ClinicOwner,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                City = "جدة",
                Neighborhood = "الروضة"
            };
            await userManager.CreateAsync(owner1, "Demo@123");
            await userManager.AddToRoleAsync(owner1, AppRoles.ClinicOwner);

            context.Clinics.Add(new Clinic
            {
                OwnerId = owner1.Id,
                ClinicName = "عيادة النور",
                Description = "عيادة متعددة التخصصات مع رعاية منزلية.",
                Address = "جدة، حي الروضة",
                Neighborhood = "الروضة",
                City = "جدة",
                Latitude = 21.5433,
                Longitude = 39.1728,
                PhoneNumber = "0123456789",
                Email = "info@noor-clinic.demo",
                IsVerified = true,
                IsActive = true,
                AverageRating = 4.2m,
                TotalReviews = 3,
                OpeningHours = "السبت–الخميس8ص–10م",
                CreatedAt = DateTime.UtcNow
            });

            var owner2 = new ApplicationUser
            {
                UserName = "clinic2@demo.com",
                Email = "clinic2@demo.com",
                EmailConfirmed = true,
                FullName = "م. عبدالله القحطاني",
                PhoneNumber = "0503333333",
                Role = AppRoles.ClinicOwner,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                City = "الدمام",
                Neighborhood = "الفيصلية"
            };
            await userManager.CreateAsync(owner2, "Demo@123");
            await userManager.AddToRoleAsync(owner2, AppRoles.ClinicOwner);

            context.Clinics.Add(new Clinic
            {
                OwnerId = owner2.Id,
                ClinicName = "مركز الرعاية الأولى",
                Description = "خدمات تمريض وعلاج طبيعي.",
                Address = "الدمام، حي الفيصلية",
                Neighborhood = "الفيصلية",
                City = "الدمام",
                Latitude = 26.3927,
                Longitude = 49.9777,
                PhoneNumber = "0137654321",
                Email = "care@first.demo",
                IsVerified = true,
                IsActive = true,
                AverageRating = 4.7m,
                TotalReviews = 5,
                OpeningHours = "يومياً 9ص–9م",
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }

        if (!await context.Articles.AnyAsync())
        {
            var adminUser = await userManager.FindByEmailAsync("admin@system.com");
            if (adminUser == null) return;

            var articles = new List<Article>
            {
                new()
                {
                    AuthorId = adminUser.Id,
                    Title = "نصائح للوقاية من الإنفلونزا",
                    Slug = "flu-prevention-tips",
                    Summary = "إرشادات بسيطة لتقوية المناعة في المنزل.",
                    Content = "المحتوى الكامل للمقال: النوم الكافي، الغذاء المتوازن، غسل اليدين، واللقاحات الموصى بها.",
                    Category = ArticleCategories.HealthTips,
                    IsNews = false,
                    IsPublished = true,
                    ViewCount = 12,
                    PublishedAt = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new()
                {
                    AuthorId = adminUser.Id,
                    Title = "افتتاح خدمة التمريض المنزلي في المنطقة الشرقية",
                    Slug = "home-nursing-east-region",
                    Summary = "توسعة تغطية الخدمات لتشمل مدناً جديدة.",
                    Content = "خبر: أعلنت المنصة عن توسعة نطاق الخدمة ليشمل عدة مدن في المنطقة الشرقية مع فرق تمريض معتمدة.",
                    Category = ArticleCategories.News,
                    IsNews = true,
                    IsPublished = true,
                    ViewCount = 45,
                    PublishedAt = DateTime.UtcNow.AddDays(-2),
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    AuthorId = adminUser.Id,
                    Title = "دليل المريض: كيف تحضر للزيارة المنزلية",
                    Slug = "patient-guide-home-visit",
                    Summary = "خطوات تسهل على الممرض تقديم أفضل رعاية.",
                    Content = "المحتوى: جهز قائمة الأدوية، مساحة مضاءة، واطلب من أحد المرافقين التواجد عند الحاجة.",
                    Category = ArticleCategories.Guides,
                    IsNews = false,
                    IsPublished = true,
                    ViewCount = 28,
                    PublishedAt = DateTime.UtcNow.AddDays(-10),
                    CreatedAt = DateTime.UtcNow.AddDays(-11)
                },
                new()
                {
                    AuthorId = adminUser.Id,
                    Title = "أهمية مراقبة ضغط الدم لدى كبار السن",
                    Slug = "bp-monitoring-elderly",
                    Summary = "لماذا القياس الدوري يقلل المخاطر.",
                    Content = "شرح مبسط لأهداف الضغط، أوقات القياس، ومتى تتصل بالطبيب.",
                    Category = ArticleCategories.HealthTips,
                    IsNews = false,
                    IsPublished = true,
                    ViewCount = 19,
                    PublishedAt = DateTime.UtcNow.AddDays(-7),
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new()
                {
                    AuthorId = adminUser.Id,
                    Title = "تحديث: معايير التحقق من التراخيص",
                    Slug = "license-verification-update",
                    Summary = "إجراءات جديدة لضمان جودة مقدمي الخدمة.",
                    Content = "خبر إداري: تم تحديث آلية مراجعة رخص الممرضين والعيادات لزيادة الشفافية.",
                    Category = ArticleCategories.News,
                    IsNews = true,
                    IsPublished = true,
                    ViewCount = 8,
                    PublishedAt = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            };

            context.Articles.AddRange(articles);
            await context.SaveChangesAsync();
        }
    }
}
