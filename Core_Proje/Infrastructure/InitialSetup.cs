using System.Data;
using DataAccessLayer.Concrete;
using EntityLayer.Concrete;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Core_Proje.Infrastructure;

internal static class InitialSetup
{
    internal sealed class SetupException(string message) : Exception(message);

    internal static bool ReadFlag(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (bool.TryParse(value, out bool enabled)) return enabled;
        throw new SetupException($"{name} yalnızca true veya false olmalıdır. Web sunucusu başlatılmadı.");
    }

    internal static async Task RunAsync(IServiceProvider services, bool admin, bool content)
    {
        // Hesap bilgilerini herhangi bir veritabanı işleminden önce doğrula.
        string username = admin ? Required("CORE_PROJE_ADMIN_USERNAME") : "";
        string email = admin ? Required("CORE_PROJE_ADMIN_EMAIL") : "";
        string password = admin ? Required("CORE_PROJE_ADMIN_PASSWORD") : "";

        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<Context>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WriterUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<WriterRole>>();

        // Migration/EnsureCreated çağrısı yoktur. Şema önceden hazırlanmalıdır.
        // Identity store ve içerik işlemleri aynı scoped Context/transaction'ı kullanır.
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        if (admin)
        {
            if (await users.FindByNameAsync(username) is not null ||
                await users.FindByEmailAsync(email) is not null)
            {
                throw new SetupException("Yönetici hazırlığı durduruldu: kullanıcı adı veya e-posta mevcut bir hesapla eşleşiyor. Hesap, parola, profil ve roller değiştirilmedi. Diğer hazırlık adımları uygulanmadı.");
            }
        }

        bool addAbout = false, addFeature = false, addContact = false;
        if (content)
        {
            addAbout = await NeedsRecordAsync(context.Abouts, x => x.AboutID == 1, "About");
            addFeature = await NeedsRecordAsync(context.Features, x => x.FeatureID == 1, "Feature");
            addContact = await NeedsRecordAsync(context.Contacts, x => x.ContactID == 1, "Contact");
        }

        if (admin)
        {
            if (!await roles.RoleExistsAsync("Admin"))
            {
                Check(await roles.CreateAsync(new WriterRole { Name = "Admin" }), "Admin rolü oluşturulamadı.");
            }

            var user = new WriterUser
            {
                UserName = username,
                Email = email,
                Name = "Örnek",
                Surname = "Yönetici",
                ImageURL = ""
            };
            Check(await users.CreateAsync(user, password), "Yönetici hesabı oluşturulamadı. Kullanıcı adı, e-posta ve Identity parola kurallarını kontrol edin.");
            Check(await users.AddToRoleAsync(user, "Admin"), "Admin rolü yeni hesaba atanamadı.");
        }

        About? about = null;
        Feature? feature = null;
        Contact? contact = null;
        if (addAbout)
        {
            about = new About
            {
                Title = "Örnek hakkımda", Description = "Başlangıç için örnek içerik.",
                Age = "—", Mail = "demo@example.invalid", Phone = "—", Address = "Örnek adres",
                ImageUrl = ""
            };
            context.Abouts.Add(about);
        }
        if (addFeature)
        {
            feature = new Feature { Header = "Örnek portföy", Name = "Örnek Kullanıcı", Title = "Örnek çalışma alanı" };
            context.Features.Add(feature);
        }
        if (addContact)
        {
            contact = new Contact
            {
                Title = "Örnek iletişim", Description = "Bu bilgiler örnektir.",
                Mail = "demo@example.invalid", Phone = "—"
            };
            context.Contacts.Add(contact);
        }

        await context.SaveChangesAsync();
        if ((about is not null && about.AboutID != 1) ||
            (feature is not null && feature.FeatureID != 1) ||
            (contact is not null && contact.ContactID != 1))
        {
            throw new SetupException("İçerik hazırlığı durduruldu: boş tablonun identity sayacı ID = 1 üretmedi. Bu çalıştırmanın değişiklikleri geri alındı; sayaçlar sıfırlanmadı. Ayrı, yeni bir deneme veritabanı kullanın.");
        }

        await transaction.CommitAsync();
    }

    private static string Required(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new SetupException($"{name} tanımlanmalıdır. Başlangıç hazırlığı çalıştırılmadı.");
        return value;
    }

    private static void Check(IdentityResult result, string message)
    {
        // Error.Description kullanıcı girdisi içerebilir; çıktıya taşınmaz.
        if (!result.Succeeded) throw new SetupException(message);
    }

    private static async Task<bool> NeedsRecordAsync<T>(DbSet<T> records,
        System.Linq.Expressions.Expression<Func<T, bool>> hasIdOne, string label) where T : class
    {
        if (await records.AnyAsync(hasIdOne)) return false;
        if (await records.AnyAsync())
            throw new SetupException($"{label} tablosunda kayıt var ancak ID = 1 yok. Hazırlık durduruldu; mevcut kayıtlar değiştirilmedi.");
        return true;
    }
}
