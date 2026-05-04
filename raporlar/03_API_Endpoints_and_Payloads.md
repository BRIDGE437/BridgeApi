# BridgeApi Dokümantasyon Cilt 3: API Endpoints & Payload Analysis
*(API Uç Noktaları, DTO'lar ve JSON Payload Referans Kitapçığı TR/EN - Tam Sürüm)*

Bu doküman, sistemdeki **TÜM** (C# Ana API, C# Matching API ve Python FastAPI) uç noktalarını eksiksiz olarak listeler. Frontend/Mobil geliştiricileri için doğrudan bir referans sözlüğüdir.

---

## 1. Ana Backend Uçları (Sosyal Ağ ve Kimlik Yönetimi - BridgeApi.API)
Burası sistemin kalbidir. Yatırımcıların ve Girişimlerin eşleşmekle kalmayıp, gönderi (Post) paylaşabildiği, takipleşebildiği (Follow) ve mesajlaşabildiği asıl LinkedIn benzeri altyapıdır.

### 1.1. `AuthController` (Giriş ve Kayıt)
- **`POST /api/v1/auth/register`**: Sisteme yeni bir kullanıcı (Girişim, Yatırımcı veya Standart) kaydeder.
  - **Body:** `{ "email": "x@x.com", "password": "...", "fullName": "..." }`
- **`POST /api/v1/auth/login`**: JWT Access Token ve Refresh Token döner.
- **`POST /api/v1/auth/refresh-token`**: Süresi dolan token'ı yeniler.

### 1.2. `PostController` & `PostCommentController` (Gönderi Sistemi)
- **`POST /api/v1/posts`**: Kullanıcının ağında paylaşması için yeni bir metin veya medya gönderisi oluşturur.
  - **Body:** `{ "content": "Looking for seed investment!", "mediaUrl": "..." }`
- **`GET /api/v1/posts`**: Kullanıcının takip ettiklerine göre şekillenen zaman tüneli (Feed).
- **`POST /api/v1/posts/{id}/like`**: Gönderiyi beğenir.
- **`POST /api/v1/posts/{postId}/comments`**: Gönderiye yorum yapar.

### 1.3. `ConnectionController` & `FollowController` (Ağ Kurma)
- **`POST /api/v1/connections/request`**: Başka bir girişimciye veya yatırımcıya "Bağlantı/Arkadaşlık İstediği" gönderir.
- **`POST /api/v1/connections/accept/{id}`**: Bağlantı isteğini kabul eder.
- **`POST /api/v1/follows/{targetUserId}`**: Bir kullanıcıyı veya şirketi tek taraflı takibe alır (Bildirim düşmesi için).

### 1.4. `MessageController` (Sohbet)
- **`POST /api/v1/messages`**: Başka bir kullanıcıya doğrudan mesaj (DM) atar.
  - **Body:** `{ "receiverId": "guid", "content": "Hi, saw your pitch deck!" }`
- **`GET /api/v1/messages/conversation/{userId}`**: İki kişi arasındaki geçmiş mesajlaşmaları sayfalı (pagination) döner.

### 1.5. `UserProfileController` & `UserController`
- **`PUT /api/v1/userprofiles`**: Kullanıcının avatarını, başlığını (Headline) ve bio'sunu günceller.
- **`GET /api/v1/users/search`**: Sistemdeki 1726 girişimi veya yatırımcıyı isim/sektör bazlı aramak için kullanılır.

---

## 2. Veri Aktarım & Vektörizasyon Uçları (Import API)

### 2.1. `POST /api/v1/Import/startups`
- **Görev:** Ana CSV dosyasındaki verileri MD5 tekilleştirmesiyle sisteme AppUser (Kullanıcı) olarak kaydeder. E-posta çakışmalarını önler.
- **Response `200 OK`:** `{"message": "Data imported successfully", "totalProcessed": 1726}`

### 2.2. `POST /api/v1/Indexing/trigger`
- **Görev:** Veritabanındaki yeni/güncellenmiş girişimleri AI servisine yollayıp `vector(384)` eklentisini doldurur.
- **Parametre:** `?batchSize=100`

---

## 3. Eşleştirme Motoru ve Etkinlikler (Matching API)

### 3.1. `POST /api/v1/events`
- **Görev:** Yeni bir "Yatırımcı" veya "Networking" etkinliği oluşturur.
- **Body:** `{ "title": "B2B Tech Summit", "scheduledAt": "2026-12-01T15:00:00Z", "eventType": "Networking" }`

### 3.2. `POST /api/v1/match/event/{eventId}/join`
- **Görev:** Bir kullanıcının bu etkinliğin havuzuna dahil olmasını sağlar.
- **Parametreler:** `?participantId=GUID&participantType=StartupProfile`

### 3.3. `POST /api/v1/match/ai-powered`
- **Görev:** Yatırımcıyı sistemdeki en uygun girişimlerle `Kural + Vektör + LLM Bonus` formülüyle eşleştirir.
- **Body:** `{ "investorId": "investor_guid", "topN": 10 }`
- **Response Özeti:** Sıralanmış (Rank) liste, `100` üzerinden Total Score, skor kırılımları (`Sector`, `Semantic` vs.) ve Gemini'nin Türkçe açıkladığı `AiReason`.

### 3.4. `POST /api/v1/match/event/match-startup` (B2B Eşleşmesi)
- **Görev:** Networking etkinliğindeki girişimleri birbirleriyle Sinerji/Partnerlik açısından eşleştirir.

---

## 4. Yapay Zeka Servisi (Python FastAPI)
*Bu uçlar dış dünyaya kapalıdır, sadece C# çağırır.*

### 4.1. `POST /api/v1/semantic-match`
- **Görev:** Vektör benzerliğini (`<=>`) pgvector üzerinde hesaplar, Gemini ile mantıksal uyumu denetler.
- **Request Body:**
```json
{
  "investor_text": "Fintech and AI investments...",
  "startups": [ { "id": "guid_1", "text": "Fintech startup description..." } ],
  "use_llm": true,
  "mode": "investor_startup"
}
```

### 4.2. `POST /api/v1/semantic-match/startup-startup`
- **Görev:** İki şirketin ortak ürün çıkarma ihtimalini (B2B Sinerji) hesaplar.
- **Request Body:** `{ "source_startup_id": "guid_a", "target_startup_ids": ["guid_b"], "use_llm": true, "mode": "startup_startup" }`

### 4.3. `POST /api/v1/index-startups`
- **Görev:** C# tarafından gönderilen 100'erlik paketleri 384-boyutlu matrislere çevirip kaydeder.
