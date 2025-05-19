# 🪖 2D Çok Oyunculu Tank Oyunu

Bu proje, Marmara Üniversitesi Teknoloji Fakültesi Bilgisayar Ağları dersi kapsamında geliştirilmiş gerçek zamanlı, çok oyunculu bir 2D tank oyunudur. Oyun, Unity oyun motoru kullanılarak C# diliyle geliştirilmiş ve ağ iletişimi UDP protokolü üzerinden sağlanmıştır.

## 🧠 Proje Amacı

- Gerçek zamanlı çok oyunculu bir oyun geliştirmek  
- Unity ile UDP tabanlı ağ haberleşmesini entegre etmek  
- İstemci-sunucu mimarisini uygulamalı olarak göstermek  

---

## ⚙️ Kullanılan Teknolojiler

| Teknoloji        | Açıklama                     |
|------------------|------------------------------|
| Unity            | Oyun motoru                  |
| C#               | Programlama dili             |
| UDP (System.Net) | Ağ iletişimi protokolü       |

---

## 🏗️ Sistem Mimarisi

### İstemci-Sunucu Yapısı

- **Sunucu**: Oyunun merkezidir. Oyuncu bilgilerini işler, senkronizasyon sağlar.
- **İstemciler**: Oyuncuların tanklarını kontrol eder, sunucu ile haberleşir.

### Ana Sınıflar

- `NetworkManager`: UDP bağlantılarını kurar ve mesaj trafiğini yönetir  
- `PlayerTank`: Oyuncu tanklarının kimliğini ve görünümünü kontrol eder  
- `PlayerMovement`: Tank hareketlerini işler ve ağ üzerinden iletir  
- `PlayerShoot`: Ateş etme mekanizmasını kontrol eder  
- `Bullet`: Mermilerin hareket ve çarpışma davranışlarını yönetir  
- `TankHealth`: Tankların sağlık durumunu takip eder

---

## 🔄 Mesaj Türleri (UDP Üzerinden)

- `CONNECT`: Yeni istemci bağlantı isteği  
- `SPAWN`: Oyuncu oyuna eklendiğinde tüm istemcilere gönderilir  
- `MOVE`: Tankın hareket verisi  
- `SHOOT`: Ateş etme verisi  
- `DAMAGE`: Hasar ve sağlık güncellemesi  

---

## 📸 Uygulama Görüntüleri

### Giriş Ekranı
*Network üzerinden bağlantı sağlanacak ekran.*

### Oyun İçi (3 Oyunculu Sahne)
*Tanklar arası etkileşim ve eş zamanlı savaş sahnesi.*

---

## 🧾 Sonuç

Bu proje, UDP protokolü ile düşük gecikmeli çok oyunculu oyun geliştirmenin temel yapı taşlarını öğrenmek için değerli bir deneyim sunmuştur. Unity ile ağ haberleşmesi üzerine yapılan bu çalışma, oyun geliştirmenin yanı sıra istemci-sunucu iletişimi konularında da güçlü bir uygulamalı öğrenme ortamı sağlamıştır.

---

## 📁 Projeyi Çalıştırmak

1. Unity ile projeyi açın (`Unity 2021.3.x` veya üzeri önerilir)  
2. `NetworkUI` sahnesini açarak oyunu başlatın  
3. Bir makinede **Server**, diğerlerinde **Client** olarak başlatın  
4. Aynı yerel ağda olduğunuzdan emin olun  

---

## 👥 Katılımcılar

- [kerem-apaydin](https://github.com/kerem-apaydin) 
- [arifbatuhanbahar](https://github.com/arifbatuhanbahar)
