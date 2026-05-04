# Startup Fingerprint System - Implementation Plan (TR/EN)

## 1. Amaç / Purpose
Veritabanında mükerrer (duplicate) kayıt oluşmasını engellemek için girişimleri "Web Sitesi + Şirket Adı" kombinasyonundan oluşan benzersiz bir parmak izi (Fingerprint) ile takip etmek.
Ensure data uniqueness and prevent duplicates by tracking startups using a unique "Fingerprint" generated from "Website + Company Name" combination.

## 2. Teknik Detaylar / Technical Details

### 2.1 Schema Updates
- **Entity**: `StartupProfile`
- **Field**: `ExternalFingerprint` (string, Max: 64 chars)
- **Database**: Add index to `ExternalFingerprint`.

### 2.2 Normalization Rules
1. Remove `https://`, `http://`, `www.`.
2. Remove trailing slashes `/`.
3. Convert to lowercase.
4. Clean Company Name (remove special chars, lowercase).

### 2.3 Fingerprint Generation
- `Fingerprint = MD5(NormalizedWebsite + CleanName)`

## 3. Uygulama Adımları / Execution Steps

| Step | Action | Status |
| :--- | :--- | :--- |
| 1 | Update `StartupProfile.cs` with `ExternalFingerprint` | [COMPLETED] |
| 2 | Create and Apply EF Core Migration | [COMPLETED] |
| 3 | Update `CsvImportService` with Fingerprint logic | [COMPLETED] |
| 4 | Implement Index-based Mapping (Ditch header names) | [COMPLETED] |
| 5 | Implement Culture-Invariant (ToLowerInvariant) logic | [COMPLETED] |
| 6 | Add `ContactEmails` & `NeedsManualReview` to Entity | [COMPLETED] |
| 7 | Fix Identity Email Override Issue (MakeUniqueEmail) | [COMPLETED] |
| 8 | Clear and Re-import data to verify 1726 count | [COMPLETED] |

## 4. Kritik Teknik Notlar / Critical Technical Notes
- **Index-based Mapping**: CSV kolon isimlerine (BOM veya Casing sorunları nedeniyle) güvenilmez. İndeksler (0, 1, 2...) kullanılarak veri çekilir.
- **Culture Invariant**: Karakter normalizasyonunda `ToLowerInvariant()` kullanılır (Türkçe 'ı/i' sorununu önler).
- **ID Strategy**: Scraping ID'ler DB'ye kaydedilmez; veritabanı kendi Guid'lerini üretir. Tekilleştirme sadece `ExternalFingerprint` üzerinden yapılır.
- **Identity Email Override**: İki farklı şirket (farklı Fingerprint) aynı e-posta adresiyle (örn. Wix/Sentry) gelirse, Identity kuralı gereği ikinci şirket eskisinin üzerine yazılıyordu. Bunu çözmek için çakışan maillere `+ID` eklenerek (`sentry+40157@...`) eşsiz hale getirildi. (Böylece 1661 yerine 1726 hedefine ulaşıldı).
- **Gerçek Maillerin Korunması**: Identity için uydurma mail üretilse bile, CSV'deki orijinal tüm mailler `StartupProfile` tablosundaki `ContactEmails` alanında virgülle ayrılarak eksiksiz saklanır.
- **Geriye Dönük Hata Takibi (NeedsManualReview)**: Scraper hataları (Wix, Sentry) veya mail çakışmaları tespit edildiğinde sistem bu şirketlere `NeedsManualReview = true` bayrağı koyar. Böylece adminler sonradan `SELECT * FROM StartupProfiles WHERE NeedsManualReview = true` sorgusuyla bu şirketleri bulup gerçek iletişim bilgilerini manuel veya botla güncelleyebilir.
