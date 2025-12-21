# Üniversite Sınav Programı Hazırlama Uygulaması

Bu proje, bir üniversitede dönem sonu veya ara sınavların otomatik olarak planlanması ve dersliklere yerleştirilmesi sürecini bilgisayar ortamına taşıyan bir ASP.NET MVC 5 uygulamasıdır.

## Teknik Gereksinimler

- **Programlama Dili:** C#
- **Framework:** ASP.NET MVC 5 (.NET Framework 4.7.2)
- **Veritabanı:** Microsoft SQL Server 2012
- **UI:** Web tabanlı (Razor Views, Bootstrap 3)

## Proje Yapısı

```
ExamSchedulingSystem/
├── Database/
│   ├── schema.sql          # Veritabanı şeması (CREATE DATABASE, TABLES, etc.)
│   └── seed.sql            # Örnek test verileri
├── Models/                 # Entity modelleri
├── Controllers/            # MVC Controller'lar
├── Views/                  # Razor View'lar
├── DAL/                    # Data Access Layer (Entity Framework Context)
├── Services/               # İş mantığı servisleri
│   ├── ExamSchedulingService.cs  # Otomatik sınav programlama algoritması
│   └── ExportService.cs          # Excel export servisi
└── Web.config             # Uygulama yapılandırması
```

## Kurulum ve Çalıştırma Adımları

### 1. SQL Server 2012 Kurulumu

SQL Server 2012'in kurulu olduğundan emin olun. SQL Server Management Studio (SSMS) kullanarak veritabanını oluşturacağız.

### 2. Veritabanı Oluşturma

1. **SQL Server Management Studio'yu açın**
2. **SQL Server'a bağlanın** (genellikle `localhost` veya `localhost\SQLEXPRESS`)
3. **schema.sql dosyasını açın** (`Database/schema.sql`)
4. **Tüm içeriği seçin ve çalıştırın (F5)**
   - Bu işlem `ExamSchedulingDB` veritabanını ve tüm tabloları oluşturur
5. **seed.sql dosyasını açın** (`Database/seed.sql`)
6. **Tüm içeriği seçin ve çalıştırın (F5)**
   - Bu işlem örnek test verilerini ekler

### 3. Connection String Ayarlama

1. **Visual Studio'da projeyi açın**
2. **Web.config dosyasını açın**
3. **Connection string'i bulun** (satır 11-12):
   ```xml
   <connectionStrings>
       <add name="ExamSchedulingContext" 
            connectionString="Server=localhost;Database=ExamSchedulingDB;Integrated Security=True;MultipleActiveResultSets=True;" 
            providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```
4. **SQL Server'ınıza göre düzenleyin:**
   - **Default instance:** `Server=localhost;Database=ExamSchedulingDB;Integrated Security=True;`
   - **Named instance (örn: SQLEXPRESS):** `Server=localhost\SQLEXPRESS;Database=ExamSchedulingDB;Integrated Security=True;`
   - **SQL Server Authentication:** `Server=localhost;Database=ExamSchedulingDB;User Id=sa;Password=YourPassword;`

### 4. NuGet Paketlerini Yükleme

Visual Studio'da projeyi açtığınızda NuGet paketleri otomatik olarak restore edilmelidir. Eğer edilmezse:

1. **Solution Explorer'da projeye sağ tıklayın**
2. **"Restore NuGet Packages" seçeneğini tıklayın**
3. Veya **Tools > NuGet Package Manager > Package Manager Console** açın ve şunu çalıştırın:
   ```
   Update-Package -reinstall
   ```

### 5. Projeyi Çalıştırma

1. **Visual Studio'da projeyi açın**
2. **F5 tuşuna basın** veya **Debug > Start Debugging**
3. Tarayıcı otomatik olarak açılacak (genellikle `http://localhost:12345/`)

### 6. Giriş Yapma

Uygulama açıldığında login sayfası görünecektir. Aşağıdaki demo kullanıcılardan biriyle giriş yapabilirsiniz:

| Rol | Kullanıcı Adı | Şifre |
|-----|---------------|-------|
| **Admin** | `admin` | `Admin123!` |
| **Bölüm Yetkilisi** | `dept_officer1` | `Dept123!` |
| **Öğretim Üyesi** | `lecturer1` | `Lect123!` |
| **Öğrenci** | `student1` | `Stud123!` |

## Kullanım Kılavuzu

### Admin Kullanıcısı

Admin kullanıcısı tüm sistemi yönetebilir:

1. **Fakülte, Bölüm, Ders, Öğretim Üyesi, Derslik Yönetimi:**
   - Menüden "Yönetim" altındaki ilgili linklere tıklayın
   - CRUD işlemlerini (Create, Read, Update, Delete) yapabilirsiniz

2. **Öğretim Üyesi Müsaitlik Girişi:**
   - Menüden "Müsaitlik" linkine tıklayın
   - Her öğretim üyesi için hangi günlerde ve saatlerde müsait olduğunu girebilirsiniz

3. **Otomatik Sınav Programı Oluşturma:**
   - Menüden "Program Oluştur" linkine tıklayın
   - Başlangıç ve bitiş tarihlerini seçin
   - Sınav saatlerini belirleyin (örn: 09:00 - 17:00)
   - "Program Oluştur" butonuna tıklayın
   - Sistem otomatik olarak:
     - HasExam=true olan tüm dersler için sınav planlar
     - Öğretim üyesi müsaitliğini kontrol eder
     - Derslik çakışmalarını önler
     - Kapasite yetersizse birden fazla derslik atar

4. **Sınav Programını Görüntüleme:**
   - Menüden "Sınav Programı" linkine tıklayın
   - Fakülte/Bölüm bazında filtreleme yapabilirsiniz
   - Excel'e aktarabilirsiniz

### Bölüm Yetkilisi

Bölüm Yetkilisi kendi bölümüne ait dersleri, öğretim üyelerini ve özel durumları sisteme girebilir. Admin ile aynı CRUD işlemlerini yapabilir.

### Öğretim Üyesi / Öğrenci

Sadece sınav programını görüntüleyebilir (read-only).

## Özellikler

### ✅ Tamamlanan Özellikler

1. **Kullanıcı Rolleri ve Yetkilendirme:**
   - Admin, DepartmentOfficer, Lecturer, Student rolleri
   - Role-based authorization

2. **CRUD İşlemleri:**
   - Faculty (Fakülte)
   - Department (Bölüm)
   - Course (Ders)
   - Lecturer (Öğretim Üyesi)
   - Classroom (Derslik)

3. **Öğretim Üyesi Müsaitlik Yönetimi:**
   - Haftanın günlerine göre müsaitlik girişi
   - Saat aralığı belirleme

4. **Otomatik Sınav Programlama:**
   - Kısıtlamalara uygun algoritma:
     - ✅ Bir ders için birden fazla sınav saati atanamaz
     - ✅ Bir derslikte aynı anda birden fazla sınav yapılamaz
     - ✅ Öğretim üyesi müsaitliği kontrol edilir
     - ✅ Kapasite yetersizse birden fazla derslik atanır
     - ✅ Sadece HasExam=true olan dersler planlanır

5. **Program Görüntüleme:**
   - Fakülte/Bölüm bazında filtreleme
   - Detaylı görüntüleme

6. **Excel Export:**
   - Sınav programını Excel formatında dışa aktarma

## Veritabanı Şeması

### Ana Tablolar

- **Users:** Kullanıcılar (Admin, DepartmentOfficer, Lecturer, Student)
- **Roles:** Roller
- **Faculty:** Fakülteler
- **Department:** Bölümler
- **Lecturer:** Öğretim Üyeleri
- **Course:** Dersler
- **Classroom:** Derslikler
- **LecturerAvailability:** Öğretim Üyesi Müsaitlik
- **ExamSchedule:** Sınav Programı
- **ExamRoomAssignment:** Sınav Derslik Atamaları (kapasite yetersizse birden fazla derslik)

## Algoritma Açıklaması

Sınav programlama algoritması şu adımları izler:

1. **HasExam=true** olan ve henüz planlanmamış tüm dersleri alır
2. Dersleri **öğrenci sayısına göre** sıralar (büyükten küçüğe)
3. Her ders için:
   - Öğretim üyesinin müsaitlik bilgilerini kontrol eder
   - Belirtilen tarih aralığında uygun zaman slotları oluşturur
   - Her zaman slotu için:
     - Öğretim üyesinin o saatte müsait olup olmadığını kontrol eder
     - Derslik çakışmalarını kontrol eder
     - Gerekli kapasiteyi sağlayacak derslikleri bulur
     - Birden fazla derslik gerekiyorsa, öğrencileri böler
   - Uygun zaman/derslik bulunursa sınavı planlar
4. Planlanan sınavları veritabanına kaydeder

## Sorun Giderme

### Veritabanı Bağlantı Hatası

- SQL Server'ın çalıştığından emin olun
- Connection string'i kontrol edin
- SQL Server Authentication kullanıyorsanız kullanıcı adı/şifrenin doğru olduğundan emin olun

### NuGet Paket Hataları

- `Update-Package -reinstall` komutunu Package Manager Console'da çalıştırın
- Visual Studio'yu yönetici olarak çalıştırmayı deneyin

### Sayfa Bulunamadı (404) Hatası

- Route yapılandırmasını kontrol edin
- Controller ve View isimlerinin doğru olduğundan emin olun

## Geliştirici Notları

- Kodlar Türkçe yorumlarla açıklanmıştır
- Entity Framework 6 Code First yaklaşımı kullanılmıştır
- Algoritma basit bir greedy yaklaşımı kullanır (backtracking veya genetic algorithm yerine)
- Production ortamında şifrelerin hash'lenmesi önerilir (şu anda basit string karşılaştırması kullanılıyor)

## Lisans

Bu proje bir üniversite ders projesi olarak geliştirilmiştir.

---

**Not:** Bu proje SQL Server 2012 ile uyumludur. Daha yeni sürümlerle de çalışabilir, ancak test edilmemiştir.






