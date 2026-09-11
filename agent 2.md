---
document_type: AI Agent Project Instructions
project: Lab Consumable Expiry Tracker
version: 1.2.0
created_at: 2026-09-09
updated_at: 2026-09-10
aligned_srs_version: 1.2.1
language: id-ID
backend_target: ASP.NET Core Web API (.NET 8)
frontend_target: React
database: PostgreSQL
orm: Entity Framework Core
identity: ASP.NET Core Identity
authentication: JWT Bearer
sources:
  - skill(1).md
  - diagram.mmd
  - srs.md
  - PRD_Lab_Consumable_Expiry_Tracker_v1_0(2).pdf
status: Ready for agent handoff; unresolved product decisions explicitly listed
---

# Instruksi AI Agent — Lab Consumable Expiry Tracker

## 1. Tujuan dan cara menggunakan dokumen

Gunakan dokumen ini ketika mengimplementasikan, mereview, memperbaiki, atau menguji Lab Consumable Expiry Tracker. Baca `agent.md`, `skill(1).md`, `diagram.mmd`, dan `srs.md` sebelum mengubah kontrak domain. Gunakan PRD untuk memeriksa scope dan acceptance criteria sumber.

Tujuan produk: mencegah bahan laboratorium expired atau tidak valid dikonsumsi pada Job, memilih Lot eligible menggunakan FEFO, menjaga stok, mencatat Consumption/Disposal, dan menghasilkan sinyal low stock yang benar.

Dokumen ini adalah panduan kerja proyek, bukan implementasi aplikasi. Nama file `agent.md` mengikuti permintaan pemilik proyek. Saat menyerahkan tugas ke agent lain, instruksikan agent tersebut secara eksplisit untuk membacanya; jangan mengasumsikan semua alat memuat nama file ini otomatis.

**MUST / MUST NOT** berarti wajib / dilarang. **SHOULD** berarti rekomendasi. **OPEN** berarti belum diputuskan; **PROPOSED** berarti usulan yang tidak boleh dianggap persetujuan produk.

## 2. Otoritas sumber dan penanganan konflik

| Sumber | Otoritas |
| --- | --- |
| Instruksi eksplisit terbaru pemilik proyek | Perubahan scope/keputusan yang perlu dicatat |
| PRD v1.0 | Tujuan produk, scope, prioritas, dan acceptance criteria |
| `diagram.mmd` | Kelas, nama member, tipe atribut, enum, relasi domain kanonis |
| `skill(1).md` | Aturan implementasi, expiry, FEFO, transaction, dan kualitas |
| `srs.md` | Requirement ID, traceability, use case, test, serta keputusan terbuka |
| `agent.md` | Prosedur kerja dan rekonsiliasi sumber untuk agent |

Ikuti aturan `skill(1).md`: bila prosa ambigu, pertahankan kontrak domain pada diagram. Matriks ini tidak membolehkan agent menutupi kontradiksi nyata dengan memilih sumber sesuka hati. Laporkan konflik, identifikasi dampaknya, dan lanjutkan bagian yang independen.

### 2.1 Rekonsiliasi terhadap SRS v1.2.1

Revisi ini menyelaraskan agent.md dengan SRS v1.2.1 dan keputusan pemilik proyek: Item memiliki Lot langsung, Lot menjadi unit inventaris terkecil, serta stack dan struktur backend telah ditetapkan. SRS tidak diubah dalam revisi agent.md ini. Diagram dan PDF sumber tidak tersedia untuk diverifikasi ulang; rincian tipe yang dikaitkan dengan diagram di bawah merupakan kontrak yang diwarisi dari agent.md v1.1.0, bukan hasil pembacaan ulang diagram.

| Area | Status sumber | Instruksi |
| --- | --- | --- |
| Model inventaris | SRS COMPAT-01: Lot langsung di bawah Item | Stok, expiry, lokasi dan status berada pada Lot; transaksi memakai LotId |
| Stack dan folder | SRS §3.1–3.4 sudah menetapkan teknologi dan penempatan | Ikuti §8 dokumen ini; jangan membuka ulang keputusan stack |
| Ketersediaan skill | SRS telah menggunakan `skill(1).md` | Baca file aktual; jangan menganggap lampiran belum tersedia |
| Entity/foreign/actor ID | Agent v1.1.0 mencatat Guid dari diagram; SRS D-04 masih OPEN | Pertahankan kontrak yang sudah ada; verifikasi diagram dan sinkronkan keputusan sebelum schema final (G-05) |
| ExpiryDate | Agent v1.1.0 mencatat DateOnly; SRS masih memakai formula/fixture instant | Jangan mengganti tipe atau menyalin perbandingan lintas tipe; selesaikan G-01/G-05 |
| Administrative enum | SRS dan skill memakai LotAdministrativeStatus | File Models/Enums/LotAdministrativeStatus.cs; AdministrativeStatus adalah properti Lot |
| RowVersion | Agent v1.1.0 mencatat byte[]; SRS menyebut token opaque | Pertahankan kontrak existing; mapping PostgreSQL/EF Core dan regenerasi token masih D-03 |
| Timestamp | Kontrak lama: DateTimeOffset untuk event; StartedAt/CompletedAt nullable | Tidak diubah oleh keputusan Lot-only; verifikasi bersama diagram sebelum perubahan tipe |
| Metode domain | Skill menetapkan Consume, Dispose, Block, Start, Complete | Use case SRS bukan izin otomatis mengganti public API domain |
| Fixture SRS | ID angka dan expiry timestamp merupakan ilustrasi | Adaptasi setelah tipe dan cutoff terkonfirmasi; jangan menjadikannya keputusan schema |

`DateOnly` tidak mempunyai jam atau offset. PRD mensyaratkan UTC untuk keputusan waktu tetapi belum menjelaskan kapan tanggal expiry berhenti valid. Dilarang membandingkan langsung `DateTimeOffset now` dengan `DateOnly ExpiryDate` atau menganggap cutoff tertentu telah disepakati. Lihat bagian 5 dan keputusan G-01.

## 3. Batas scope yang wajib dijaga

- Item memiliki nol atau lebih Lot secara langsung.
- Lot adalah unit inventaris terkecil dan bukan sekadar wadah: Lot.Id adalah identitasnya, sedangkan LotNumber, InitialQuantity, RemainingQuantity, ExpiryDate, ReceivedAt, AdministrativeStatus, StorageLocation, dan RowVersion disimpan langsung pada Lot.
- Consumption, Disposal, dan LotAllocation selalu merujuk langsung ke LotId. Setiap expiry berbeda direpresentasikan sebagai Lot berbeda.
- Jangan menambah lapisan inventaris di bawah Lot, warehouse hierarchy, atau operasi pemecahan Lot.
- Jangan menambah purchase order, supplier management, keuangan, geofencing, data pasien, atau integrasi ERP/perangkat sebagai dependensi MVP.
- Prioritaskan F-01 sampai F-08 dan ringkasan stok MVP; quick actions F-09 adalah Should; ekspor/integrasi F-10 menunggu keputusan lanjutan.
- Aplikasi MVP memiliki tepat dua role: `LabOperator` dan `WarehouseAdmin`.
- Stakeholder seperti Product Owner, Engineering, QA, Lab Manager, atau Procurement bukan role login tambahan.
- Terapkan permission matrix pada bagian 3.1 di server menggunakan ASP.NET Core Identity dan JWT; kebijakan akun/token yang belum diputuskan tetap mengikuti D-11.

### 3.1 Dua role dan permission matrix

Gunakan nama kanonis `LabOperator` dan `WarehouseAdmin`; nama tampilannya adalah Lab Operator dan Admin Gudang. Dilarang membuat role ketiga untuk kebutuhan MVP tanpa perubahan requirement.

| Operasi | LabOperator | WarehouseAdmin |
| --- | :---: | :---: |
| Baca dashboard, Item/Lot, status, stok, dan audit | Ya | Ya |
| Create/update/deactivate Item | Tidak | Ya |
| Register/update administratif Lot | Tidak | Ya |
| Create/Start/Complete/Cancel Job | Ya | Tidak |
| Allocate FEFO dan confirm Consumption | Ya | Tidak |
| Dispose Lot | Tidak | Ya |
| Block, quarantine, dan release Lot | Tidak | Ya |
| Login dan melihat profil sendiri | Ya | Ya |
| Mengelola akun/assignment role | Belum ditetapkan | Belum ditetapkan |

Agent MUST:

- menegakkan izin pada Application/API/server, termasuk ketika endpoint dipanggil tanpa UI;
- menggunakan actor terverifikasi dari application context untuk Consumption/Disposal;
- mengembalikan forbidden tanpa perubahan domain, audit sukses, atau partial commit untuk operasi yang tidak diizinkan;
- tidak menyimpulkan Admin Gudang boleh melakukan Consumption hanya karena memiliki hak administrasi stok;
- tidak menyimpulkan Lab Operator boleh mengubah stok langsung di luar metode Consumption;
- mempertahankan pemisahan tugas tersebut dalam command handler, endpoint, test, dan dokumentasi.

Satu akun MAY memiliki kedua role sesuai kebijakan multi-role yang disetujui. Kombinasi tersebut tetap memakai dua role kanonis dan tidak membentuk role baru. ASP.NET Core Identity dan JWT sudah final; provisioning akun, pemberi role, dukungan multi-role pada aplikasi, dan kebijakan sesi/token masih OPEN D-11. Nama Scientist pada DTO tidak menambah role ketiga atau otomatis mengizinkan pengelolaan akun.

## 4. Kontrak domain kanonis

### 4.1 Atribut

Tabel tipe berikut dipertahankan dari agent.md v1.1.0. Status verifikasi dan perbedaan terhadap SRS dicatat pada §2.1 dan G-05; revisi Lot-only tidak mengubah tipe data tersebut secara sepihak. Model User untuk ASP.NET Core Identity adalah model persistence terpisah, bukan lapisan inventaris atau dependensi domain.

| Tipe | Atribut dan tipe C# |
| --- | --- |
| Item | `Guid Id`, `string Code`, `string Name`, `UnitOfMeasure BaseUnit`, `decimal MinimumStock`, `int ExpiringSoonDays` |
| Lot | `Guid Id`, `Guid ItemId`, `string LotNumber`, `decimal InitialQuantity`, `decimal RemainingQuantity`, `DateOnly ExpiryDate`, `DateTimeOffset ReceivedAt`, `LotAdministrativeStatus AdministrativeStatus`, `string StorageLocation`, `byte[] RowVersion` |
| Job | `Guid Id`, `string JobNumber`, `JobStatus Status`, `DateTimeOffset? StartedAt`, `DateTimeOffset? CompletedAt` |
| Consumption | `Guid Id`, `Guid JobId`, `Guid LotId`, `decimal Quantity`, `DateTimeOffset ConsumedAt`, `Guid ConsumedBy` |
| Disposal | `Guid Id`, `Guid LotId`, `decimal Quantity`, `string Reason`, `DateTimeOffset DisposedAt`, `Guid DisposedBy` |
| LotAllocation | `Guid LotId`, `decimal Quantity` |

Jangan menambahkan atribut persisted atau mengganti tipe kanonis diam-diam. Teknik encapsulation, constructor/factory, private setter, dan navigation mapping dapat mengikuti pola repository selama tidak mengubah makna bisnis. Diagram bukan instruksi membuat seluruh setter dapat dimodifikasi bebas oleh UI.

### 4.2 Enum

```text
UnitOfMeasure          : Milliliter, Gram, Unit, Vial
LotAdministrativeStatus: Active, Quarantined, ManuallyBlocked, Disposed
ExpiryCondition        : Valid, ExpiringSoon, Expired
JobStatus              : Draft, InProgress, Completed, Cancelled
UserRole               : LabOperator, WarehouseAdmin
```

Dilarang menyatukan dimensi status tersebut atau menambah `Expired` pada `LotAdministrativeStatus`. Low stock adalah kondisi Item, bukan status administratif Lot.

Nama file enum status MUST `Models/Enums/LotAdministrativeStatus.cs`, sesuai nama tipenya. UserRole hanya mendefinisikan nama role; membership otoritatif dikelola ASP.NET Core Identity, bukan enum sebagai sumber data kedua.

### 4.3 Metode dan tanggung jawab

| Kontrak | Tanggung jawab |
| --- | --- |
| `Item.IsLowStock(totalQuantity) -> bool` | Bandingkan aggregate usable quantity dengan MinimumStock |
| `Lot.GetExpiryCondition(now, warningDays) -> ExpiryCondition` | Evaluasi tanggal/waktu dengan kebijakan expiry yang telah disepakati |
| `Lot.IsEligible(now) -> bool` | Active, remaining positif, belum expired |
| `Lot.Consume(quantity, jobId, now) -> Consumption` | Validasi domain, kurangi stok, hasilkan Consumption |
| `Lot.Dispose(quantity, reason, now) -> Disposal` | Validasi domain, kurangi stok, hasilkan Disposal |
| `Lot.Block(reason) -> void` | Ubah status menjadi ManuallyBlocked sesuai aturan |
| `Job.Start(now) -> void` | Draft menjadi InProgress, isi StartedAt |
| `Job.Complete(now) -> void` | InProgress menjadi Completed, isi CompletedAt |
| `LotSelectionService.Allocate(lots, requestedQuantity, now) -> IReadOnlyList<LotAllocation>` | Pilih Lot FEFO secara deterministik tanpa akses database/UI |
| `ILotRepository.GetCandidatesAsync(itemId)` | Muat kandidat Lot untuk satu Item |
| `ILotRepository.GetByIdAsync(id)` | Muat Lot tertentu |
| `ILotRepository.AddAsync(lot)` | Tambah Lot |
| `IUnitOfWork.SaveChangesAsync()` | Commit quantity dan audit bisnis secara atomik |

Parameter quantity menggunakan decimal, identifier menggunakan Guid, warningDays menggunakan int, dan now berasal dari TimeProvider. Tipe parameter ini merupakan penjabaran dari atribut dan kontrak waktu sumber; diagram tidak menuliskan seluruh parameter secara typed.

Diagram menuliskan return logis repository/UoW, bukan signature C# async lengkap. **PROPOSED:** gunakan `Task<IReadOnlyList<Lot>>`, `Task<Lot?>`, `Task`, dan `Task<int>` untuk keempat operasi async, beserta CancellationToken jika mengikuti konvensi repository. Konfirmasi kebijakan not-found dan interface sebelum mengubah public contract yang sudah ada. Jangan membuat metode async palsu dengan return non-awaitable hanya karena menyalin notasi diagram.

Relasi UoW menuju Consumption pada diagram tidak mempersempit atomicity: `skill(1).md` juga mewajibkan Disposal commit bersama perubahan Lot.

## 5. Aturan waktu dan status

1. Application MUST menggunakan `.NET 8 TimeProvider.GetUtcNow()` untuk mengambil waktu keputusan. Berikan nilai `now` itu ke domain.
2. Domain MUST tetap deterministik: tidak membaca `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, atau jam mesin langsung.
3. Hubungan LotSelectionService dengan TimeProvider pada diagram dipenuhi dengan waktu terkontrol yang masuk melalui parameter `now`; jangan membaca jam kedua di dalam Allocate.
4. Persist `DateOnly ExpiryDate` sebagai tanggal. Timestamp event disimpan sebagai instant UTC; tampilan lokal pada boundary UI.
5. Tentukan policy cutoff DateOnly sebelum menyelesaikan expiry engine. Dua interpretasi berikut berbeda dan belum disetujui:

| Interpretasi kandidat | Contoh ExpiryDate 2026-09-10 dalam kalender UTC |
| --- | --- |
| Expired sejak awal tanggal tercantum | Cutoff 2026-09-10T00:00:00Z |
| Masih valid sepanjang tanggal tercantum | Cutoff 2026-09-11T00:00:00Z |

Keduanya hanya ilustrasi keputusan G-01. Jangan menerapkan salah satunya sebagai default produk tersembunyi. Jangan mengganti date dengan timestamp untuk menghindari keputusan ini. Policy yang disepakati harus tetap murni dan eksplisit, bukan mengandalkan timezone mesin.

Setelah cutoff resmi ditetapkan, expired berlaku ketika `now >= cutoff`. Konfirmasi pula warning window memakai selisih tanggal kalender atau durasi 24 jam serta apakah boundary inklusif. Test wajib menggunakan policy yang sama.

### 5.1 Eligibility

```text
eligible =
    AdministrativeStatus == Active
    AND RemainingQuantity > 0
    AND notExpiredAccordingToApprovedDatePolicy(ExpiryDate, now)
```

ExpiringSoon tetap eligible jika kondisi lain terpenuhi. Expired tidak perlu memutasi persisted AdministrativeStatus. Evaluasi ulang pada baca status, alokasi, dan konsumsi aktual. Lot yang valid saat rekomendasi dapat ditolak saat konsumsi jika kemudian expired.

Consumption yang telah berhasil sebelum expiry tetap menjadi riwayat valid setelah Lot tersebut expired; jangan menghapus atau membatalkan audit historis karena waktu berlalu.

### 5.2 Low stock

Hitung `usableStock` sebagai jumlah RemainingQuantity semua Lot eligible milik satu Item pada waktu evaluasi yang sama. Item tanpa Lot eligible mempunyai usableStock nol. Jangan menjumlahkan expired, blocked, quarantined, disposed, atau kosong sebagai stok usable.

`Item.IsLowStock(usableStock)` membandingkan nilai ini dengan MinimumStock. Operator `<` atau `<=` masih OPEN SRS D-06; usulan SRS `<` belum keputusan final.

## 6. Algoritma FEFO

Lakukan langkah berikut secara berurutan:

1. Validasi requestedQuantity positif dan request merujuk satu Item.
2. Muat kandidat melalui `ILotRepository.GetCandidatesAsync(itemId)`.
3. Ambil waktu keputusan dan evaluasi ulang eligibility semua kandidat.
4. Keluarkan Lot expired, empty, Quarantined, ManuallyBlocked, Disposed.
5. Urutkan `ExpiryDate ASC`, `ReceivedAt ASC`, kemudian `Id ASC` sebagai tie-breaker stabil.
6. Ambil `min(RemainingQuantity, quantityYangMasihDibutuhkan)` dari setiap Lot sesuai urutan.
7. Jika total eligible stock kurang, gagalkan operasi tanpa partial persistence.
8. Kembalikan LotAllocation dengan quantity positif, total sama dengan request, dan LotId langsung.

`Id` bertipe Guid. Pilih dan dokumentasikan satu comparator deterministik; hasil tidak boleh bergantung urutan query database atau enumeration yang kebetulan. **PROPOSED:** bila sorting dilakukan dalam memori, gunakan comparator Guid .NET yang sama pada semua jalur dan test; jangan mengasumsikan ordering native database identik.

Allocate tidak mengurangi stok, tidak membuat Consumption, dan tidak menjadi reservasi. Jangan memakai preview sebagai bukti stok masih tersedia pada commit konsumsi. Kandidat lintas Item atau duplikat perlu ditangani secara eksplisit oleh kontrak application; jangan mencampur unit.

## 7. Konsumsi, disposal, audit, dan concurrency

### 7.1 Invariant

- `InitialQuantity` disimpan saat registrasi dan tidak dihitung ulang setelah konsumsi/disposal.
- Pada registrasi, `RemainingQuantity = InitialQuantity`.
- RemainingQuantity tidak pernah negatif; operasi MVP hanya mengurangi stok.
- Quantity konsumsi/disposal MUST > 0 dan <= RemainingQuantity.
- Lot Disposed tidak boleh dikonsumsi maupun didispose ulang.
- Konsumsi juga menolak expired, Quarantined, ManuallyBlocked, dan empty.
- Disposal memerlukan Reason; aturan transisi status partial/full disposal masih perlu dipastikan.

### 7.2 Batas transaksi

1. Pastikan actor terautentikasi dan berwenang di Application.
2. Muat Lot/Job yang diperlukan beserta version token dan state terkini.
3. Validasi command, Job policy yang telah disetujui, quantity, dan versi yang diharapkan.
4. Ambil waktu konsumsi aktual, lalu evaluasi eligibility; jangan memakai waktu preview.
5. Panggil metode domain untuk menghasilkan perubahan quantity dan Consumption/Disposal.
6. Pastikan actor audit lengkap sebelum persistence; lihat G-02.
7. Track perubahan Lot dan seluruh record terkait dalam UoW yang sama.
8. Commit atomik. Konflik atau kegagalan menyebabkan rollback seluruh command.

Untuk command multi-Lot, seluruh deduction dan audit harus berada pada satu transaction boundary. Jangan melakukan SaveChanges per Lot. Setelah kegagalan, jangan memakai tracked entity yang sudah berubah untuk commit berikutnya tanpa memulihkan/memuat ulang state.

### 7.3 Concurrency

- Teruskan ExpectedRowVersion dari update DTO/kontrak conditional update ke pemeriksaan persistence.
- Bandingkan nilai byte token, bukan reference array.
- Token harus berubah pada setiap perubahan Lot yang relevan, termasuk block/quarantine.
- Jadikan pemeriksaan token bagian dari update database atomik; pemeriksaan di memori saja tidak cukup menghadapi race condition.
- Mismatch menghasilkan conflict yang dapat ditindaklanjuti pengguna; tidak ada silent overwrite.
- Jangan retry konsumsi non-idempotent secara otomatis setelah hasil commit tidak pasti. Kunci kebijakan retry/idempotency pada SRS D-09.

### 7.4 Identitas audit dan kekosongan kontrak

Diagram mengharuskan `Guid ConsumedBy` dan `Guid DisposedBy`, tetapi metode Consume/Dispose tidak menerima actor. Jangan mengisi Guid.Empty, membuat Guid acak, mengambil actor dari payload pengguna, atau memasukkan HTTP/authentication context ke Domain.

**OPEN G-02:** tentukan cara application memberikan actor terverifikasi sebelum commit sambil mempertahankan signature kanonis. Jika diperlukan factory/internal completion method atau perubahan signature, dokumentasikan kontraknya terlebih dahulu. Persistence MUST menolak audit yang belum lengkap. Jangan mengklaim alur end-to-end selesai selama identitas actor belum terselesaikan.

`Block(reason)` menerima reason tetapi Lot tidak memiliki BlockReason atau entitas riwayat status. Jangan menyimpan reason ke StorageLocation atau membuat Disposal palsu. Penyimpanan/penggunaan reason block adalah OPEN G-03.

Consumption dan Disposal adalah audit bisnis yang persisten. Log teknis tidak menggantikannya. Audit harus dapat ditelusuri melalui LotId dan JobId untuk Consumption, serta LotId dan Reason untuk Disposal.

## 8. Arsitektur, persistence, dan batas perubahan

Gunakan Clean Architecture secara arah dependensi, Repository Pattern, dan Dependency Injection dengan struktur backend pada SRS §3.2–3.4. Susunan folder dalam satu proyek Web API tidak sama dengan isolasi assembly; periksa dependensi nyata. Jangan mengganti struktur yang ditetapkan dengan pemisahan proyek lain tanpa keputusan. Jika source existing berbeda, petakan dampaknya dan lakukan perubahan dalam scope tugas, bukan reorganisasi luas.

| Lapisan | Boleh memuat | Dilarang |
| --- | --- | --- |
| Domain | Entitas, enum, invariant, FEFO, aturan waktu murni | DbContext, UI, HttpContext, akses jaringan, jam sistem langsung |
| Application | Use case, repository/UoW abstraction, mapping/DTO sesuai pola proyek, TimeProvider, actor context abstraction | Dependensi ke Infrastructure/UI konkret |
| Infrastructure | Repository/UoW implementation, EF mapping, concurrency, identity/logging adapter | Mengganti aturan domain agar sesuai keterbatasan database |
| Presentation | Transport, form, tampilan, pemetaan error | Menjadi satu-satunya pengaman stock/expiry/permission |

- Backend ASP.NET Core Web API pada .NET 8; frontend React melalui HTTP/JSON; jangan menaikkan versi framework tanpa instruksi atau keputusan proyek.
- PostgreSQL + EF Core sudah dipilih. Map Item–Lot langsung, simpan stok/expiry pada Lot, dan gunakan LotId untuk relasi transaksi.
- Quantity memakai decimal dan kolom numeric dengan precision/scale eksplisit; jangan memilih scale tanpa memeriksa kebutuhan unit.
- `byte[] RowVersion` tidak otomatis memiliki perilaku concurrency hanya karena namanya RowVersion. Pilih mapping dan mekanisme regenerasi token yang kompatibel dengan provider, lalu buktikan lewat integration test.
- DTO dan AutoMapper profile mengikuti model kanonis; semua referensi inventory menggunakan LotId.
- Jangan menerima edit bebas RemainingQuantity atau InitialQuantity untuk melewati Consume/Dispose.
- Lindungi integritas foreign key dan riwayat audit; jangan menghapus histori melalui cascade tanpa keputusan produk.
- Library logging belum ditentukan oleh lampiran. Ikuti pilihan repository bila sudah ada; jangan membangun framework logging sendiri sebagai prasyarat.

### 8.1 Penempatan file backend

Root backend adalah `LabConsumableExpiryTracker/`. Path berikut relatif terhadap root tersebut dan merujuk baseline SRS §3.2. Ini instruksi penempatan, bukan klaim bahwa file sudah diimplementasikan.

| Path | Isi dan tanggung jawab |
| --- | --- |
| `Common/Results/ServiceResult.cs` | Hasil use case, data dan kode kegagalan; bebas HTTP, EF Core dan Identity |
| `Configurations/JwtSettings.cs` | Konfigurasi JWT; nilai rahasia dari konfigurasi aman, bukan source code |
| `Controllers/AuthController.cs`, `Controllers/LotController.cs` | Method HTTP, authorization dan pemetaan hasil; panggil service abstraction |
| `Data/Interfaces/IDbInitializer.cs` | Kontrak initializer infrastructure |
| `Data/AppDbContext.cs`, `Data/DbInitializer.cs`, `Data/UserSeeder.cs` | Persistence EF Core, orkestrasi inisialisasi dan seed akun/role |
| `DTOs/Auth/AuthResponseDto.cs`, `CreateScientistRequestDto.cs`, `LoginRequestDto.cs`, `ScientistResponseDto.cs`, `UpdateScientistRequestDto.cs` | Kelima file berada dalam DTOs/Auth; request/response aman tanpa model Identity mentah |
| `DTOs/Lots/` | DTO consumable/CRUD Lot dan command dengan ExpectedRowVersion sesuai kebutuhan |
| `Mapping/MappingProfile.cs` | AutoMapper; hanya field yang diizinkan, tanpa overwrite bebas stok/actor/version |
| `Migrations/` | Riwayat migration EF Core untuk PostgreSQL dan constraint inventaris |
| `Models/Enums/ExpiryCondition.cs`, `JobStatus.cs`, `LotAdministrativeStatus.cs`, `UnitOfMeasure.cs`, `UserRole.cs` | Kelima file berada dalam Models/Enums; status tetap terpisah |
| `Models/Consumption.cs`, `Disposal.cs`, `Item.cs`, `Job.cs`, `Lot.cs`, `User.cs` | Keenam file berada dalam Models; User merupakan model Identity, bukan domain inventaris |
| `Repositories/Interfaces/ILotRepository.cs`, `Repositories/Interfaces/IUnitOfWork.cs` | Abstraction persistence; jangan membocorkan DbSet/DbContext/query provider |
| `Repositories/LotRepository.cs` | Orkestrasi LINQ EF Core dan tracking, tanpa commit mandiri per Lot |
| `Services/Interfaces/IIdentityService.cs`, `Services/IdentityService.cs` | Kontrak dan adapter Identity/JWT; implementasi secara logis infrastructure |
| `Validators/Auth/CreateScientistRequestValidator.cs`, `LoginRequestValidator.cs`, `UpdateScientistRequestValidator.cs` | Ketiga file berada dalam Validators/Auth; FluentValidation untuk input |

Komponen pelengkap mengikuti SRS §3.4: `Models/LotAllocation.cs`, `Services/LotSelectionService.cs`, `Validators/Lots/`, serta usulan `Services/Interfaces/ILotService.cs`, `Services/LotService.cs`, dan `Repositories/UnitOfWork.cs`. File bootstrap `Program.cs`, proyek `.csproj`, dan konfigurasi nonsecret berada di root backend. Controller/service/DTO Item, Job, Consumption, Disposal dan audit ditambahkan dalam folder sejenis sesuai requirement; baseline tidak membatasi scope hanya pada Auth dan Lot.

React dan proyek test berada di luar root backend. Nama folder frontend, tooling dan struktur internalnya belum dikunci; jangan mengarangnya sebagai keputusan produk.

### 8.2 Batas layer dan registrasi dependency

- Controller tidak mengakses AppDbContext langsung, menjalankan FEFO, atau mengurangi quantity.
- Service application mengorkestrasi actor/permission, TimeProvider, repository, domain dan satu commit UoW. Semua repository dan UoW dalam command berbagi AppDbContext scoped yang sama.
- LINQ persistence berada pada repository. LINQ in-memory untuk FEFO boleh berada pada LotSelectionService yang murni.
- Models/User.cs dan Services/IdentityService.cs secara logis infrastructure walaupun foldernya bercampur dengan tipe lain. Domain inventaris tidak bergantung pada User, Identity, DbContext, HttpContext atau React.
- ServiceResult netral terhadap transport; controller memetakan kegagalan ke 400/401/403/404/409/500 sesuai SRS §7.1, bukan HTTP 200.
- FluentValidation memvalidasi input, bukan menggantikan invariant domain atau authorization. AutoMapper tidak boleh memetakan request bebas ke InitialQuantity, RemainingQuantity, actor, status, atau version token.
- DbInitializer/UserSeeder idempotent, tidak mereset data, dan bukan bagian dari request bisnis normal. Jangan menjalankan migration produksi sebagai verifikasi lokal.

### 8.3 Identity, JWT, dan frontend

Ikuti SRS AUTH-01–04 dan RBAC-01/02. AuthController menerima LoginRequestDto; IIdentityService/IdentityService memverifikasi kredensial melalui ASP.NET Core Identity dan menghasilkan AuthResponseDto berisi JWT, expiry token dan profil/role aman.

Backend MUST memvalidasi signature, issuer, audience dan lifetime JWT. Token tidak valid/expired menghasilkan 401; identitas valid tanpa izin menghasilkan 403. Actor berasal dari identitas terverifikasi, bukan payload. Untuk actor Guid dalam kontrak lama, pemetaan ID Identity harus ditetapkan dan tervalidasi; jangan membuat Guid acak atau mengasumsikan format claim cocok (G-02/G-05).

Jangan mengekspos password, hash, security stamp, signing secret atau detail exception melalui response/log. Akun bootstrap tidak memakai password hardcoded. Gunakan HTTPS pada produksi dan origin React yang dikonfigurasi. React hanya mengakses Web API, tidak PostgreSQL; UI bukan sumber otoritatif role, eligibility atau quantity.

Lifetime, refresh/revocation, penyimpanan token React, password/lockout policy, provisioning, pemberi role dan pemetaan istilah Scientist tetap D-11. Jangan membuka endpoint Create/UpdateScientist atau self-registration sebelum otorisasinya ditetapkan. React harus menangani 401/403 dan menawarkan reload/reconfirm saat conflict 409 tanpa silent overwrite.

## 9. Prosedur kerja agent

### 9.1 Sebelum mengubah kode

1. Baca dokumen sumber yang tersedia dan instruksi repository yang berlaku.
2. Temukan solution/project, konfigurasi SDK, dependensi, test, CI, persistence provider, dan pola yang sudah dipakai. Jangan mengarang path atau perintah khusus repo.
3. Identifikasi requirement `FR-*`, `BR-*`, `NFR-*`, dan `AC-*` dari SRS/PRD yang akan disentuh.
4. Periksa apakah perubahan bergantung keputusan OPEN. Lanjutkan bagian independen; ajukan hanya keputusan yang diperlukan untuk bagian terdampak.
5. Buat perubahan sekecil yang memadai. Jangan melakukan refactor luas atau menambah framework tanpa kebutuhan requirement.

### 9.2 Saat mengimplementasikan

- Mulai dari domain dan failure path, kemudian application/persistence, lalu transport/UI.
- Jaga public contract diagram; catat usulan perubahan yang diperlukan sebelum menggantinya.
- Jangan membuat entity DTO atau database menjadi sumber rule yang berbeda dari domain.
- Setiap defect fix harus memperbaiki penyebabnya; test regresi untuk risiko bisnis yang relevan.
- Jangan memperlemah assertion atau menonaktifkan warnings untuk membuat pipeline hijau.
- Jangan memodifikasi file sumber pengguna yang tidak termasuk lingkup perubahan.

### 9.3 Verifikasi

Gunakan perintah proyek yang sudah tersedia. Untuk repository .NET standar, contoh berikut adalah pola, bukan klaim bahwa solution/test project sudah ada:

```bash
dotnet restore
dotnet build --no-restore -warnaserror
dotnet test --no-build
```

Jalankan dari direktori solution yang benar, atau tambahkan path solution/project yang ditemukan. Sesuaikan configuration konsisten dengan CI. Jika test memakai collector Coverlet yang telah dipasang, jalankan coverage sesuai konfigurasi proyek, misalnya `dotnet test --collect:"XPlat Code Coverage"`. Jangan mengklaim coverage tersedia jika collector belum terpasang atau command tidak berjalan.

- Build dan test pada setiap push melalui CI.
- Aktifkan TreatWarningsAsErrors; target static-analysis warnings nol.
- Laporkan coverage dengan Coverlet/Cobertura atau ekuivalen.
- Unit test domain menggunakan waktu terkontrol dan tanpa database/UI.
- Integration test transaction/concurrency menggunakan PostgreSQL nyata melalui EF Core; mock/in-memory saja tidak membuktikan atomicity.
- Verifikasi build/test/lint React menggunakan script dan package manager yang benar-benar tersedia di proyek; jangan mengarang hasil atau nama script.
- Jangan menerapkan migration ke lingkungan produksi sebagai langkah verifikasi lokal.

## 10. Checklist test wajib

| Perilaku | Bukti minimum | Rujukan |
| --- | --- | --- |
| Item/Lot registration | Atribut tersimpan, Item-Lot langsung, initial = remaining, expiry berbeda menjadi Lot berbeda | AC-F01-01/02/03 |
| Expiry | Sebelum, tepat pada, sesudah cutoff tanggal yang disepakati | AC-F02-01/03; G-01 |
| Warning | Di luar, tepat pada, di dalam warning window; warningDays 0 | AC-F02-02; G-01 |
| Eligibility | Active valid positif lolos; ExpiringSoon lolos; semua exclusion gagal | AC-F02-02/03, AC-F07-02 |
| Low stock | Multi-Lot usable, tanpa Lot eligible, di bawah/sama/di atas threshold | AC-F03-01; D-06 |
| Job | Draft -> InProgress dan StartedAt; InProgress -> Completed dan CompletedAt | AC-F04-01/02 |
| FEFO | Multi-Lot, partial last Lot, insufficient stock, expiry tie, received tie, Guid tie-breaker | AC-F05-01/02/03 |
| Consume | Parsial/penuh, nol/negatif/melebihi remaining, InitialQuantity tetap | AC-F06-01/02 |
| Mid-job expiry | Preview valid kemudian consume setelah cutoff ditolak; audit sukses lama tetap | AC-F06-03 |
| Disposal | Quantity/reason valid, invalid ditolak, atomic deduction + Disposal | AC-F07-01 |
| Audit identity | Actor valid dari context, LotId/JobId/reason/waktu benar | AC-F08-01; G-02 |
| Authorization LabOperator | Job, FEFO, Consumption diizinkan; mutasi inventory administratif ditolak tanpa perubahan | SRS RBAC-01, AT-017 |
| Authorization WarehouseAdmin | Item/Lot, Disposal, block/quarantine diizinkan; Job/Consumption ditolak tanpa perubahan | SRS RBAC-01, AT-018 |
| Authorization umum | Kedua role dapat membaca dashboard/audit; akun tanpa role ditolak | SRS RBAC-01, AT-019 |
| Concurrency | Dua writer versi sama; conflict tidak menimpa writer yang sukses | AC-X-01 |
| Atomic rollback | Insert audit gagal atau salah satu Lot gagal; tidak ada partial commit | AC-X-02 |
| Concurrent block | Status berubah setelah load; consume versi lama gagal | SRS AT-008 |
| Identity/JWT | Login valid/invalid; signature, issuer, audience dan expiry token diuji | SRS AT-020/021 |
| Initializer/seeder | Eksekusi ulang tanpa duplikasi akun/role, reset data, atau secret terekspos | SRS AT-022 |
| Struktur Lot-only | Item–Lot langsung, stok/expiry pada Lot, seluruh transaksi memakai LotId, enum/file LotAdministrativeStatus konsisten | SRS AT-023 |
| React–API | UI melalui HTTP/JSON menangani 401/403/409; permission dan eligibility tetap divalidasi server | SRS AT-024 |
| Batas dependensi | Controller melalui service; domain/FEFO dapat diuji tanpa EF Core, Identity, dan UI | SRS AT-025 |

Ganti fixture SRS yang memakai integer ID menjadi Guid nyata. Ganti timestamp expiry menjadi DateOnly dan waktu uji di sekitar cutoff yang telah disepakati. Jangan sekadar memotong timestamp lama ke tanggal tanpa menyesuaikan expected result.

## 11. Keputusan yang belum terselesaikan

| ID | Gap / keputusan | Tindakan agent |
| --- | --- | --- |
| G-01 / D-05 | DateOnly expiry: cutoff, kalender UTC, warning boundary | Pertahankan DateOnly; kunci policy sebelum expiry engine/test boundary final |
| G-02 | Actor tidak ada pada parameter Consume/Dispose | Rancang penyaluran identitas dari Application tanpa merusak kemurnian Domain; jangan persist audit kosong |
| G-03 / D-08 | Block(reason) tanpa field penyimpanan reason | Konfirmasi apakah reason hanya validasi, technical log, atau perlu kontrak riwayat tambahan |
| G-04 / D-07/D-08 | Quarantine/Cancel ada di scope, tetapi metode domain tidak ada di diagram | Usulkan mekanisme/transisi dan pembaruan kontrak; jangan drop fitur atau menambah signature diam-diam |
| G-05 / D-04/D-05 | Kontrak tipe lama dan SRS belum seragam: Guid, DateOnly, byte[] dan fixture instant | Verifikasi diagram ketika tersedia, pertahankan kontrak existing sementara, dan sinkronkan keputusan schema/fixture sebelum implementasi terkait; jangan menyatakan perbedaan ini selesai |
| D-03 (sebagian diputuskan) | PostgreSQL/EF Core final; mapping token dan deployment migration masih OPEN | Pertahankan byte[] dari kontrak lama sampai verifikasi; buktikan concurrency PostgreSQL |
| D-04 | Tipe ID, precision, uniqueness, panjang/validasi, aturan edit | Guid tercatat pada kontrak lama, tetapi SRS masih OPEN; selesaikan G-05 dan detail schema sebelum final |
| D-06 | Low stock `<` atau `<=` | Usulan SRS `<` belum final |
| D-07 | Cancel, state consume, terminal/reversal | Jangan mengembalikan stok otomatis saat cancel tanpa aturan eksplisit |
| D-08 | Full disposal, release block/quarantine, disposal non-Active | Jangan menciptakan transisi status berdasarkan tebakan |
| D-09 | Manual selection, konfirmasi preview, idempotency/retry | Jangan menjamin reservasi atau retry aman yang belum diimplementasikan |
| D-10 | SLA, kapasitas, retention, backup, observability provider | Jangan mengarang target operasional |
| D-11 (sebagian diputuskan) | Identity/JWT final; provisioning, assignment, multi-role dan kebijakan token/akun masih OPEN | Ikuti stack terpilih; jangan mengarang hak kelola akun, expiry/refresh token, atau pemetaan Scientist |

D-02 sudah DECIDED: React dan ASP.NET Core Web API .NET 8 melalui HTTP/JSON. D-01 menetapkan dua role, sedangkan PostgreSQL/EF Core dan Identity/JWT menutup pilihan teknologi D-03/D-11, bukan seluruh detail implementasinya. Kontrak tipe dari dokumen lama tidak otomatis menutup SRS D-04. Revisi ini hanya memperbarui agent.md; tidak mengubah SRS, diagram, skill atau source code.

## 12. Kriteria selesai dan format laporan agent

Sebelum menyatakan pekerjaan selesai, pastikan:

- [ ] Model produksi, DTO, mapping, repository, service, migration, dan test mempertahankan Lot sebagai unit terkecil.
- [ ] File dan tipe enum bernama LotAdministrativeStatus; kontrak Guid, DateOnly, DateTimeOffset, decimal, byte[] telah diverifikasi dan perbedaan G-05 diselesaikan untuk fitur terkait.
- [ ] Penempatan backend mengikuti SRS §3.2–3.4; LotController melalui service abstraction dan domain bebas EF Core/Identity/UI.
- [ ] PostgreSQL/EF Core, Identity/JWT, serta integrasi React–Web API diverifikasi sesuai acceptance yang relevan.
- [ ] Aturan eligibility/FEFO/stock memiliki satu implementasi yang konsisten dan deterministik.
- [ ] Stock deduction dan audit bisnis atomik; tidak ada stok negatif atau silent overwrite.
- [ ] Hanya LabOperator dan WarehouseAdmin digunakan; permission matrix diuji pada server.
- [ ] Actor audit valid dan kebijakan expiry yang relevan sudah diselesaikan untuk fitur yang diklaim selesai.
- [ ] Acceptance criteria perubahan lulus; kegagalan dan batas verifikasi dijelaskan.
- [ ] Build/test dan analisis statis diperiksa; hasil aktual dilaporkan tanpa klaim fiktif.
- [ ] Tidak ada perluasan scope atau perubahan kontrak yang disembunyikan.

Laporkan hasil secara ringkas dengan urutan:

1. Perilaku yang berubah dan requirement terkait.
2. File utama yang diubah beserta tujuannya.
3. Perintah verifikasi dan hasil aktual, termasuk test yang tidak dijalankan beserta alasannya.
4. Keputusan terbuka atau keterbatasan yang masih memengaruhi fitur.

Jika source code belum tersedia, laporkan bahwa hasil baru berupa spesifikasi/panduan; jangan mengklaim aplikasi telah diimplementasikan atau test .NET telah lulus.

## 13. Riwayat dokumen

| Versi | Tanggal | Perubahan |
| --- | --- | --- |
| 1.2.0 | 2026-09-10 | Menyelaraskan dengan SRS v1.2.1: Lot-only, LotId dan LotAdministrativeStatus; menetapkan stack, penempatan backend, aturan Identity/JWT/React dan test terkait; memperbarui D-02/D-03/D-11 serta mencatat perbedaan tipe lama dengan SRS sebagai G-05 |
| 1.1.0 | 2026-09-09 | Menetapkan LabOperator dan WarehouseAdmin sebagai dua role aplikasi serta menambahkan aturan authorization dan test akses |
| 1.0.0 | 2026-09-09 | Panduan agent berdasarkan skill(1).md dan diagram.mmd, dengan rekonsiliasi SRS serta gap expiry, actor audit, reason block, dan transisi domain |
