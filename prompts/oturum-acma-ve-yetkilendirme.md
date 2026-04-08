ASP.NET Core MVC projesine Authentication ve Authorization altyapısı ekle.

Beklenen gereksinimler:

1. Kimlik doğrulama altyapısı
   ASP.NET Core Identity kullanılsın.
   Giriş ve kayıt işlemleri Identity tabanlı çalışsın.
   Oturum yönetimi cookie authentication ile yapılandırılsın.
   Logout işlemi eklensin.
   Yetkisiz erişimlerde kullanıcı login sayfasına yönlendirilsin.
   Erişim reddedilen durumlar için AccessDenied sayfası oluşturulsun.

2. Rol yapısı
   Sistemde varsayılan olarak şu roller tanımlı olsun:
   Normal
   Editor
   Admin

Bu roller uygulama ilk çalıştığında seed mekanizması ile veritabanına eklensin.
Seed işlemi tekrar çalıştırıldığında mükerrer kayıt oluşmasın.

3. Varsayılan kullanıcı
   Uygulama ilk çalıştığında aşağıdaki kullanıcı seed ile oluşturulsun:
   Ad Soyad: Zafer CÖMERT
   E-posta: [comertzafer@gmail.com](mailto:comertzafer@gmail.com)
   Şifre: Zafer123456

Bu kullanıcıya varsayılan olarak Admin rolü atansın.
Kullanıcı zaten varsa tekrar eklenmesin.
E-posta doğrulama başlangıç aşamasında zorunlu olmasın.

4. Şifre politikası
   Şifre politikası esnek olsun.
   Aşağıdaki zorunluluklar kaldırılabilir veya gevşetilebilir:
   Büyük harf zorunluluğu olmasın
   Küçük harf zorunluluğu olmasın
   Rakam zorunluluğu olmasın
   Özel karakter zorunluluğu olmasın
   Minimum uzunluk makul bir değer olacak şekilde yapılandırılsın

5. Sayfalar
   Aşağıdaki sayfalar oluşturulsun:
   Login
   Register
   AccessDenied

Login sayfasında:
E-posta
Şifre
Beni hatırla
Giriş yap

Register sayfasında:
Ad Soyad
E-posta
Şifre
Şifre tekrar

Formlarda doğrulama mesajları gösterilsin.

6. Yetkilendirme kuralları
   Anonim kullanıcılar herkese açık sayfalara erişebilsin.
   Giriş gerektiren sayfalarda [Authorize] kullanılsın.
   Sadece Admin rolündeki kullanıcıların erişebileceği örnek bir Admin alanı veya Admin controller oluşturulsun.
   Gerekirse Editor ve Admin için ayrı yetki örnekleri gösterilsin.
   Rol bazlı authorization attribute kullanımı örneklerle eklensin.

7. Menü ve arayüz davranışı
   Top menüde kullanıcı giriş yapmamışsa:
   Login
   Register
   linkleri görünsün.

Kullanıcı giriş yapmışsa:
Kullanıcı adı veya e-posta bilgisi
Logout
görüntülensin.

Kullanıcının Admin rolü varsa top menüde ayrıca Admin bağlantısı görünsün.
Admin rolü yoksa bu bağlantı görünmesin.

8. Veritabanı ve kurulum
   Identity tabloları veritabanına migration ile eklensin.
   Gerekli DbContext yapılandırmaları tam olsun.
   Program.cs veya ilgili başlangıç dosyasında Identity, authentication ve authorization servisleri doğru şekilde yapılandırılsın.

9. Kod beklentisi
   Gerekli model, viewmodel, controller, view, seed ve configuration kodları eksiksiz verilsin.
   Kodlar çalıştırılabilir ve tutarlı olsun.
   Gerekli NuGet paketleri belirtilsin.
   Adım adım hangi dosyanın nereye ekleneceği açıklansın.

10. Ek beklenti
    Varsayılan yapı güvenli ve geliştirilebilir olsun.
    İleride e-posta doğrulama, şifre sıfırlama ve kullanıcı profil yönetimi eklenebilecek şekilde temiz bir mimari tercih edilsin.
11. Oturum açma ve yetkilendirme ile ilgili tümler işlevleri AuthService.cs dosyasında organize et. 
12. E-posta adresi unique (benzersiz, eşsiz) olmalı. 

 