# ExamSchedulingSystem - Güncelleme Özeti

Bu döküman, projeye yapılan tüm güncellemeleri ve adım adım uygulama talimatlarını içermektedir.

## Yapılan Güncellemeler

### 1. Veritabanı Yapısı Güncellemeleri

#### Yeni Tablo Yapısı:

**Users Tablosu:**
- `username_or_email` (UNIQUE) - Kullanıcı adı veya e-posta
- `password_hash` - Şifre hash'i
- `role` (string) - ADMIN, INSTRUCTOR, DEPT_AUTH, STUDENT
- `created_at` - Oluşturulma tarihi

**Students Tablosu:**
- `student_no` (UNIQUE) - Sadece Students tablosunda
- `name`, `surname` - Ad ve soyad ayrı
- `department_or_program` - Bölüm/Program bilgisi
- `user_id` (NULL, FK -> Users.id, UNIQUE) - Öğrenci kayıt olduğunda dolar

**CourseStudents Tablosu:**
- `course_id`, `student_id` (composite key)
- `created_date`
- UNIQUE(course_id, student_id)

**ClassroomProximity Tablosu:**
- Derslikler arası yakınlık bilgisi
- `classroom_id1`, `classroom_id2`
- `proximity_value` - Yakınlık değeri (düşük = yakın)

#### Migration Script:
`Database/Migration_Update_Users_Students.sql` dosyası oluşturuldu.

**ÖNEMLİ:** Migration script'i çalıştırmadan önce:
1. Veritabanınızın yedeğini alın!
2. Script'i inceleyin ve COMMIT/ROLLBACK satırlarını kontrol edin.
3. Test ortamında önce deneyin.

### 2. Öğrenci Kayıt & Giriş Akışı

#### Yeni Özellikler:
- **Öğrenci Kayıt Ekranı**: `/Account/Register`
  - Öğrenci numarası + ad/soyad doğrulama (opsiyonel) + şifre ile kayıt
  - Kayıt mantığı:
    - `students` tablosunda `student_no` yoksa → uyarı
    - `user_id` doluysa → "Zaten kayıtlı" uyarısı
    - `user_id` boşsa → `users` tablosunda `role=STUDENT` kullanıcı oluştur ve `students.user_id` ile bağla

- **Öğrenci Girişi**: 
  - Mevcut login sistemi güncellendi
  - Student rolü için session'da `StudentId`, `StudentNo` bilgileri tutulur

- **Öğrenci Dashboard**:
  - Öğrenci giriş yaptıktan sonra sadece kendi derslerini görebilir
  - `course_students` üzerinden kendi derslerini bulup, sınavlarını tek ekranda görüntüler
  - `ExamSchedule/Index` endpoint'i student rolü için otomatik filtreleme yapar

### 3. Excel Import Servisi (Merged Cell Desteği)

#### Yeni Servis: `ExcelImportService`

**Özellikler:**
- ✅ Merged cell desteği - Birleştirilmiş hücreleri otomatik açma
- ✅ Header-based column matching - Sütun isimlerine göre eşleştirme (hardcode yok)
- ✅ Eksik değerleri tamamlama - Yukarıdaki/solundaki değerleri kullanma
- ✅ Course + Student + CourseStudents import - Tek seferde hepsini import etme
- ✅ Student upsert - `student_no` ile varsa güncelle, yoksa ekle
- ✅ Course upsert - `course_code` ile varsa güncelle, yoksa ekle
- ✅ Course-Student ilişkilerini kurma

**Kullanım:**
```csharp
var importService = new ExcelImportService();
var result = importService.ImportCoursesAndStudents(fileStream, departmentId);
```

**Desteklenen Sütun İsimleri (Türkçe/İngilizce):**
- Ders Kodu: "Ders Kodu", "Course Code", "CourseCode", "Kod", "Code"
- Ders Adı: "Ders Adı", "Course Name", "CourseName", "Adı", "Name"
- Öğrenci No: "Öğrenci No", "Student No", "StudentNo", "Ogrenci No", "Numara", "No"
- Öğrenci Adı: "Öğrenci Adı", "Student Name", "Name", "Adı", "First Name"
- Öğrenci Soyadı: "Soyadı", "Surname", "Last Name", "Soy Adı"
- Bölüm: "Bölüm", "Department", "Program", "Fakülte", "Faculty"

**Not:** Excel örneği daha sonra eklenecek, kod esnek yapıda hazırlandı.

### 4. Kapasite Kontrolü Geliştirmeleri

#### Yapılan İyileştirmeler:
- ✅ **Gereksiz bölme önleme**: Yeterli kapasitesi olan tek bir derslik varken sınav asla bölünmez
- ✅ **Mantık**:
  - `öğrenci_sayısı ≤ kapasite` → Tek derslik
  - `öğrenci_sayısı > kapasite` → Bölme (yakınlık bazlı)
- ✅ **Büyük derslik tercihi**: Büyük derslik varken küçük 2 dersliğe gereksiz bölme yapılmaz

**Algoritma Güncellemesi:**
`ExamSchedulingService.FindAvailableRooms()` metodu güncellendi:
1. Önce tek derslikte çözüm aranır
2. Tek derslik yeterliyse, bölme yapılmaz
3. Bölme gerekliyse, yakınlık bazlı seçim yapılır

### 5. Yakınlık Bazlı Derslik Seçimi

#### Yeni Model: `ClassroomProximity`
- Derslikler arası yakınlık bilgisi saklanır
- `proximity_value`: Düşük değer = yakın (örn: 50 = aynı kat, 100 = aynı bina, 200 = farklı bina)

#### Algoritma:
- Kapasite yetersizse sınav birden fazla dersliğe bölünebilir
- Derslik seçimi yakınlık bilgisine göre yapılır (en yakınlardan başlayarak)
- Rastgele seçim yapılmaz

**Not:** Yakınlık verilerini `ClassroomProximity` tablosuna manuel olarak eklemeniz gerekmektedir. Varsayılan olarak 1000 (uzak) değeri kullanılır.

### 6. Course.StudentCount Otomatik Hesaplama

#### Değişiklikler:
- ✅ `Course.StudentCount` artık `CourseStudents` üzerinden otomatik hesaplanır
- ✅ Veritabanında saklanmaz (NotMapped property)
- ✅ Manuel giriş kaldırıldı
- ✅ `CourseStudents.Count` üzerinden hesaplanır

**Güncelleme:** `ExamSchedulingService` içinde course'ların student count'u artık `CourseStudents` tablosundan hesaplanıyor.

## Dosya Değişiklikleri

### Yeni Dosyalar:
1. `Models/CourseStudent.cs` - Course-Student ilişkisi (CourseEnrollment yerine)
2. `Models/ClassroomProximity.cs` - Derslik yakınlık modeli
3. `Services/ExcelImportService.cs` - Excel import servisi
4. `Database/Migration_Update_Users_Students.sql` - Veritabanı migration script'i

### Güncellenen Dosyalar:
1. `Models/User.cs` - Yeni yapıya göre güncellendi
2. `Models/Student.cs` - `user_id` FK, `name/surname` ayrı alanlar
3. `Models/Course.cs` - `StudentCount` otomatik hesaplama, `CourseStudents` ilişkisi
4. `DAL/ExamSchedulingContext.cs` - Yeni modeller ve ilişkiler eklendi
5. `Controllers/AccountController.cs` - Yeni login yapısı, Register endpoint eklendi
6. `Controllers/ExamScheduleController.cs` - Student rolü için filtreleme eklendi
7. `Controllers/CourseController.cs` - CourseStudent kullanımına güncellendi
8. `Services/ExamSchedulingService.cs` - Kapasite kontrolü, yakınlık algoritması güncellendi

### Eski Dosyalar (Artık Kullanılmıyor):
- `Models/CourseEnrollment.cs` - CourseStudent ile değiştirildi (backup olarak bırakıldı)

## Kurulum Adımları

### 1. Veritabanı Migration

1. **Backup alın:**
   ```sql
   BACKUP DATABASE ExamSchedulingDB TO DISK = 'C:\Backup\ExamSchedulingDB.bak'
   ```

2. **Migration script'ini çalıştırın:**
   - SQL Server Management Studio'yu açın
   - `Database/Migration_Update_Users_Students.sql` dosyasını açın
   - Script'i inceleyin
   - `COMMIT TRANSACTION;` satırını uncomment edin
   - Script'i çalıştırın (F5)

3. **Verifikasyon:**
   ```sql
   -- Yeni tabloların oluştuğunu kontrol edin
   SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('Users', 'Students', 'CourseStudents', 'ClassroomProximity')
   
   -- Eski tabloların silindiğini kontrol edin
   SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'CourseEnrollment'
   ```

### 2. Proje Derleme

1. Visual Studio'da projeyi açın
2. NuGet paketlerini restore edin:
   ```
   Update-Package -reinstall
   ```
3. Projeyi derleyin (F6)
4. Linter hatalarını kontrol edin (yok olmalı)

### 3. Test Kullanıcıları

Mevcut admin kullanıcıları için:
- `Users` tablosunda `role` sütununu güncelleyin:
  ```sql
  UPDATE Users SET Role = 'ADMIN' WHERE UsernameOrEmail = 'admin'
  UPDATE Users SET Role = 'DEPT_AUTH' WHERE UsernameOrEmail = 'dept_officer1'
  UPDATE Users SET Role = 'INSTRUCTOR' WHERE UsernameOrEmail = 'lecturer1'
  ```

### 4. Excel Import Testi

1. Test Excel dosyası hazırlayın (örnek format):
   ```
   | Ders Kodu | Ders Adı        | Öğrenci No | Öğrenci Adı | Öğrenci Soyadı |
   |-----------|-----------------|------------|-------------|----------------|
   | CS101     | Programlama     | 2021001    | Ahmet       | Yılmaz         |
   | CS101     | Programlama     | 2021002    | Mehmet      | Demir          |
   ```

2. Excel import endpoint'ini kullanın (Controller eklenecek veya mevcut Course/UploadStudents endpoint'i güncellenecek)

### 5. Yakınlık Verilerini Ekleme

Yakınlık bilgilerini `ClassroomProximity` tablosuna ekleyin:

```sql
-- Örnek: Aynı bina, aynı kat
INSERT INTO ClassroomProximity (ClassroomId1, ClassroomId2, ProximityValue, Description)
VALUES (1, 2, 50, 'Aynı kat')

-- Örnek: Aynı bina, farklı kat
INSERT INTO ClassroomProximity (ClassroomId1, ClassroomId2, ProximityValue, Description)
VALUES (1, 3, 100, 'Farklı kat, aynı bina')

-- Örnek: Farklı bina
INSERT INTO ClassroomProximity (ClassroomId1, ClassroomId2, ProximityValue, Description)
VALUES (1, 4, 200, 'Farklı bina')
```

## Notlar ve Uyarılar

### ⚠️ ÖNEMLİ:
1. **Mevcut çalışan kodlar korundu:** Özellikle `ClassroomController`'daki Excel import (derslik kapasitesi) dokunulmadı.
2. **Password hashing:** Şu anda basit string karşılaştırması kullanılıyor. Production ortamında BCrypt veya benzeri hash algoritması kullanılmalıdır.
3. **Role enum:** Roller artık string olarak tutuluyor: `ADMIN`, `INSTRUCTOR`, `DEPT_AUTH`, `STUDENT`
4. **StudentCount hesaplama:** Artık `CourseStudents.Count` üzerinden hesaplanıyor. Eski `StudentCount` değerleri migration sırasında kaybolabilir.

### 🔄 Geri Dönüş:
Eğer sorun yaşarsanız:
1. Migration script'in sonundaki backup tablolarını kullanarak verileri geri yükleyebilirsiniz
2. `ROLLBACK TRANSACTION;` komutu ile tüm değişiklikleri geri alabilirsiniz (script çalıştırılmadan önce)

### 📝 Eksikler (Sonraki Adımlar):
1. `Register.cshtml` view dosyası oluşturulmalı
2. Excel import için controller endpoint'i eklenebilir (şu anda servis hazır, controller endpoint'i CourseController'a eklenebilir)
3. Yakınlık verilerini Excel'den import edebilme özelliği eklenebilir
4. Password hashing implementasyonu (BCrypt)

## Test Senaryoları

### 1. Öğrenci Kayıt Testi:
- [ ] Öğrenci numarası sistemde yok → Hata mesajı
- [ ] Öğrenci numarası var, user_id dolu → "Zaten kayıtlı" mesajı
- [ ] Öğrenci numarası var, user_id boş → Başarılı kayıt

### 2. Öğrenci Giriş Testi:
- [ ] Kayıt olmuş öğrenci ile giriş → Başarılı
- [ ] Sadece kendi derslerini görebilme

### 3. Excel Import Testi:
- [ ] Merged cell'ler doğru açılıyor
- [ ] Header-based matching çalışıyor
- [ ] Course + Student + CourseStudents import ediliyor

### 4. Kapasite Kontrolü Testi:
- [ ] Tek derslik yeterliyse bölme yapılmıyor
- [ ] Büyük derslik varsa küçük dersliklere bölme yapılmıyor

### 5. Yakınlık Bazlı Seçim Testi:
- [ ] Yakın derslikler öncelikli seçiliyor
- [ ] Bölme gerekliyse yakın derslikler kullanılıyor

---

**Son Güncelleme:** Tüm ana özellikler tamamlandı. Test edilmeye hazır.

