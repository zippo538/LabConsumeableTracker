# Software Requirements Specification (SRS): Lab Consumable Expiry Tracker

| Metadata | Nilai |
| --- | --- |
| Versi dokumen | 1.0 |
| Tanggal | 11 September 2026 |
| Status | Draft untuk penyelarasan dan implementasi |
| Baseline produk | PRD v1.4, 11 September 2026, 34 halaman |
| Berkas sumber | PRD_Lab_Consumable_Expiry_Tracker_v1_4.pdf |
| Nama berkas SRS | srs.md |
| Tim awal pada PRD | Hary Zulfa Supriyanto; Mahindra Irvan Saputra |
| Role kanonis | `scientis` dan `WarehouseAdmin` |
| Backend | ASP.NET Core Web API (.NET 8), EF Core, ASP.NET Core Identity, JWT Bearer |
| Frontend / database | React / PostgreSQL |

## 1. Pendahuluan

### 1.1 Tujuan dokumen

Dokumen ini menerjemahkan PRD v1.4 menjadi kebutuhan perangkat lunak yang dapat ditelusuri dan diverifikasi oleh Product Owner, Engineering, QA, Security/IT, serta perwakilan laboratorium. Dokumen mencakup fungsi aplikasi, aturan domain, data, antarmuka pengguna, endpoint, kebutuhan nonfungsional, dan kriteria penerimaan.

Pernyataan kebutuhan merupakan target implementasi. Dokumen ini tidak menyatakan bahwa aplikasi, migrasi, endpoint, atau pengujiannya telah selesai.

### 1.2 Acuan dan batas sumber

Sumber utama yang dibaca adalah **PRD_Lab_Consumable_Expiry_Tracker_v1_4.pdf**. Nomor fitur `F-01` sampai `F-14`, keputusan `D-01` sampai `D-11`, serta ID acceptance PRD dipertahankan untuk penelusuran.

PRD menyebut SRS v1.2.1, agent.md v1.2.0, skill(1).md, dan Diagram.mmd sebagai konteks historis. Berkas-berkas tersebut tidak menjadi sumber yang diverifikasi langsung dalam penyusunan ini. SRS ini merupakan dokumen turunan PRD v1.4, bukan klaim bahwa berkas historis tersebut telah diperbarui.

### 1.3 Konvensi kebutuhan

| Istilah | Makna |
| --- | --- |
| MUST / Must | Kebutuhan wajib untuk penerimaan MVP, kecuali detailnya secara eksplisit masih bergantung keputusan terbuka. |
| SHOULD / Should | Kebutuhan tambahan; menjadi gate penerimaan apabila dimasukkan ke release. |
| NICE / Nice | Fase lanjutan; tidak menjadi gate MVP. |
| DECIDED | Keputusan produk/teknologi yang telah ditetapkan pada PRD. |
| PARTIAL | Sebagian telah final, sedangkan rincian yang disebutkan masih terbuka. |
| OPEN | Keputusan belum ditetapkan; implementasi terkait harus menunggu penyelarasan. |
| PROPOSED | Usulan desain; tidak boleh diperlakukan sebagai keputusan produk otomatis. |
| FR-xx-yy | Kebutuhan fungsional SRS; `xx` merujuk nomor fitur PRD. |
| BR / NFR / UI | Aturan bisnis / kebutuhan nonfungsional / kebutuhan frontend. |

Prioritas fitur berbeda dari kematangan kontraknya. Sebagai contoh, F-07 adalah Must, tetapi beberapa transisi statusnya masih menunggu D-08. Endpoint pada Lampiran F PRD adalah rancangan kontrak HTTP yang harus diselaraskan dalam OpenAPI sebelum integrasi.

### 1.4 Definisi

| Istilah | Definisi |
| --- | --- |
| Item | Jenis bahan yang memiliki kode, nama, unit dasar, minimum stock, dan warning days. |
| Lot | Unit inventaris terkecil; langsung berada di bawah Item dan menyimpan identitas, quantity, expiry, status, serta lokasi. |
| LotId | Referensi ke satu record Lot; digunakan oleh alokasi dan transaksi. |
| FEFO | First-Expired-First-Out; mendahulukan Lot eligible dengan expiry paling dekat. |
| Eligible | Lot Active, RemainingQuantity positif, dan belum expired pada waktu keputusan. |
| Usable stock | Jumlah RemainingQuantity seluruh Lot eligible milik satu Item. |
| Consumption | Catatan penggunaan quantity dari satu Lot untuk satu Job. |
| Disposal | Catatan pembuangan quantity dari satu Lot, disertai alasan. |
| RowVersion | Token concurrency untuk conditional update; bukan identitas Lot. |
| Actor | Identitas pengguna terverifikasi yang melakukan tindakan. |
| Preview | Hasil rekomendasi alokasi sementara; bukan reservasi atau bukti commit. |

## 2. Gambaran umum dan ruang lingkup

### 2.1 Tujuan sistem

Sistem mengelola bahan laboratorium pada tingkat Lot untuk mencegah penggunaan bahan expired atau tidak eligible, memprioritaskan FEFO, menyediakan sinyal low stock, dan mencatat konsumsi/disposal secara atomik serta dapat ditelusuri.

Alur utama: WarehouseAdmin membuat Item dan mendaftarkan Lot; scientis membuat dan menjalankan Job, meminta preview FEFO, lalu mengonfirmasi konsumsi. Server memvalidasi kembali waktu, stok, status, izin, dan token sebelum commit. WarehouseAdmin menangani disposal dan status administratif; kedua role dapat membaca audit.

### 2.2 Fitur dan prioritas

| ID PRD | Fitur | Prioritas | Bagian SRS |
| --- | --- | --- | --- |
| F-01 | Master Item dan registrasi/pembaruan Lot | Must | 4.1 |
| F-02 | Expiry engine dan eligibility | Must | 4.2 |
| F-03 | Stock, low stock, dashboard dasar | Must | 4.3, 7.3 |
| F-04 | Job lifecycle | Must | 4.4, 7.4 |
| F-05 | Preview FEFO | Must | 4.5, 7.5 |
| F-06 | Consumption atomik | Must | 4.6, 7.6 |
| F-07 | Disposal, blocking, quarantine, release | Must | 4.7, 7.7 |
| F-08 | Riwayat Consumption dan Disposal | Must | 4.8, 7.8 |
| F-09 | Quick actions dashboard | Should | 4.9, 7.9 |
| F-10 | Export / integration | Nice; PROPOSED | 4.10, 7.10 |
| F-11 | Profil dan User Management | Must | 4.11, 7.2 |
| F-12 | Register | Must | 4.12, 7.2 |
| F-13 | Login dan akses sesi | Must | 4.13, 7.2 |
| F-14 | Frontend React | Must; FE-12 tetap Should | 4.14, 8 |

### 2.3 Di luar scope MVP

Purchase order, supplier management, keuangan, warehouse hierarchy, entitas inventaris di bawah Lot, pemecahan Lot, geofencing, data medis pasien, integrasi otomatis alat laboratorium/ERP, kanal email/push, dan mode offline tidak termasuk MVP. Email verification dan reset password ditunda serta tidak menjadi dependensi registrasi MVP. F-10 tetap fase lanjut.

Reservasi FEFO, pengembalian stok otomatis saat Cancel, pilihan manual Lot, dan scope multi-Item tidak boleh dianggap telah disetujui. Kebijakannya mengikuti D-07/D-09.

### 2.4 Asumsi operasional

Pengguna memiliki koneksi jaringan dan akses aplikasi. Setiap Item dapat mempunyai banyak Lot. Semua quantity Lot memakai BaseUnit Item, tanpa konversi unit otomatis. MinimumStock dan ExpiringSoonDays dikonfigurasi per Item. Timestamp event menggunakan UTC; tampilan lokal harus menyebut zona waktu. Cutoff expiry berbasis tanggal tetap memerlukan keputusan.

## 3. Aktor dan otorisasi

### 3.1 Matriks akses

| Kemampuan | scientis | WarehouseAdmin |
| --- | --- | --- |
| Login, baca profil sendiri, ubah FullName sendiri | Ya | Ya |
| Baca dashboard, Item, Lot, stock, expiry, low stock | Ya | Ya |
| Create/update/deactivate master Item | Tidak | Ya |
| Registrasi/update administratif Lot | Tidak | Ya |
| Create/read Job dan jalankan lifecycle | Ya | Tidak |
| Meminta preview FEFO | Ya | Tidak |
| Mengonfirmasi Consumption | Ya | Tidak |
| Mencatat Disposal | Tidak | Ya |
| Block, Quarantine, Release sesuai policy | Tidak | Ya |
| Membaca seluruh riwayat Consumption/Disposal | Ya | Ya |
| Kelola akun lain, status aktif, assignment role | Tidak | Ya |

Pengguna yang hanya memiliki role `scientis` menerima 403 pada endpoint admin. Pengguna yang hanya memiliki role `WarehouseAdmin` tidak memperoleh izin Job/FEFO/Consumption. Hak baca audit tidak otomatis memberikan hak mengoperasikan Job.

### 3.2 Ketentuan identitas

- **AUTH-01:** Nilai role kanonis adalah `scientis` (huruf kecil) dan `WarehouseAdmin`. Seed, membership Identity, claim JWT, policy, DTO role, UI, dan pengujian harus konsisten.
- **AUTH-02:** Registrasi mandiri hanya menghasilkan membership `scientis`; pengguna publik tidak menentukan role atau status aktif.
- **AUTH-03:** Semua endpoint terlindungi memeriksa identitas terverifikasi, status aktif, dan izin pada server. Menyembunyikan tombol bukan mekanisme otorisasi.
- **AUTH-04:** Satu akun boleh mempunyai gabungan izin hanya jika kebijakan multi-role disetujui. Tidak ada role ketiga yang dibentuk otomatis.
- **AUTH-05:** Nama `Scientist` pada DTO baseline bukan role tambahan. Pemetaan/refactor DTO mengikuti D-11.
- **AUTH-06:** Bila terdapat sistem lama, migrasi membership ke `scientis` harus mempertahankan UserId dan audit, menyelaraskan seed/policy, serta menangani claim sesi lama sesuai D-11. Keberadaan implementasi lama tidak diasumsikan.

Lab Manager, QA/QC, Procurement/Planner, Product Owner, dan Support/Security/IT adalah stakeholder proses; bukan role login tambahan.

## 4. Kebutuhan fungsional

### 4.1 F-01: Master Item dan Lot — Must

| ID | Kebutuhan |
| --- | --- |
| FR-01-01 | WarehouseAdmin dapat membuat Item dengan Code, Name, BaseUnit, MinimumStock, dan ExpiringSoonDays. |
| FR-01-02 | WarehouseAdmin dapat memperbarui field master yang diizinkan serta menonaktifkan Item tanpa menghapus histori. Batas edit historis dan representasi penonaktifan mengikuti D-04. |
| FR-01-03 | Kedua role dapat membaca daftar/detail Item serta Lot terkait, menggunakan pencarian, filter relevan, dan pagination. |
| FR-01-04 | WarehouseAdmin dapat mendaftarkan Lot yang langsung merujuk ItemId dengan LotNumber, InitialQuantity, ExpiryDate, ReceivedAt, dan StorageLocation; field status awal mengikuti policy. |
| FR-01-05 | Server menginisialisasi RemainingQuantity sama dengan InitialQuantity. Client tidak boleh menentukan RemainingQuantity secara bebas. |
| FR-01-06 | Bahan dengan expiry berbeda harus direkam sebagai Lot berbeda, walaupun Item sama. |
| FR-01-07 | Pembaruan administratif Lot menggunakan token versi terbaru dan hanya mengubah field yang diizinkan D-04/D-08. |
| FR-01-08 | InitialQuantity/RemainingQuantity tidak tersedia sebagai field edit bebas setelah registrasi. Koreksi historis tidak boleh mengubah audit melalui jalur CRUD umum. |
| FR-01-09 | Validasi required field, uniqueness, precision/scale, batas quantity, tanggal, unit pecahan, dan registrasi expired mengikuti schema/policy yang ditetapkan pada D-04/D-05. |

### 4.2 F-02: Expiry dan eligibility — Must

| ID | Kebutuhan |
| --- | --- |
| FR-02-01 | Server mengevaluasi ExpiryCondition saat membaca data, melakukan alokasi, dan mengonsumsi bahan. |
| FR-02-02 | Lot sebelum cutoff serta di luar warning window berstatus Valid; dalam warning window dan belum melewati cutoff berstatus ExpiringSoon; pada/setelah cutoff berstatus Expired. |
| FR-02-03 | ExpiryCondition merupakan hasil perhitungan, terpisah dari status administratif persisten. |
| FR-02-04 | ExpiringSoon tetap eligible apabila Active dan RemainingQuantity > 0. Expired selalu tidak eligible untuk alokasi/konsumsi. |
| FR-02-05 | Waktu keputusan berasal dari TimeProvider; domain menerima nilai now dan tidak membaca jam sistem langsung. |
| FR-02-06 | Cutoff, timezone untuk tanggal, serta boundary warning window harus diselaraskan melalui D-05 sebelum evaluasi tanggal diterapkan. |

### 4.3 F-03: Stock dan dashboard — Must

| ID | Kebutuhan |
| --- | --- |
| FR-03-01 | Server menyediakan RemainingQuantity per Lot beserta BaseUnit, status administratif, expiry, eligibility, RowVersion, dan EvaluatedAt. |
| FR-03-02 | Server menghitung UsableStock per Item dari Lot eligible saja; Item tanpa Lot eligible menghasilkan nol. |
| FR-03-03 | Server menyediakan MinimumStock dan IsLowStock per Item. Operator pembanding `<` atau `<=` menunggu D-06. |
| FR-03-04 | Dashboard menyediakan jumlah Item low stock serta jumlah Lot expiring soon, expired, dan blocked, dengan waktu evaluasi server. |
| FR-03-05 | Blocked pada ringkasan mencakup Quarantined atau ManuallyBlocked. Kategori yang tumpang tindih tidak dijumlahkan menjadi total Lot unik. |
| FR-03-06 | Query stock mendukung Search, IsLowStock, dan pagination; dashboard mendukung filter ItemId. |
| FR-03-07 | Quantity dengan unit berbeda tidak dijumlahkan sebagai satu angka stok total. |

**PROPOSED dari Lampiran F:** kartu expiry/blocked menghitung Lot dengan RemainingQuantity > 0; kriteria hitungan diekspos sebagai CountCriteria. Cakupan ini perlu diselaraskan dalam kontrak dashboard final.

### 4.4 F-04: Job lifecycle — Must

| ID | Kebutuhan |
| --- | --- |
| FR-04-01 | scientis dapat membaca daftar/detail Job dengan pencarian, filter Status, dan pagination. Scope ownership tidak ditambahkan tanpa keputusan akses. |
| FR-04-02 | Pembuatan Job menghasilkan Draft, dengan StartedAt dan CompletedAt belum terisi. JobNumber mengikuti validasi D-04. |
| FR-04-03 | Start mengubah Draft menjadi InProgress dan mengisi StartedAt menggunakan waktu server. |
| FR-04-04 | Complete mengubah InProgress menjadi Completed dan mengisi CompletedAt menggunakan waktu server. |
| FR-04-05 | Sistem menolak transisi yang tidak sah tanpa perubahan state. Detail Cancel dan terminal state mengikuti D-07. |
| FR-04-06 | Validasi lifecycle harus aman terhadap konsumsi, Complete, dan Cancel yang berjalan bersamaan. |

**PROPOSED D-07:** konsumsi hanya pada InProgress; Cancel dari Draft/InProgress menuju Cancelled; terminal state tidak dibuka kembali; tidak ada stock reversal otomatis. Kebutuhan fungsi Cancel adalah Must, tetapi aturan transisinya belum final.

### 4.5 F-05: Preview FEFO — Must

| ID | Kebutuhan |
| --- | --- |
| FR-05-01 | scientis dapat meminta alokasi berdasarkan JobId, ItemId, dan RequestedQuantity positif. |
| FR-05-02 | Server memuat kandidat Lot milik Item melalui repository dan menyingkirkan Lot tidak eligible pada waktu preview. |
| FR-05-03 | Alokasi diurutkan berdasarkan ExpiryDate ascending, ReceivedAt ascending, lalu Id sebagai tie-breaker stabil. |
| FR-05-04 | Sistem dapat mengalokasikan satu atau beberapa Lot sampai quantity terpenuhi; tiap alokasi tidak melebihi remaining Lot tersebut. |
| FR-05-05 | Jika stock eligible kurang, seluruh permintaan ditolak; tidak ada respons alokasi sukses parsial atau persistence parsial. |
| FR-05-06 | Respons menampilkan LotId, LotNumber, expiry, remaining, Quantity alokasi, RowVersion, BaseUnit, total, dan PreviewedAt. |
| FR-05-07 | Preview tidak mengurangi stok, tidak membuat Consumption, dan tidak menciptakan reservasi. |

### 4.6 F-06: Consumption — Must

| ID | Kebutuhan |
| --- | --- |
| FR-06-01 | scientis dapat mengonfirmasi konsumsi untuk Job dengan alokasi berisi LotId, Quantity, dan ExpectedRowVersion. |
| FR-06-02 | Server memvalidasi identitas, izin, Job, Item, Lot, quantity, total, token, expiry, dan eligibility aktual sebelum commit. |
| FR-06-03 | Quantity setiap Lot positif dan tidak melebihi RemainingQuantity. Validasi total harus sesuai request. |
| FR-06-04 | Deduksi seluruh Lot dan pembuatan seluruh Consumption dalam satu command commit/rollback bersama. |
| FR-06-05 | ConsumedBy dan ConsumedAt berasal dari context/waktu server; payload tidak boleh memalsukannya. |
| FR-06-06 | Konflik pada salah satu Lot membatalkan seluruh command; server tidak mengganti Lot secara otomatis. |
| FR-06-07 | Respons sukses memuat catatan Consumption, total per Item/unit, remaining, dan RowVersion terbaru. |
| FR-06-08 | Timeout/network failure setelah submit tidak dianggap bukti gagal commit. Client tidak melakukan retry otomatis sebelum rekonsiliasi. |

**PROPOSED D-09:** satu Item per command, semua Lot milik Item yang sama, dan penolakan LotId ganda. Kebijakan kepatuhan persis pada preview, pemilihan manual Lot, scope multi-Item, idempotency, dan korelasi rekonsiliasi masih terbuka.

### 4.7 F-07: Disposal dan status administratif — Must

| ID | Kebutuhan |
| --- | --- |
| FR-07-01 | WarehouseAdmin dapat mencatat Disposal untuk Lot dengan Quantity positif, Reason tidak kosong, dan ExpectedRowVersion. |
| FR-07-02 | Quantity disposal tidak melebihi remaining; Lot Disposed ditolak. |
| FR-07-03 | Deduksi quantity dan pembuatan Disposal disimpan atomik dengan actor/waktu server. Disposal tidak mempunyai JobId. |
| FR-07-04 | WarehouseAdmin dapat melakukan Block, Quarantine, dan Release sesuai matriks transisi yang disetujui. |
| FR-07-05 | Block/Quarantine/Release tidak mengurangi quantity dan tidak membuat Disposal palsu. |
| FR-07-06 | Lot blocked/quarantined dikeluarkan dari FEFO dan Consumption. |
| FR-07-07 | Release tidak memperpanjang expiry; Lot expired tetap tidak eligible meskipun menjadi Active. |
| FR-07-08 | Perubahan status menggunakan ExpectedRowVersion; respons memuat status, eligibility, remaining, token terbaru, dan waktu evaluasi. |

**OPEN D-08:** izin disposal pada expired/blocked/quarantined, efek full disposal, matriks release, kebutuhan Reason setiap perubahan status, serta bentuk audit status. Usulan: Active → ManuallyBlocked/Quarantined; ManuallyBlocked/Quarantined → Active; Disposed tidak direlease; full disposal → Disposed. Jika audit status diwajibkan, perubahan status dan auditnya harus atomik.

### 4.8 F-08: Audit — Must

| ID | Kebutuhan |
| --- | --- |
| FR-08-01 | Kedua role dapat membaca seluruh riwayat Consumption dan Disposal melalui daftar terpadu, daftar khusus, dan detail. |
| FR-08-02 | Consumption menampilkan Lot, Item, Job, quantity/unit, waktu, dan actor; Disposal menampilkan Lot, Item, quantity/unit, Reason, waktu, dan actor. |
| FR-08-03 | Riwayat mendukung filter jenis event, ItemId, LotId, rentang waktu, serta JobId untuk Consumption; filter diterapkan sebelum pagination. |
| FR-08-04 | Kombinasi EventType=Disposal dengan JobId pada audit terpadu ditolak 400. JobId tanpa EventType membatasi hasil ke Consumption. |
| FR-08-05 | Endpoint audit bersifat baca; tidak disediakan edit/hapus histori transaksi melalui kontrak ini. |
| FR-08-06 | Histori tetap dapat ditelusuri ketika Lot kemudian expired atau akun actor dinonaktifkan. |
| FR-08-07 | Daftar tanpa hasil menghasilkan respons sukses kosong; resource detail tidak ditemukan menghasilkan 404. |

### 4.9 F-09: Quick actions — Should

- **FR-09-01:** Dashboard dapat menyediakan alert LowStock, ExpiringSoon, Expired, dan Blocked dengan tautan Item/Lot terkait.
- **FR-09-02:** AllowedActions mengikuti role dan policy aktual pengguna; Href dibatasi ke route internal.
- **FR-09-03:** Navigasi tidak memicu mutasi stok. Disposal/Consumption/perubahan status tetap memerlukan konfirmasi dan endpoint terotorisasi.
- **FR-09-04:** Tidak ada email/push atau penyimpanan notifikasi baru yang diwajibkan oleh fitur ini.

### 4.10 F-10: Export dan integration — Nice / PROPOSED

- **FR-10-01:** Kandidat ekspor menyediakan stock per Item, Consumption, dan Disposal menggunakan filter yang relevan.
- **FR-10-02:** CSV UTF-8 merupakan usulan format; otorisasi kedua role untuk ekspor juga masih usulan.
- **FR-10-03:** Jika diaktifkan, export harus sesuai seluruh hasil filter dalam batas yang disepakati; tidak boleh memotong hasil diam-diam atau hanya mengekspor halaman aktif tanpa definisi tersebut.
- **FR-10-04:** Data kosong pada usulan CSV menghasilkan header tanpa baris; invalid format/rentang menghasilkan 400.
- **FR-10-05:** Kandidat integrasi baca menggunakan kembali endpoint JSON stock/audit; tujuan, kredensial, rate limit, delta sync, dan otorisasi akhir belum ditetapkan.
- **FR-10-06:** Batas baris, snapshot, sanitasi sel CSV, timeout, dan kebutuhan proses asynchronous diselesaikan sebelum release F-10. Tidak ada webhook, API key, atau koneksi ERP yang otomatis dianggap disetujui.

### 4.11 F-11: User Management — Must

| ID | Kebutuhan |
| --- | --- |
| FR-11-01 | Pengguna aktif dapat melihat profil sendiri dan memperbarui FullName saja melalui endpoint mandiri. |
| FR-11-02 | WarehouseAdmin dapat membaca daftar/detail akun dengan search/pagination, membuat akun, dan memperbarui profil yang diizinkan. |
| FR-11-03 | Perubahan status aktif dan role akun dilakukan melalui operasi khusus yang terotorisasi; pengguna tidak menaikkan role sendiri melalui profil. |
| FR-11-04 | Akun yang dirujuk audit dinonaktifkan, bukan dihapus keras. UserId historis dipertahankan. |
| FR-11-05 | Sistem menolak penonaktifan atau penghapusan role yang menghilangkan WarehouseAdmin aktif terakhir, termasuk pada request bersamaan. |
| FR-11-06 | Audit akun mencatat actor, target UserId, waktu, dan jenis perubahan; tidak menyimpan password/token. |
| FR-11-07 | Username/email unik setelah normalisasi Identity; password/hash dikelola Identity, bukan tabel password custom. |
| FR-11-08 | Profil aman memuat Id, FullName, UserName, Email, IsActive, dan role yang diizinkan, tanpa PasswordHash, SecurityStamp, signing secret, atau token akun lain. |

### 4.12 F-12: Register — Must

- **FR-12-01:** Request publik menerima FullName, UserName, Email, Password, dan ConfirmPassword.
- **FR-12-02:** FluentValidation memeriksa required field, format email, dan kecocokan password; Identity menegakkan password policy dan keunikan akun.
- **FR-12-03:** Server membuat akun aktif dan membership `scientis` sebagai satu operasi konsisten. Kegagalan assignment role tidak meninggalkan akun parsial.
- **FR-12-04:** Payload yang menyisipkan role, IsActive, UserId, atau actor ditolak 400; form registrasi tidak memiliki pemilih role.
- **FR-12-05:** Sukses menghasilkan 201 dengan profil aman, tanpa JWT otomatis; UI mengarahkan ke Login. Input invalid menghasilkan 400; konflik akun menghasilkan 409.

### 4.13 F-13: Login dan sesi — Must

- **FR-13-01:** Login menerima UserName dan Password; Identity memverifikasi kredensial dan status akun.
- **FR-13-02:** Sukses menghasilkan 200 dengan JWT, tokenType Bearer, expiresAt, dan profil/role aman. UI membuka dashboard atau return path internal yang diizinkan.
- **FR-13-03:** Kredensial salah, akun nonaktif, atau terkunci memakai pesan umum dan tidak mengungkap keberadaan akun.
- **FR-13-04:** Endpoint terlindungi memvalidasi signature, issuer, audience, lifetime JWT, status aktif, dan authorization.
- **FR-13-05:** Penonaktifan akun atau pencabutan role menghentikan izin lama pada request berikutnya melalui pemeriksaan state/version server; validitas signature saja tidak cukup.
- **FR-13-06:** JWT invalid/expired menghasilkan 401; identitas valid tanpa izin menghasilkan 403.
- **FR-13-07:** Logout membersihkan state sesi, token client, dan cache pengguna, lalu membuka Login. Logout client tidak dianggap mencabut token server.
- **FR-13-08:** Lifetime/refresh/revocation, penyimpanan token React, password policy, dan lockout policy tetap D-11. Tidak ada retry login tanpa batas.

### 4.14 F-14: Frontend React — Must

- **FR-14-01:** React menyediakan halaman akun, dashboard, Item/Lot, Job, FEFO/Consumption, disposal/status, audit, dan kelola user sesuai role.
- **FR-14-02:** Semua operasi inti harus terintegrasi API; mock/fixture saja tidak memenuhi acceptance stok, akun, atomicity, atau audit.
- **FR-14-03:** Frontend mengikuti aturan state, aksesibilitas, responsivitas, dan integrasi pada Bagian 8.
- **FR-14-04:** FE-12 Quick actions tetap Should dan tidak menaikkan prioritas F-09.

## 5. Aturan domain dan transaksi

### 5.1 Invariant

| ID | Aturan |
| --- | --- |
| BR-01 | Item memiliki Lot secara langsung; Lot adalah unit inventaris terkecil. Tidak ada lapisan inventaris persisten di bawah Lot. |
| BR-02 | Lot menyimpan identity, expiry, remaining, status, dan lokasi sendiri. Semua Consumption/Disposal/alokasi merujuk LotId. |
| BR-03 | RemainingQuantity awal sama dengan InitialQuantity; remaining tidak negatif; InitialQuantity tidak berubah setelah transaksi. |
| BR-04 | Konsumsi/disposal menggunakan quantity positif dan tidak lebih besar daripada remaining aktual. |
| BR-05 | Unit transaksi mengikuti BaseUnit Item; konversi otomatis dan penjumlahan lintas unit tidak tersedia. |
| BR-06 | Eligible = Active AND RemainingQuantity > 0 AND belum expired pada waktu keputusan. |
| BR-07 | UsableStock(Item) = jumlah remaining Lot eligible milik Item tersebut. |
| BR-08 | Expired berbeda dari AdministrativeStatus. ExpiringSoon tidak berarti tidak eligible. |
| BR-09 | Alokasi FEFO deterministik berdasarkan ExpiryDate, ReceivedAt, Id; penentuan ordering ID mengikuti D-04. |
| BR-10 | Rekomendasi bukan reservasi; waktu preview tidak digunakan sebagai waktu validasi commit. |
| BR-11 | Actor dan timestamp transaksi berasal dari context terverifikasi dan waktu server. |
| BR-12 | Seluruh deduksi dan audit satu command commit/rollback bersama, termasuk multi-Lot. |
| BR-13 | ExpectedRowVersion diperiksa secara atomik; mismatch menghasilkan conflict tanpa silent overwrite. |
| BR-14 | Server tidak mengganti Lot secara diam-diam pada konflik konsumsi. |
| BR-15 | Histori Consumption yang sah sebelum expiry tetap sah setelah Lot kemudian expired. |
| BR-16 | Disposal tidak memiliki JobId; perubahan administratif tidak membentuk Disposal palsu. |

### 5.2 Algoritme FEFO

1. Validasi akses scientis, keberadaan Job/Item, quantity positif, dan policy Job yang berlaku.
2. Ambil kandidat dengan `ILotRepository.GetCandidatesAsync(itemId)`.
3. Ambil `now` dari `TimeProvider.GetUtcNow()` pada application boundary.
4. Evaluasi eligibility kandidat pada `now` dengan cutoff D-05.
5. Urutkan kandidat eligible berdasarkan expiry, received, lalu Id ascending.
6. Untuk setiap Lot, alokasikan `min(RemainingQuantity, quantity yang belum terpenuhi)`.
7. Berhenti saat quantity terpenuhi. Jika tidak cukup, tolak tanpa persistence.
8. Kembalikan alokasi, unit, total, waktu preview, dan RowVersion setiap Lot.

### 5.3 Boundary transaksi Consumption

Satu command mencakup validasi Job, pembacaan state Lot yang relevan, pengecekan versi dan eligibility, deduksi quantity, serta pembuatan semua Consumption. Commit memakai repository/UoW dengan AppDbContext scoped yang sama. `IUnitOfWork.SaveChangesAsync()` dan strategi transaksi harus benar-benar menjamin all-or-nothing; pemanggilan SaveChanges saja bukan bukti bahwa race condition telah ditangani.

Strategi locking/isolation/concurrency Job ditentukan Engineering dan diuji pada PostgreSQL. Jangan menambahkan field versi Job atau mengubah model kanonis tanpa menyelaraskan desain. Kegagalan validasi, versi stale, atau kegagalan persistence tidak boleh menyisakan stock/audit sukses parsial.

### 5.4 Ringkasan lifecycle

| Resource | State awal | Operasi | State hasil | Status ketentuan |
| --- | --- | --- | --- | --- |
| Job | Belum ada | Create | Draft | Ditetapkan |
| Job | Draft | Start | InProgress | Ditetapkan |
| Job | InProgress | Complete | Completed | Ditetapkan |
| Job | Draft/InProgress | Cancel | Cancelled | PROPOSED D-07 |
| Job | Completed/Cancelled | Buka kembali | Tidak diizinkan | PROPOSED D-07 |
| Lot | Active | Block | ManuallyBlocked | Transisi usulan D-08 |
| Lot | Active | Quarantine | Quarantined | Transisi usulan D-08 |
| Lot | ManuallyBlocked/Quarantined | Release | Active | Transisi usulan D-08 |
| Lot | Status yang diizinkan disposal | Full disposal | Disposed | PROPOSED D-08 |

## 6. Spesifikasi data

### 6.1 Model kanonis

Tipe di bawah adalah tipe logis untuk kebutuhan produk. Tipe ID database/API, panjang, nullability rinci, precision/scale, encoding versi, dan representasi expiry harus difinalisasi melalui D-03/D-04/D-05. Tidak ada penetapan otomatis Guid, DateOnly, atau byte[] hanya karena disebut pada dokumen historis.

| Entitas | Atribut | Ketentuan |
| --- | --- | --- |
| Item | Id; Code; Name; BaseUnit; MinimumStock; ExpiringSoonDays | Memiliki nol atau lebih Lot; menghitung IsLowStock dari total usable sesuai D-06. |
| Lot | Id; ItemId; LotNumber; InitialQuantity; RemainingQuantity; ExpiryDate; ReceivedAt; AdministrativeStatus; StorageLocation; RowVersion | Unit terkecil; BaseUnit berasal dari Item; quantity merupakan decimal. |
| Job | Id; JobNumber; Status; StartedAt; CompletedAt | Timestamp lifecycle diisi server; Draft belum mempunyai waktu Start/Complete. |
| Consumption | Id; JobId; LotId; Quantity; ConsumedAt; ConsumedBy | Satu record untuk penggunaan satu Lot pada satu Job; actor merujuk identitas pengguna. |
| Disposal | Id; LotId; Quantity; Reason; DisposedAt; DisposedBy | Pembuangan satu Lot; tidak mempunyai JobId. |
| LotAllocation | LotId; Quantity | Hasil perhitungan, bukan entitas inventaris persisten; metadata preview dimuat oleh response DTO. |
| User | Id; FullName; UserName; Email; IsActive; timestamp pencatatan/perubahan | Identity mengelola password hash dan membership role; response hanya field aman. |

UsableStock, IsLowStock, ExpiryCondition, IsEligible, dan EvaluatedAt merupakan data hasil evaluasi/read model; jangan memperlakukannya sebagai field bebas dari request. Representasi penonaktifan Item belum tercantum pada atribut kanonis dan perlu diselaraskan sebelum schema difinalisasi.

### 6.2 Relasi dan integritas

| Relasi | Kardinalitas |
| --- | --- |
| Item → Lot | Satu Item memiliki 0..n Lot; setiap Lot merujuk satu Item. |
| Lot → Consumption | Satu Lot memiliki 0..n Consumption; setiap Consumption merujuk satu Lot. |
| Job → Consumption | Satu Job memiliki 0..n Consumption; setiap Consumption merujuk satu Job. |
| Lot → Disposal | Satu Lot memiliki 0..n Disposal; setiap Disposal merujuk satu Lot. |
| User → actor audit | UserId actor historis harus tetap dapat ditelusuri; nonaktif tidak menghapus histori. |

Foreign key dan strategi penghapusan harus menjaga integritas histori. Kontrak ini tidak menyediakan hard delete akun yang dirujuk audit, maupun edit/hapus transaksi audit. Aturan uniqueness Code, LotNumber, dan JobNumber mengikuti D-04; jangan mengasumsikan global/per-Item sebelum diputuskan.

### 6.3 Enum

| Enum | Nilai |
| --- | --- |
| UserRole | `scientis`, `WarehouseAdmin` |
| LotAdministrativeStatus | `Active`, `Quarantined`, `ManuallyBlocked`, `Disposed` |
| ExpiryCondition | `Valid`, `ExpiringSoon`, `Expired` |
| UnitOfMeasure | `Milliliter`, `Gram`, `Unit`, `Vial` |
| JobStatus | `Draft`, `InProgress`, `Completed`, `Cancelled` |

### 6.4 Validasi data

Server menolak input quantity tidak valid, field yang tidak boleh ditentukan client, resource induk yang tidak ada, dan status/transisi yang tidak sah. InitialQuantity saat registrasi harus valid menurut D-04; aturan khusus quantity konsumsi/disposal adalah > 0. Reason disposal wajib tidak kosong. Username/email dinormalisasi dan divalidasi melalui Identity. Timestamp event adalah instant UTC; tanggal expiry tidak dikonversi ke midnight tanpa keputusan D-05.

## 7. Spesifikasi antarmuka API

### 7.1 Konvensi umum

| Aspek | Ketentuan |
| --- | --- |
| Transport | HTTP/JSON; HTTPS pada produksi. Prefix API `/api`; route UI React bukan route API. |
| Autentikasi | `Authorization: Bearer <token>` pada endpoint terlindungi; Register/Login publik. |
| Casing | Contoh inventaris memakai PascalCase sesuai Lampiran F. Contoh akun pada PRD menyebut tokenType/expiresAt. Casing final seluruh schema harus diselaraskan di OpenAPI sebelum integrasi. |
| Identitas | Path ID dan token versi diperlakukan opaque; format final mengikuti D-03/D-04. |
| Pagination | Rancangan: `Page=1`, `PageSize=20`; maksimum PageSize 100 masih usulan. Envelope daftar: `Items`, `Page`, `PageSize`, `TotalCount`. |
| Validasi query | Enum, ID, pagination, dan rentang tidak valid menghasilkan 400. |
| Waktu event | ISO 8601 UTC; `From` inklusif dan `To` eksklusif; jika keduanya diisi harus `From < To`. |
| Sumber identitas command | JobId/LotId dari path; server memvalidasi relasi dengan Item/Lot payload. Actor tidak berasal dari payload. |
| Field server | ConsumedBy, DisposedBy, timestamp event, RemainingQuantity, dan status turunan tidak boleh diisi bebas oleh client. Penyisipan field terlarang menghasilkan 400. |
| Concurrency | ExpectedRowVersion pada konsumsi/disposal/perubahan status Lot; response sukses memuat RowVersion terbaru. Update administratif Lot juga mengikuti token pada F-01. |
| Daftar kosong | 200 dengan Items kosong dan TotalCount 0. |
| Detail/resource induk tidak ada | 404; berbeda dari stock nol. |
| POST dan retry | Tidak ada jaminan idempotency sebelum D-09 final; jangan melakukan retry mutasi otomatis saat hasil commit tidak diketahui. |

Singkatan akses pada tabel: **S** = scientis, **W** = WarehouseAdmin, **S/W** = salah satu dari kedua role yang aktif, **Aktif** = pengguna terautentikasi aktif sesuai kebijakan akun. Error umum 400/401/403/404/409/500 mengikuti kondisi yang relevan; tidak setiap endpoint harus mengembalikan semua kode tersebut.

### 7.2 F-11–F-13: Akun, Register, Login

| Method | Endpoint | Akses | Request/query | Respons sukses |
| --- | --- | --- | --- | --- |
| POST | `/api/auth/register` | Publik | FullName, UserName, Email, Password, ConfirmPassword | 201 profil aman; tanpa JWT otomatis. |
| POST | `/api/auth/login` | Publik | UserName, Password | 200 JWT, tokenType Bearer, expiresAt, profil/role aman. |
| GET | `/api/users/me` | Aktif | Tanpa body | 200 profil sendiri. |
| PATCH | `/api/users/me` | Aktif | FullName saja | 200 profil terbaru. |
| GET | `/api/users` | W | Search dan pagination; penamaan final query mengikuti kontrak akun | 200 daftar akun aman. |
| GET | `/api/users/{id}` | W | ID path | 200 detail akun aman. |
| POST | `/api/users` | W | Data pembuatan akun dan password; field role/status melalui kebijakan admin yang diselaraskan | 201 profil akun aman. |
| PATCH | `/api/users/{id}` | W | Field profil yang diizinkan; bukan password/status/role bebas | 200 profil terbaru. |
| PATCH | `/api/users/{id}/status` | W | Perubahan status aktif; schema final mengikuti kontrak akun | 200 hasil terbaru. |
| PATCH | `/api/users/{id}/roles` | W | Assignment role kanonis; cardinality mengikuti keputusan multi-role | 200 hasil terbaru. |

Login gagal menggunakan 401 dengan pesan umum. Registrasi tidak valid menggunakan 400; konflik username/email menggunakan 409. Perlindungan WarehouseAdmin aktif terakhir menggunakan 409. Detail payload create user, edit profil admin, status, dan roles belum dirinci penuh oleh PRD; jangan menafsirkan tabel sebagai izin untuk mengisi semua field User.

**DTO respons profil aman:** Id, FullName, UserName, Email, IsActive, dan role yang diizinkan. Password hanya input pembuatan akun, tidak dikembalikan. Logout merupakan perilaku client pada baseline; tidak ada endpoint refresh/logout/revoke server yang ditetapkan PRD ini.

### 7.3 F-03: Stock dan dashboard

| Method | Endpoint | Akses | Query | Respons 200 |
| --- | --- | --- | --- | --- |
| GET | `/api/stock/items` | S/W | Search, IsLowStock, Page, PageSize | Daftar ItemId, Code, Name, BaseUnit, UsableStock, MinimumStock, IsLowStock, EvaluatedAt. |
| GET | `/api/items/{itemId}/stock` | S/W | ID path | Detail stock Item dengan field ringkasan serta ExpiringSoonDays dan EvaluatedAt. |
| GET | `/api/lots/{lotId}/stock` | S/W | ID path | LotId, ItemId, LotNumber, BaseUnit, InitialQuantity, RemainingQuantity, ExpiryDate, AdministrativeStatus, ExpiryCondition, IsEligible, RowVersion, EvaluatedAt. |
| GET | `/api/dashboard/summary` | S/W | ItemId opsional | LowStockItemCount, ExpiringSoonLotCount, ExpiredLotCount, BlockedLotCount, EvaluatedAt; CountCriteria sesuai rancangan hitungan. |

Item yang ada tetapi tidak memiliki Lot eligible menghasilkan UsableStock=0. Item/Lot detail yang tidak ada menghasilkan 404. Ringkasan tidak menjumlahkan quantity lintas unit dan tidak menjumlahkan kategori Lot yang tumpang tindih sebagai total unik. Cakupan CountCriteria masih harus diselaraskan sebagaimana Bagian 4.3.

### 7.4 F-04: Job lifecycle

| Method | Endpoint | Akses | Input | Hasil |
| --- | --- | --- | --- | --- |
| GET | `/api/jobs` | S | Search, Status, Page, PageSize | 200 daftar JobId, JobNumber, Status, StartedAt, CompletedAt. |
| GET | `/api/jobs/{jobId}` | S | ID path | 200 detail Job dan ringkasan konsumsi per Item/unit. |
| POST | `/api/jobs` | S | JobNumber | 201 Job Draft; StartedAt/CompletedAt null; Location ke detail. |
| POST | `/api/jobs/{jobId}/start` | S | Tanpa body | 200 Job InProgress dengan StartedAt server. |
| POST | `/api/jobs/{jobId}/complete` | S | Tanpa body | 200 Job Completed dengan CompletedAt server. |
| POST | `/api/jobs/{jobId}/cancel` | S | Tanpa body pada rancangan | 200 Job Cancelled; transisi mengikuti D-07 yang masih terbuka. |

Resource tidak ditemukan: 404. Transisi tidak sah: 409 INVALID_JOB_STATE. JobNumber invalid: 400; duplikat: 409 DUPLICATE_RESOURCE bila aturan unik disepakati D-04. Riwayat konsumsi lengkap tersedia pada F-08 dengan JobId. Scope daftar Job tidak otomatis dibatasi pada pembuat Job, karena model ownership belum ditetapkan PRD.

### 7.5 F-05: Preview FEFO

**Endpoint:** `POST /api/jobs/{jobId}/allocations/preview` — akses S — Must.

| Bagian | Field |
| --- | --- |
| Request | ItemId, RequestedQuantity > 0 |
| Response 200 | JobId, ItemId, BaseUnit, RequestedQuantity, AllocatedQuantity, PreviewedAt, IsReservation=false, Allocations[] |
| Setiap alokasi | LotId, LotNumber, ExpiryDate, RemainingQuantity, Quantity, RowVersion |

Contoh request:

```json
{
  "ItemId": "<item-id>",
  "RequestedQuantity": 7
}
```

Contoh response ilustratif:

```json
{
  "JobId": "<job-id>",
  "ItemId": "<item-id>",
  "BaseUnit": "Milliliter",
  "RequestedQuantity": 7,
  "AllocatedQuantity": 7,
  "PreviewedAt": "2026-09-11T08:00:00Z",
  "IsReservation": false,
  "Allocations": [
    {
      "LotId": "<lot-1>",
      "LotNumber": "LOT-01",
      "ExpiryDate": "<format-sesuai-D-05>",
      "RemainingQuantity": 4,
      "Quantity": 4,
      "RowVersion": "<version-1>"
    },
    {
      "LotId": "<lot-2>",
      "LotNumber": "LOT-02",
      "ExpiryDate": "<format-sesuai-D-05>",
      "RemainingQuantity": 5,
      "Quantity": 3,
      "RowVersion": "<version-2>"
    }
  ]
}
```

Placeholder ID, versi, dan expiry bukan schema final. Waktu/angka hanya ilustrasi. Total alokasi harus sama dengan request. Stock eligible kurang: 409 INSUFFICIENT_STOCK tanpa perubahan data; Job tidak memenuhi policy: 409 INVALID_JOB_STATE; Job/Item tidak ditemukan: 404. Preview tidak mengurangi quantity.

### 7.6 F-06: Consumption

**Endpoint:** `POST /api/jobs/{jobId}/consumptions` — akses S — Must.

| Bagian | Field |
| --- | --- |
| Request rancangan | ItemId, RequestedQuantity, Allocations[] |
| Setiap alokasi request | LotId, Quantity, ExpectedRowVersion |
| Response 201 | JobId, ItemId, BaseUnit, TotalConsumedQuantity, Consumptions[], UpdatedLots[] |
| Setiap Consumption | Id, JobId, LotId, Quantity, ConsumedAt, ConsumedBy |
| Setiap UpdatedLot | LotId, RemainingQuantity, RowVersion |

```json
{
  "ItemId": "<item-id>",
  "RequestedQuantity": 7,
  "Allocations": [
    {
      "LotId": "<lot-1>",
      "Quantity": 4,
      "ExpectedRowVersion": "<version-1>"
    },
    {
      "LotId": "<lot-2>",
      "Quantity": 3,
      "ExpectedRowVersion": "<version-2>"
    }
  ]
}
```

Server memvalidasi setiap Lot dan Job pada command aktual. Quantity nonpositif, total tidak cocok, atau Lot salah Item menghasilkan 400; penolakan LotId ganda mengikuti usulan D-09. Stock kurang, expired, tidak eligible, state Job invalid, atau token stale menghasilkan 409 dengan kode khusus. Satu kegagalan membatalkan seluruh deduksi/audit. Response 201 menjadi bukti commit dan memicu refresh UI.

Satu Item per command dan consume hanya InProgress tetap usulan D-09/D-07. Belum ada jaminan idempotency key, command receipt unik, atau endpoint status command; audit tidak boleh diklaim selalu dapat membedakan dua request identik sebelum mekanisme korelasi ditetapkan.

### 7.7 F-07: Disposal dan status Lot

| Method | Endpoint | Akses | Body | Respons sukses |
| --- | --- | --- | --- | --- |
| POST | `/api/lots/{lotId}/disposals` | W | Quantity, Reason, ExpectedRowVersion | 201 Disposal dan snapshot Lot; Location ke `/api/disposals/{id}`. |
| POST | `/api/lots/{lotId}/block` | W | Reason, ExpectedRowVersion pada rancangan | 200 snapshot Lot; transisi D-08. |
| POST | `/api/lots/{lotId}/quarantine` | W | Reason, ExpectedRowVersion pada rancangan | 200 snapshot Lot; transisi D-08. |
| POST | `/api/lots/{lotId}/release` | W | Reason, ExpectedRowVersion pada rancangan | 200 snapshot Lot; transisi D-08. |

**Disposal response:** `Disposal {Id, LotId, Quantity, Reason, DisposedAt, DisposedBy}` dan `Lot {RemainingQuantity, AdministrativeStatus, RowVersion}`.

**Snapshot status response:** LotId, AdministrativeStatus, RemainingQuantity, ExpiryCondition, IsEligible, RowVersion, EvaluatedAt.

```json
{
  "Quantity": 2,
  "Reason": "Bahan rusak dan tidak dapat digunakan",
  "ExpectedRowVersion": "<version-terbaru>"
}
```

Input invalid/field terlarang: 400; Lot tidak ada: 404; stock kurang, token stale, atau state tidak sah: 409. Disposal harus atomik dengan deduksi. Kebutuhan Reason pada setiap aksi status, matriks transisi, izin disposal untuk tiap status, dan full disposal mengikuti D-08. Release tidak mengubah expiry.

### 7.8 F-08: Audit

| Method | Endpoint | Akses | Filter/input | Respons 200 |
| --- | --- | --- | --- | --- |
| GET | `/api/audit-events` | S/W | EventType, ItemId, LotId, JobId, From, To, Page, PageSize | Daftar gabungan: EventType, EventId, ItemId, LotId, LotNumber, Quantity, BaseUnit, OccurredAt, ActorId, serta JobId atau Reason sesuai jenis. |
| GET | `/api/consumptions` | S/W | JobId, ItemId, LotId, From, To, Page, PageSize | Daftar Id, JobId, LotId, ItemId, Quantity, BaseUnit, ConsumedAt, ConsumedBy. |
| GET | `/api/consumptions/{id}` | S/W | ID path | Detail Consumption beserta identitas Item/Lot/Job dan actor aman. |
| GET | `/api/disposals` | S/W | ItemId, LotId, From, To, Page, PageSize | Daftar Id, LotId, ItemId, Quantity, BaseUnit, Reason, DisposedAt, DisposedBy. |
| GET | `/api/disposals/{id}` | S/W | ID path | Detail Disposal beserta identitas Item/Lot dan actor aman; tanpa JobId fiktif. |

EventType baseline adalah Consumption/Disposal. Audit perubahan akun mengikuti F-11; perluasan event status Lot menunggu D-08. Filter JobId pada audit terpadu membatasi ke Consumption; kombinasi dengan Disposal ditolak 400. From/To mengikuti konvensi UTC. Detail tidak ada: 404; filter valid tanpa hasil: daftar kosong.

**PROPOSED:** urutan audit terpadu `OccurredAt DESC, EventType, EventId`; daftar khusus memakai timestamp event descending lalu Id. Ordering ID mengikuti D-04. Response tidak membocorkan data autentikasi atau profil pribadi yang tidak diperlukan.

### 7.9 F-09: Quick actions

**Endpoint:** `GET /api/dashboard/alerts` — akses S/W — Should.

| Bagian | Field |
| --- | --- |
| Query | Type (LowStock/ExpiringSoon/Expired/Blocked), ItemId, Page, PageSize |
| Item respons | AlertType, ResourceType, ResourceId, ItemId, LotId bila relevan, Message, EvaluatedAt, AllowedActions[] |
| AllowedAction | Code, Label, Href |

Href LowStock mengarah ke `/app/items/{id}`; alert Lot ke `/app/lots/{id}`. Aksi khusus role mengikuti policy aktual; tautan tidak melakukan mutasi. UI tetap memakai konfirmasi dan endpoint terkait untuk menjalankan tindakan.

### 7.10 F-10: Export / integration

Semua kontrak dalam bagian ini **PROPOSED untuk fase lanjut**. Akses S/W, CSV, dan batas operasional belum menjadi keputusan release final.

| Method | Endpoint | Akses usulan | Query | Respons usulan |
| --- | --- | --- | --- | --- |
| GET | `/api/exports/stock` | S/W | Format=csv, ItemId, IsLowStock | 200 text/csv: ItemId, Code, Name, BaseUnit, UsableStock, MinimumStock, IsLowStock, EvaluatedAt. |
| GET | `/api/exports/consumptions` | S/W | Format=csv, JobId, ItemId, LotId, From, To | 200 text/csv dengan field Consumption F-08. |
| GET | `/api/exports/disposals` | S/W | Format=csv, ItemId, LotId, From, To | 200 text/csv dengan field Disposal F-08; tanpa JobId. |

Usulan response menggunakan UTF-8 serta `Content-Disposition: attachment` dengan nama file dari server. Hasil kosong: CSV header saja. Format/rentang invalid: 400. Batas baris, snapshot, sanitasi sel, timeout, dan asynchronous processing harus ditetapkan sebelum aktivasi. Tidak boleh ada truncation diam-diam.

Integrasi baca dapat direncanakan melalui `/api/stock/items`, `/api/items/{itemId}/stock`, `/api/consumptions`, dan `/api/disposals` sebagai JSON. Kredensial, konsumen, otorisasi, rate limit, dan sinkronisasi delta masih terbuka.

### 7.11 F-01/F-02: Cakupan kontrak yang belum dirinci

PRD mewajibkan create/update/deactivate Item, list/detail Item/Lot, registrasi Lot, dan update administratif Lot. Namun Lampiran D/F belum menetapkan route dan schema HTTP lengkap untuk CRUD tersebut. SRS mempertahankan kebutuhan F-01 tanpa menciptakan route tambahan sebagai kontrak yang dianggap telah disetujui.

Engineering harus melengkapi kontrak F-01 dalam OpenAPI berdasarkan FR-01-01 sampai FR-01-09, termasuk input, field yang boleh diedit, response, status nonaktif, pagination/filter, error, dan token concurrency. F-02 merupakan evaluasi pada read/alokasi/konsumsi; PRD tidak mewajibkan endpoint expiry mandiri, scheduler, atau cron tertentu.

### 7.12 Error

Bentuk error rancangan Lampiran F: Code, Message, TraceId, dan FieldErrors untuk validasi. Code stabil digunakan frontend; pesan tidak mengandung stack trace, secret, atau token. Nama kode konflik akun dan admin terakhir perlu diselaraskan pada kontrak akun; HTTP 409 sudah ditetapkan.

| HTTP | Code rancangan | Kondisi / perilaku |
| --- | --- | --- |
| 400 | VALIDATION_ERROR | Field/quantity/total/enum/rentang/format invalid atau penyisipan field terlarang; tampilkan validasi yang dapat diperbaiki. |
| 401 | UNAUTHENTICATED | Token invalid/expired atau sesi tidak lagi valid; alur Login sesuai Lampiran D/E. |
| 403 | FORBIDDEN | Identitas sah tetapi izin tidak memenuhi policy. |
| 404 | RESOURCE_NOT_FOUND | Resource detail/induk tidak ada; jangan dianggap stock nol. |
| 409 | INSUFFICIENT_STOCK | Stock eligible atau remaining tidak cukup. |
| 409 | LOT_EXPIRED | Lot melewati cutoff pada evaluasi aktual. |
| 409 | LOT_NOT_ELIGIBLE | Lot gagal syarat eligibility selain kasus expired khusus. |
| 409 | CONCURRENCY_CONFLICT | ExpectedRowVersion stale; reload/reconfirm tanpa overwrite. |
| 409 | INVALID_JOB_STATE | Job tidak memenuhi transisi/operasi yang diizinkan. |
| 409 | INVALID_LOT_STATE | Status Lot tidak memenuhi transisi/disposal yang diizinkan. |
| 409 | DUPLICATE_RESOURCE | Konflik uniqueness yang telah disepakati, misalnya JobNumber. |
| 500 | INTERNAL_ERROR | Kegagalan internal dengan pesan aman dan TraceId; client tidak menyimpulkan mutasi berhasil. |

Semua error mutasi harus menjaga invariant stok dan audit. Jika response gagal diterima akibat jaringan, status commit dapat tidak diketahui; hal ini berbeda dari response penolakan server yang sudah terkonfirmasi.

## 8. Frontend React dan antarmuka pengguna

### 8.1 Pemetaan halaman

| ID PRD | Route UI | Akses | Fungsi |
| --- | --- | --- | --- |
| FE-01 | `/register`, `/login` | Publik | Register/Login; logout melalui state sesi client. |
| FE-02 | `/app` | Aktif | Shell, sidebar/menu mobile, identitas, navigasi. |
| FE-03 | `/app/dashboard` | S/W | Low stock, expiring soon, expired/blocked, waktu evaluasi. |
| FE-04 | `/app/items`, `/app/items/:id` | S/W; mutasi W | Daftar/detail Item dan pengelolaan field master yang sah. |
| FE-05 | `/app/lots`, `/app/lots/:id` | S/W; mutasi W | Detail stock/expiry/status/lokasi, registrasi/update administratif. |
| FE-06 | `/app/jobs`, `/app/jobs/:id` | S | Daftar, create, lifecycle Job. |
| FE-07 | `/app/jobs/:id/consume` | S | Kebutuhan Item, preview FEFO, konfirmasi konsumsi. |
| FE-08 | `/app/lots/:id/disposal` dan aksi status | W | Disposal, Block, Quarantine, Release sesuai policy. |
| FE-09 | `/app/audit` | S/W | Riwayat, filter, pagination, detail. |
| FE-10 | `/app/profile` | Aktif | Profil dan edit FullName sendiri. |
| FE-11 | `/app/users`, `/app/users/:id` | W | Pengelolaan akun/status/role. |
| FE-12 | Dashboard | Sesuai role | Quick actions, Should. |

Form create/edit dapat memakai halaman atau dialog dalam modul yang sama. Penempatan UI tidak menciptakan izin baru. Route langsung tetap diperiksa, dan server tetap menjadi otoritas akses/data.

### 8.2 Perilaku form dan alur

- **UI-01:** Register menampilkan FullName, UserName, Email, Password, ConfirmPassword; tidak ada pemilih role. Validasi dan pesan server tetap terlihat. Sukses menuju Login.
- **UI-02:** Login gagal menampilkan pesan umum. Login sukses hanya menggunakan return path internal yang diizinkan.
- **UI-03:** Profil hanya mengizinkan edit mandiri FullName. Manajemen user memiliki konfirmasi perubahan status/role dan menampilkan error admin terakhir.
- **UI-04:** Form Item/Lot mengikuti aturan field backend. InitialQuantity/RemainingQuantity tidak dapat diedit bebas; mutation yang relevan membawa token terbaru.
- **UI-05:** Quantity tidak dibulatkan diam-diam atau mencampur separator desimal lokal; expiry tanggal tidak dikonversi diam-diam ke UTC midnight.
- **UI-06:** Pemilihan kebutuhan konsumsi dilakukan dari Job; preview menampilkan urutan Lot, expiry, remaining/unit, quantity alokasi, total, dan waktu preview.
- **UI-07:** Preview menjelaskan bahwa stok belum direservasi. Pilihan manual Lot tidak disediakan sebelum D-09 mengizinkan.
- **UI-08:** Konfirmasi konsumsi menampilkan Job, Item, Lot, quantity/unit. Submit pending tidak dapat digandakan.
- **UI-09:** Setelah commit sukses, tampilkan bukti Consumption/Disposal dari server dan refresh resource terkait; pengurangan stok lokal bukan bukti commit.
- **UI-10:** Konflik meminta reload/reconfirm. UI tidak mengubah Lot diam-diam dan tidak retry mutasi otomatis ketika hasil commit tidak pasti.
- **UI-11:** Disposal memerlukan dialog ringkasan Lot, quantity/unit, dan Reason. Error mempertahankan input nonrahasia. Status actions menjelaskan dampak eligibility sesuai policy.
- **UI-12:** Filter audit Job hanya relevan pada Consumption. Daftar kosong berbeda dari error. Ekspor tidak ditampilkan sebagai fitur MVP yang telah tersedia.

### 8.3 State dan error UI

| State/hasil | Perilaku wajib |
| --- | --- |
| Initial loading | Spinner/skeleton berlabel; angka stock belum tampil sebagai nol sebelum data tersedia. |
| Pending mutation | Status proses dan pencegahan double submit. |
| Success | Bukti server, feedback singkat, refresh data terkait tanpa kehilangan filter relevan. |
| Empty | Bedakan belum ada data dengan hasil filter kosong; sediakan reset filter atau aksi sah. |
| 400 | Pesan dekat field dan ringkasan yang dapat difokuskan; input aman dipertahankan. |
| 401 | Pesan umum pada Login; pada route terlindungi bersihkan sesi/cache dan arahkan Login dengan return path aman. |
| 403/404 | Halaman akses ditolak/tidak ditemukan dengan navigasi yang diizinkan. |
| 409 | Bedakan stock/expiry/Job/versi/akun conflict berdasarkan Code; reload/reconfirm, tanpa overwrite otomatis. |
| 500/network | Tampilkan kegagalan; retry read boleh; mutasi tidak otomatis diulang jika commit belum diketahui. |
| Background refresh gagal | Data lama boleh tetap terlihat dengan penanda stale/gagal refresh; jangan mengklaim terbaru. |

### 8.4 Pengelolaan state dan integrasi

- **UI-13:** React menggunakan satu adapter HTTP ke ASP.NET Core Web API. Data, enum, token, FEFO, eligibility, dan stock menggunakan hasil server, tanpa kalkulasi bisnis alternatif pada client.
- **UI-14:** Pisahkan state sesi, server data, filter/pagination, dan draft form. Query URL hanya memuat filter aman; tidak memuat password/token.
- **UI-15:** Abaikan response pencarian lama yang datang setelah pencarian baru; back/refresh mempertahankan filter aman.
- **UI-16:** Logout/pergantian akun membersihkan data pengguna lama. Password tidak disimpan dalam draft/cache.
- **UI-17:** Ganti token versi menggunakan response sukses. Pilihan cookie/localStorage/sessionStorage/refresh token belum dikunci sebelum D-11 final.

### 8.5 Responsivitas dan aksesibilitas

- **UI-18:** Uji viewport minimal 360 px, 768 px, dan 1280 px. Tidak ada scroll horizontal seluruh halaman; tabel lebar dapat memiliki scroll lokal atau tampilan kartu.
- **UI-19:** Dialog, form, pesan validasi, dan tombol utama tetap terjangkau pada viewport sempit.
- **UI-20:** Setiap input memiliki label; error terhubung dengan field; status tidak hanya dibedakan oleh warna.
- **UI-21:** Semua operasi utama dapat dijalankan melalui keyboard, fokus terlihat, urutan tab masuk akal, dan dialog mengembalikan fokus ke pemicu.
- **UI-22:** Loading/sukses/error dapat dikenali teknologi bantu; kontras teks/ikon/warna memadai. PRD belum menetapkan klaim sertifikasi atau level standar aksesibilitas tertentu.
- **UI-23:** Format tanggal, zona waktu berlabel, quantity, unit, navigasi aktif, dan tombol utama konsisten. Draft belum tersimpan diberi peringatan jika relevan.

## 9. Kebutuhan nonfungsional dan arsitektur

| ID | Area | Kebutuhan / bukti |
| --- | --- | --- |
| NFR-01 | Stack | Backend ASP.NET Core Web API .NET 8; React; PostgreSQL; EF Core; Identity; JWT Bearer. Teknologi ini adalah keputusan baseline PRD. |
| NFR-02 | Dependensi | Clean Architecture, Repository Pattern, dan DI. Domain/application tidak bergantung implementasi EF/Identity/UI. Susunan folder saja bukan bukti isolasi. |
| NFR-03 | Domain murni | LotSelectionService dan aturan inventory dapat diuji tanpa database/UI, dengan waktu eksplisit. |
| NFR-04 | Persistence | AppDbContext, repository, dan UoW scoped yang sama per command; atomicity dan concurrency dibuktikan pada PostgreSQL nyata. |
| NFR-05 | Numeric/schema | Decimal dengan precision/scale eksplisit; FK dan aturan uniqueness sesuai D-04. Mapping RowVersion harus bekerja pada PostgreSQL, tidak mengasumsikan perilaku database lain. |
| NFR-06 | DTO/mapping | AutoMapper dan FluentValidation mengikuti model kanonis; request mapping tidak membuka quantity/actor/status yang seharusnya diatur server. |
| NFR-07 | Autentikasi/otorisasi | Validasi JWT lengkap, akun aktif, dan policy pada setiap endpoint; role lama tidak tetap berlaku setelah dicabut. |
| NFR-08 | Secret | HTTPS produksi, origin React terkonfigurasi, secret/hash/token tidak terekspos pada response/log. Seed idempotent tanpa password hardcoded. |
| NFR-09 | Error | ServiceResult netral transport; controller memetakan status HTTP; pesan operasional aman tanpa detail exception sensitif. |
| NFR-10 | Integritas | Tidak ada remaining negatif, konsumsi expired, atau konsumsi/disposal tanpa audit atomik. |
| NFR-11 | CI/build | Build/test setiap push, TreatWarningsAsErrors, target static warnings nol; gunakan coverage kritis dengan Coverlet/Cobertura atau ekuivalen. Persentase threshold coverage belum ditetapkan. |
| NFR-12 | Verifikasi | Unit test rule domain, integration PostgreSQL, React-API, RBAC, dan end-to-end kedua role mencakup jalur berhasil/gagal. |
| NFR-13 | Observability | Catat kegagalan eligibility, stock kurang, concurrency, serta TraceId/log teknis aman; pesan pengguna dapat ditindaklanjuti. Provider dan retention mengikuti D-10. |
| NFR-14 | UX | Responsif dan aksesibel sesuai UI-18 sampai UI-23; data gagal tidak ditampilkan sebagai stock nol. |
| NFR-15 | Kinerja/kapasitas | Kapasitas, jumlah pengguna konkuren, latency, dan target throughput belum ditetapkan; finalisasi D-10 sebelum membuat gate kuantitatif. |
| NFR-16 | Operasional | Availability, retention audit, backup, RPO/RTO, strategi migration/deployment, dan konfigurasi operasional mengikuti D-03/D-10. Tidak ada SLA numerik yang dikarang. |

### 9.1 Tanggung jawab implementasi

| Komponen | Tanggung jawab |
| --- | --- |
| Controllers | HTTP, request binding, authorization, dan pemetaan ServiceResult ke response. |
| Services/application | Orkestrasi use case, context pengguna/waktu, validasi lintas resource, dan boundary transaksi. |
| Repositories | Query/persistence melalui EF Core; detail LINQ EF tidak bocor ke domain. |
| Models domain | Invariant Item/Lot/Job, expiry, quantity, dan nilai domain. |
| LotSelectionService | Algoritme FEFO murni; tidak bergantung UI/EF. |
| Data | AppDbContext, initializer, migration, dan seeder. |
| IdentityService/User | Integrasi Identity/JWT secara logis berada pada infrastructure. |
| React | Presentasi, form, navigasi, state, adapter API; bukan otoritas rule inventory. |

### 9.2 Penempatan berkas referensi

Struktur berikut merangkum penempatan yang disebut PRD; bukan bukti berkas implementasi sudah ada:

| Area | Path/nama baseline atau rancangan |
| --- | --- |
| Common | `Common/Results/ServiceResult.cs` |
| Konfigurasi | `Configurations/JwtSettings.cs` |
| Controller baseline | `Controllers/AuthController.cs`, `Controllers/LotController.cs` |
| Pelengkap akun | `Controllers/UserController.cs`; DTO/validator Register dan `DTOs/Users/`, `Validators/Users/` |
| Data | `Data/AppDbContext.cs`, `Data/DbInitializer.cs`, `Data/UserSeeder.cs`, `Data/Interfaces/IDbInitializer.cs` |
| Domain | `Models/Item.cs`, `Lot.cs`, `Job.cs`, `Consumption.cs`, `Disposal.cs`, `User.cs`; `Models/Enums/` sesuai Bagian 6.3 |
| FEFO | Pelengkap `Models/LotAllocation.cs`, `Services/LotSelectionService.cs` |
| Repository | `Repositories/Interfaces/ILotRepository.cs`, `IUnitOfWork.cs`, `Repositories/LotRepository.cs` |
| Identity | `Services/Interfaces/IIdentityService.cs`, `Services/IdentityService.cs` |
| DTO auth baseline | `DTOs/Auth/AuthResponseDto.cs`, `CreateScientistRequestDto.cs`, `LoginRequestDto.cs`, `ScientistResponseDto.cs`, `UpdateScientistRequestDto.cs` |
| Mapping/validator | `Mapping/MappingProfile.cs`, `Validators/Auth/`, `Validators/Lots/` |
| Pelengkap yang masih usulan | ILotService/LotService, implementasi UnitOfWork, dan IUserService/UserService bila diperlukan |
| Root | Program.cs, .csproj, konfigurasi nonsecret; React dan proyek test berada di luar root backend |

Kontrak endpoint Job, stock, audit, status, dan export memerlukan penempatan controller/service yang disepakati Engineering; nama berkas tambahannya belum dikunci PRD.

Frontend direncanakan pada `LabConsumableExpiryTracker.Frontend/src/` dengan `app/`, `features/auth/`, `users/`, `dashboard/`, `items/`, `lots/`, `jobs/`, `consumption/`, `disposal/`, `audit/`, serta `components/`, `layouts/`, `services/`, `hooks/`, `utils/`, `styles/`, dan `tests/`. Pilihan JavaScript/TypeScript, bundler, router, data-fetching, dan UI library belum dikunci.

## 10. Acceptance criteria dan verifikasi

ID acceptance berikut dipertahankan dari PRD. Uraian diringkas dalam pola Given / When / Then. Semua merupakan target verifikasi, bukan hasil uji yang sudah dijalankan.

### 10.1 Domain dan inventaris

| ID PRD | Given / When / Then |
| --- | --- |
| AC-F01-01 | Data Item valid / disimpan / seluruh atribut sah tersimpan sesuai input. |
| AC-F01-02 | Dua expiry berbeda / registrasi / dua Lot terpisah di Item terkait. |
| AC-F01-03 | InitialQuantity valid / registrasi / InitialQuantity tersimpan dan remaining awal sama. |
| AC-F02-01 | Now sebelum cutoff dan di luar warning window / evaluasi / Valid. |
| AC-F02-02 | Expiry dalam warning window, Active, remaining > 0 / evaluasi / ExpiringSoon dan eligible. |
| AC-F02-03 | Now pada/setelah cutoff / alokasi / Lot dikeluarkan. |
| AC-F03-01 | Item dengan banyak Lot / hitung / hanya usable remaining dijumlahkan lalu dibanding threshold sesuai D-06. |
| AC-F04-01 | Job Draft / Start / InProgress dan StartedAt terisi. |
| AC-F04-02 | Job InProgress / Complete / Completed dan CompletedAt terisi. |
| AC-F05-01 | Beberapa Lot eligible pada Item yang sama / alokasi / expiry ascending. |
| AC-F05-02 | Expiry sama / alokasi / ReceivedAt ascending lalu Id. |
| AC-F05-03 | Total eligible kurang / alokasi / seluruh permintaan ditolak tanpa partial persistence. |
| AC-F06-01 | Lot eligible, quantity positif <= remaining / consume / deduksi dan Consumption tercipta atomik. |
| AC-F06-02 | Quantity nol/negatif/melebihi remaining / consume / ditolak tanpa perubahan stok. |
| AC-F06-03 | Lot valid pada preview lalu expired / consume aktual / ditolak. |
| AC-F07-01 | Lot Active, quantity valid, Reason terisi / dispose / quantity dan Disposal commit bersama. |
| AC-F07-02 | Lot blocked/quarantined / alokasi / tidak eligible. |
| AC-F08-01 | Konsumsi/disposal sukses / baca audit / LotId, quantity, waktu, actor, serta Job/Reason tersedia sesuai jenis. |
| AC-X-01 | RowVersion mismatch / update stock / conflict tanpa menimpa perubahan pihak lain. |
| AC-X-02 | Salah satu validasi gagal / akhir operasi / tidak ada quantity/audit sukses parsial. |

### 10.2 Akun dan autentikasi

| ID PRD | Given / When / Then |
| --- | --- |
| AC-F11-01 | Akun aktif / baca atau edit profil sendiri / hanya profil sendiri dan FullName dapat diperbarui melalui endpoint mandiri. |
| AC-F11-02 | scientis / panggil daftar/detail/mutasi user admin / 403 tanpa perubahan. |
| AC-F11-03 | WarehouseAdmin dan input valid / kelola akun/status/role / hasil aman, tersimpan, dan audit tersedia. |
| AC-F11-04 | Akun dirujuk transaksi / dinonaktifkan / audit historis tetap, login dan akses berikutnya ditolak. |
| AC-F11-05 | Admin aktif terakhir / dinonaktifkan atau kehilangan role / 409 dan minimal satu admin aktif tetap ada, termasuk race dua request. |
| AC-F11-06 | JWT telah terbit / role dicabut atau akun nonaktif / request berikutnya tidak memakai hak lama. |
| AC-F12-01 | Register valid/unik / submit / 201 akun aktif scientis, tanpa JWT otomatis, UI ke Login. |
| AC-F12-02 | Required/email/password invalid / register / 400 tanpa akun tersimpan. |
| AC-F12-03 | Username/email sudah digunakan setelah normalisasi / register / 409 tanpa duplikasi. |
| AC-F12-04 | Payload publik menyisipkan role/status / register / 400 tanpa privilege escalation. |
| AC-F12-05 | Pembuatan akun berhasil, assignment role gagal / akhir transaksi / tidak ada akun registrasi parsial. |
| AC-F13-01 | Akun aktif dan password benar / login / 200 JWT, Bearer, expiresAt, profil/role aman. |
| AC-F13-02 | Password salah atau akun nonaktif/terkunci / login / gagal dengan pesan umum. |
| AC-F13-03 | Token invalid/expired atau role tidak sesuai / endpoint terlindungi / 401 atau 403 sesuai kondisi, tanpa mutasi. |
| AC-F13-04 | Sesi aktif / logout / data sesi client dibersihkan dan UI ke Login; tidak mengklaim revocation server. |

### 10.3 Frontend React

| ID PRD | Given / When / Then |
| --- | --- |
| AC-FE-01 | Belum login / membuka route terlindungi / Login dan hanya kembali ke route internal yang sah. |
| AC-FE-02 | Role berbeda / membuka menu/route / UI mengikuti izin, route dilarang ditolak, server tetap memeriksa. |
| AC-FE-03 | Register invalid/duplikat/valid / submit / error jelas atau sukses ke Login, tanpa pemilih role. |
| AC-FE-04 | Login berhasil/gagal lalu logout / jalankan alur / feedback dan navigasi tepat, cache/sesi lama dibersihkan. |
| AC-FE-05 | Dashboard loading/gagal/kosong / render / state dibedakan dan kegagalan bukan stock nol. |
| AC-FE-06 | Item dengan unit berbeda / baca dashboard/detail / usable stock dari API benar, tanpa total quantity lintas unit. |
| AC-FE-07 | Admin/scientis / buka Item/Lot / hanya admin mengelola field sah, initial/remaining tidak diedit bebas. |
| AC-FE-08 | Lot dibaca / render detail / status administratif, expiry, eligibility, unit, lokasi, waktu evaluasi dapat dibedakan. |
| AC-FE-09 | Job berbagai state / scientis melihat aksi / hanya transisi sah sesuai policy backend tersedia. |
| AC-FE-10 | Preview multi-Lot / ditinjau / urutan, quantity/unit, total, dan penjelasan bukan reservasi terlihat. |
| AC-FE-11 | Submit konsumsi pending/sukses / klik berulang / UI mencegah duplikasi; sukses menampilkan bukti server dan refresh. |
| AC-FE-12 | Expiry berubah atau token stale / consume / 409, reload/reconfirm, tanpa silent substitution atau pengurangan stok semu. |
| AC-FE-13 | Disposal invalid/valid / admin submit / validasi/konfirmasi terlihat; sukses menunjukkan bukti dan stock/audit terbaru. |
| AC-FE-14 | Audit dipilih/difilter / render / Consumption memiliki Job, Disposal memiliki Reason; filter/pagination berfungsi. |
| AC-FE-15 | Kelola status/role atau edit profil / submit / field/izin sesuai F-11 dan error admin terakhir terlihat. |
| AC-FE-16 | 401/403/404/500/network / request / state UI sesuai Bagian 8.3; mutasi tidak diulang otomatis saat commit tidak pasti. |
| AC-FE-17 | Viewport 360/768/1280 dan keyboard / alur utama / tidak terpotong, fokus terlihat, label/error terbaca. |
| AC-FE-18 | Dua pencarian dan response terbalik / response tiba / hasil filter terbaru dipertahankan, back/refresh menjaga filter aman. |

### 10.4 Endpoint tambahan dan perubahan role

| ID PRD | Given / When / Then |
| --- | --- |
| AC-API-01 | Stock ada/kosong / kedua role membaca / jumlah per Item benar, unit/EvaluatedAt tersedia, threshold sesuai D-06. |
| AC-API-02 | Job berbagai state / lifecycle dijalankan / hanya transisi sah; race consume/complete/cancel tidak melanggar policy. |
| AC-API-03 | Preview multi-Lot / consume setelah expiry/stock berubah / 409 dan rollback semua deduksi/audit tanpa substitusi diam-diam. |
| AC-API-04 | Disposal/status valid, invalid, stale / admin submit / akses, reason, transaksi, dan token sesuai D-08. |
| AC-API-05 | Dua jenis event / filter/pagination / actor, waktu, quantity benar; Job hanya Consumption, Reason pada Disposal. |
| AC-API-06 | Role berbeda / baca alert dan buka tautan / hanya aksi sah; navigasi tidak memicu mutasi. Gate jika F-09 masuk release. |
| AC-API-07 | Fase ekspor disetujui / export/integrasi baca / schema, filter, akses, unit, batas, dan kelengkapan hasil sesuai kontrak final. |
| AC-API-08 | Register/login/akses menggunakan scientis / seluruh alur / role, claim, seed, UI, policy konsisten; WarehouseAdmin dan audit tetap benar. |

### 10.5 Bukti penerimaan historis yang dirujuk PRD

PRD mengutip AT-017 sampai AT-025 dari SRS historis. Makna ringkas berikut dipertahankan sebagai rujukan PRD; dokumen sumber historisnya tidak diverifikasi langsung pada penyusunan ini.

| Referensi PRD | Bukti wajib |
| --- | --- |
| AT-017–019 | Matriks dua role, actor tidak dapat dipalsukan, dan akun tanpa role ditolak. |
| AT-020–022 | Identity/JWT, penolakan token invalid/expired, seeding idempotent tanpa secret terekspos. |
| AT-023–025 | Item-Lot langsung, enum/file LotAdministrativeStatus, React melalui API, domain bebas UI/database. |

### 10.6 Strategi verifikasi

| Lapisan | Skenario bermakna |
| --- | --- |
| Unit domain | Boundary expiry dan warning; eligibility; low stock per Item; FEFO dengan tie; jumlah tidak cukup; quantity nol/negatif; konsumsi/disposal; lifecycle. |
| Integrasi PostgreSQL | Transaksi multi-Lot berhasil/gagal, dua writer pada Lot sama, perubahan status saat consume, lifecycle Job bersamaan, keunikan akun, register dengan assignment role gagal, perlindungan admin terakhir. |
| API/RBAC | Endpoint publik/terlindungi, 401/403, pemalsuan actor, field terlarang, filter audit, error Code, row version, role dicabut pada JWT yang sudah terbit. |
| React-API/E2E | Register/Login kedua role, administrasi Item/Lot, Job/preview/consume, disposal/status, audit, profil/user, conflict dan hasil commit tidak pasti. |
| UX | Viewport 360/768/1280, keyboard/fokus/label, empty/loading/error, response pencarian terbalik, refresh/logout/pergantian akun. |
| F-09/F-10 | Dijalankan jika fitur masuk release, setelah kontrak/policy fase terkait disepakati. |

Boundary expiry bergantung D-05, low stock D-06, lifecycle D-07, disposal/status D-08, dan konsumsi/idempotency D-09. Mock, build berhasil, atau screenshot UI tidak cukup untuk membuktikan atomicity dan authorization.

## 11. Matriks keterlacakan

| Fitur PRD | Kebutuhan SRS | API/antarmuka | Acceptance utama | Keputusan terkait |
| --- | --- | --- | --- | --- |
| F-01 | FR-01-01–09 | Bagian 7.11; FE-04/05 | AC-F01-01–03, AC-FE-07/08 | D-03/04/05/08 |
| F-02 | FR-02-01–06; BR-06/08 | Evaluasi pada read/preview/consume | AC-F02-01–03, AC-F06-03 | D-04/05 |
| F-03 | FR-03-01–07 | Bagian 7.3; FE-03/04/05 | AC-F03-01, AC-API-01, AC-FE-05/06 | D-05/06; CountCriteria |
| F-04 | FR-04-01–06 | Bagian 7.4; FE-06 | AC-F04-01/02, AC-API-02, AC-FE-09 | D-04/07 |
| F-05 | FR-05-01–07 | Bagian 7.5; FE-07 | AC-F05-01–03, AC-FE-10 | D-04/05/07/09 |
| F-06 | FR-06-01–08; BR-11–14 | Bagian 7.6; FE-07 | AC-F06-01–03, AC-X-01/02, AC-API-03, AC-FE-11/12 | D-03/04/05/07/09 |
| F-07 | FR-07-01–08 | Bagian 7.7; FE-08 | AC-F07-01/02, AC-X-01/02, AC-API-04, AC-FE-13 | D-03/04/08 |
| F-08 | FR-08-01–07 | Bagian 7.8; FE-09 | AC-F08-01, AC-API-05, AC-FE-14 | D-04/08/10 |
| F-09 | FR-09-01–04 | Bagian 7.9; FE-12 | AC-API-06 | Scope release Should |
| F-10 | FR-10-01–06 | Bagian 7.10 | AC-API-07 | Persetujuan fase/format/akses/kapasitas |
| F-11 | FR-11-01–08; AUTH-01–06 | Bagian 7.2; FE-10/11 | AC-F11-01–06, AC-FE-15, AC-API-08 | D-04/11; multi-role |
| F-12 | FR-12-01–05; AUTH-02 | Bagian 7.2; FE-01 | AC-F12-01–05, AC-FE-03, AC-API-08 | D-04/11 |
| F-13 | FR-13-01–08 | Bagian 7.2; FE-01/02 | AC-F13-01–04, AC-FE-01/02/04 | D-11 |
| F-14 | FR-14-01–04; UI-01–23 | Seluruh halaman Bagian 8 | AC-FE-01–18 | D-02/04–11 sesuai alur |

Ketentuan lintas fitur: NFR-01–16 dan BR-01–16. Acceptance yang melibatkan policy terbuka harus diperbarui setelah keputusan final dicatat, tanpa mengubah ID rujukan sumber secara diam-diam.

## 12. Keputusan terbuka dan dependensi

### 12.1 Register keputusan PRD

| ID | Status menurut substansi PRD | Keputusan/detail | Owner/gate |
| --- | --- | --- | --- |
| D-01 | DECIDED | Dua role scientis dan WarehouseAdmin, dengan matriks akses Bagian 3. | Product Owner/perwakilan lab; sudah ditetapkan. |
| D-02 | Stack DECIDED; rincian OPEN | React, ASP.NET Core Web API .NET 8, HTTP/JSON; tooling, route detail yang belum ada, deployment mengikuti penyelarasan. | Engineering untuk rincian implementasi. |
| D-03 | PARTIAL | PostgreSQL dan EF Core final; mapping RowVersion, deployment migration, dan operasional terbuka. | Engineering/DevOps; sebelum persistence terkait difinalisasi. |
| D-04 | OPEN | Tipe/ordering ID, precision/scale, panjang/nullability, uniqueness, pecahan unit, registrasi expired, edit historis. | Product Owner/perwakilan lab dengan Engineering; sebelum schema/validasi terkait. |
| D-05 | OPEN / PROPOSED | Cutoff expiry, timezone untuk DateOnly, warning window; usulan instant UTC dan warning inklusif 24 jam bukan keputusan final. | Product Owner/perwakilan lab; sebelum expiry engine dan boundary test final. |
| D-06 | OPEN / PROPOSED | Usulan usable stock < MinimumStock; putuskan perlakuan nilai sama. | Product Owner/perwakilan lab; sebelum low-stock rule final. |
| D-07 | OPEN / PROPOSED | Consume InProgress, Cancel Draft/InProgress, terminal tidak dibuka, tanpa stock reversal otomatis. | Product Owner/perwakilan lab; sebelum lifecycle/consume terkait final. |
| D-08 | OPEN | Disposal expired/blocked/quarantined, full disposal, release, reason dan audit status. | Product Owner/perwakilan lab; sebelum status/disposal final. |
| D-09 | OPEN / PROPOSED | Kepatuhan pada preview, pilihan manual Lot, multi-Item, idempotency; usulan satu Item dan tolak LotId ganda. Tanpa silent substitution tetap invariant baseline. | Product Owner/perwakilan lab dengan Engineering; sebelum kontrak commit/retry final. |
| D-10 | OPEN | Kapasitas, latency, availability, retention audit, backup/RPO/RTO, logging provider. | Engineering/DevOps; sebelum gate operasional/release. |
| D-11 | PARTIAL | Identity/JWT, register scientis, admin user/role final; lifetime/refresh/revocation, penyimpanan token, password/lockout, pemetaan Scientist DTO terbuka. | Client dan Security/IT; sebelum aspek sesi/security terkait final. |

### 12.2 Gap penyelarasan yang teridentifikasi

ID `GAP-SRS-*` berikut adalah catatan penyusunan SRS, bukan ID keputusan baru dari PRD. Catatan ini tidak menambahkan scope produk.

| ID | Gap | Tindakan yang diperlukan |
| --- | --- | --- |
| GAP-SRS-01 | Kontrak CRUD F-01 belum dirinci pada Lampiran D/F. | Lengkapi method/route, DTO, status/error, search/filter, pagination, dan concurrency pada OpenAPI. |
| GAP-SRS-02 | Fitur deactivate Item ada, tetapi atribut status nonaktif belum ada pada model kanonis. | Putuskan representasi dan dampaknya pada baca/registrasi/eligibility bersama D-04; jangan mengarang field atau rule. |
| GAP-SRS-03 | Multi-role boleh hanya jika disetujui; schema assignment role belum final. | Putuskan cardinality membership dan perilaku UI/response yang konsisten dengan matriks akses. |
| GAP-SRS-04 | Casing respons akun dan contoh inventaris belum seragam; beberapa DTO akun belum lengkap. | Selaraskan penamaan/schema OpenAPI, termasuk role/status dan payload admin, tanpa membuka field sensitif. |
| GAP-SRS-05 | CountCriteria dashboard, maksimum pagination, dan urutan daftar masih rancangan. | Kunci definisi hitungan, schema kriteria, batas page, serta urutan deterministik tiap daftar. |
| GAP-SRS-06 | Race lifecycle Job harus aman, tetapi mekanisme persistence-nya belum ditetapkan. | Rancang locking/isolation/concurrency pada transaksi Job-Consumption dan verifikasi PostgreSQL. |
| GAP-SRS-07 | Audit tersedia, tetapi korelasi request/idempotency untuk commit yang tidak pasti belum final. | Putuskan mekanisme D-09; jangan menganggap audit list saja menjamin identifikasi satu command secara unik. |
| GAP-SRS-08 | F-10 masih fase lanjut, termasuk format, tujuan integrasi, akses, dan batas export. | Finalisasi kontrak dan kriteria penerimaan sebelum dimasukkan ke release; tetap Nice sampai scope berubah resmi. |
| GAP-SRS-09 | Pemetaan DTO Scientist dan rujukan struktur historis belum diverifikasi terhadap repository. | Selaraskan penamaan/model saat repository tersedia; pertahankan role kanonis scientis. |

Bagian yang tidak bergantung gap dapat dilanjutkan. Gap yang memengaruhi tahap berjalan harus diselesaikan sebelum perilaku terkait dinyatakan final atau diterima.

## 13. Risiko, target, dan Definition of Done

### 13.1 Risiko utama

| Risiko | Dampak | Mitigasi dan bukti |
| --- | --- | --- |
| Boundary expiry salah | Bahan expired terpakai atau bahan valid terblokir. | Finalisasi D-05; uji cutoff, timezone, warning, expiry antara preview dan commit. |
| Race stock/status/Job | Remaining negatif atau audit/lifecycle tidak sesuai. | Token, transaksi atomik, strategi concurrency Job, integration PostgreSQL dua writer. |
| FEFO tidak deterministik | Lot prioritas salah. | Urutan expiry/received/Id dan uji tie. |
| Status dicampur | UI/logic menyamakan expiry, low stock, dan status administratif. | Enum/read model terpisah, label jelas, evaluasi server. |
| Hak akses lama tetap berlaku | Akun nonaktif/role dicabut masih dapat mengakses data. | Pemeriksaan state/version setiap request; uji JWT yang sudah terbit. |
| Scope atau schema ditetapkan dari asumsi | Implementasi menyimpang dari keputusan produk. | Register keputusan dan gap; OpenAPI serta model diselaraskan sebelum gate terkait. |
| Retry mutasi ambigu | Konsumsi/disposal ganda. | Tidak retry otomatis; finalisasi D-09 dan mekanisme rekonsiliasi. |

### 13.2 Target keberhasilan

| Metrik | Target PRD / bukti |
| --- | --- |
| Pencegahan expired-use | Nol jalur sukses yang memakai Lot expired pada pengujian penerimaan. |
| Ketepatan FEFO | Seluruh skenario yang ditetapkan mengikuti expiry/received/Id. |
| Integritas stock | Tidak ada remaining negatif; invariant dan integration test lulus. |
| Kelengkapan audit | Seluruh Consumption/Disposal sukses memiliki actor, waktu, quantity, serta Job/Reason yang tepat dalam transaksi atomik. |
| Sinyal low stock | Perhitungan usable per Item dan threshold yang disepakati benar. |
| Kualitas delivery | CI/build/test lulus, warnings sebagai error, target static warnings nol, bukti UX/integrasi tersedia. |

### 13.3 Definition of Done

Release MVP dapat diterima apabila:

1. Seluruh kebutuhan Must dan acceptance yang relevan lulus pada environment yang disepakati, termasuk akun dan frontend React.
2. Tidak ada jalur sukses menggunakan Lot expired/blocked, remaining negatif, atau transaksi inventory tanpa audit.
3. FEFO deterministik; atomicity multi-Lot dan concurrency dibuktikan melalui PostgreSQL nyata.
4. Kedua role telah diuji dari UI sampai API, termasuk akses ditolak, perubahan role/nonaktif, dan failure path.
5. Model Item-Lot, DTO, mapping, repository, migration, dan OpenAPI konsisten; gap yang memengaruhi release terselesaikan.
6. Responsivitas, keyboard, validasi, empty/loading/error, dan conflict handling memenuhi AC-FE terkait.
7. Requirement direview Product Owner, perwakilan scientis, WarehouseAdmin, dan Engineering dengan masukan Lab Manager/QA.
8. Bukti uji, dokumentasi operasional singkat, serta keputusan relevan tersedia. F-09/F-10 hanya menjadi gate jika masuk scope release.

SRS ini sendiri belum memenuhi atau membuktikan gate aplikasi tersebut; statusnya adalah spesifikasi kebutuhan yang siap ditinjau dan diturunkan menjadi backlog, kontrak OpenAPI, serta pengujian.

## 14. Tahapan implementasi dan kendali perubahan

| Tahap | Fokus | Output yang ditargetkan |
| --- | --- | --- |
| 0. Alignment | Keputusan terbuka yang memengaruhi tahap berjalan, schema/cutoff, kontrak API. | Backlog, keputusan, model/DTO/OpenAPI yang selaras. |
| 1. Domain core | Item, Lot, expiry, eligibility, low stock, FEFO, lifecycle Job. | Domain murni dan unit test kritis. |
| 2. Persistence/application | EF Core/PostgreSQL, repository/UoW, mapping/validator, Identity/JWT, transaksi. | Implementasi layanan dan integration test. |
| 3. Operational UI | Shell/akun, Item/Lot/dashboard, Job/FEFO/Consumption, disposal/audit/user. | React terintegrasi API dan bukti AC-FE. |
| 4. Hardening | CI, static analysis, coverage kritis, usability, observability. | Bukti acceptance dan kesiapan operasional sesuai scope. |
| 5. Expansion | Quick actions dan export/integrasi/kanal lanjutan yang disetujui. | Release tambahan sesuai prioritas dan keputusan baru. |

Perubahan relasi Item-Lot, eligibility, FEFO, boundary transaksi, hak akses, atau scope harus dicatat bersama dampaknya pada FR/BR/NFR/UI, DTO/schema, endpoint, dan acceptance. Jangan menyatakan fitur selesai hanya karena build berhasil.

## 15. Riwayat dokumen

| Versi | Tanggal | Perubahan |
| --- | --- | --- |
| 1.0 | 11 September 2026 | Menyusun srs.md dari PRD v1.4; memuat seluruh F-01–F-14, role scientis/WarehouseAdmin, endpoint Lampiran D/F, data, aturan transaksi, frontend, nonfungsional, acceptance, keterlacakan, dan keputusan terbuka. |
