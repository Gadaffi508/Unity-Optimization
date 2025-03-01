# Unity DOTS Optimization Project

Bu proje, **Unity DOTS (Data-Oriented Technology Stack)** kullanılarak optimize edilmiş bir oyun performansı elde etmeyi amaçlamaktadır. DOTS sistemleri, performans açısından yüksek verimli bir kod yapısı sunar ve CPU üzerindeki yükü azaltarak daha akıcı bir oyun deneyimi sağlar.

![Ekran görüntüsü 2025-03-01 112405](https://github.com/user-attachments/assets/6e22fe5d-ae09-4c8a-b094-0b641c7d5b56)


## 🚀 Proje İçeriği

- **ECS (Entity Component System)** kullanarak bileşen bazlı bir yapı.
- **Job System** ile çoklu iş parçacıklı hesaplamalar.
- **Burst Compiler** sayesinde yüksek performanslı kod yürütme.
- **Physics ve AI optimizasyonları** için DOTS kullanımı.

  ![Ekran görüntüsü 2025-02-28 173155](https://github.com/user-attachments/assets/e79d8012-f5ee-49c9-b441-a0af30f8e85a)


## 📂 Proje Kurulumu

1. **Unity Versiyonu:** Unity 2022.3+ sürümünü kullanmanız önerilir.
2. **Gereksinimler:**
   - **Entities Package** (`com.unity.entities`)
   - **Burst Package** (`com.unity.burst`)
   - **Jobs Package** (`com.unity.jobs`)
3. **Projeyi Klonlayın:**
   ```sh
   git clone https://github.com/kullaniciadi/unity-dots-optimization.git
   cd unity-dots-optimization
   ```
4. **Unity ile Açın:** Unity Hub veya Unity Editor üzerinden açabilirsiniz.
5. **Gerekli Paketleri Yükleyin:** Unity Package Manager'dan eksik olanları tamamlayın.

![Ekran görüntüsü 2025-03-01 120934](https://github.com/user-attachments/assets/42dccda8-2304-4554-8cf5-bca4944becdc)


## 📜 Kullanım

- **ECS ile Sistemlerin Kullanımı**
  - `ComponentSystem` veya `SystemBase` türetilerek özel sistemler oluşturulabilir.
  - `IJobForEach` veya `IJobEntityBatch` kullanarak optimize edilmiş hesaplamalar yapılabilir.

Örnek bir ECS sistemi:
```csharp
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public partial struct MoveSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithEntityAccess())
        {
            transform.ValueRW.Position += new float3(0, 0, 1) * SystemAPI.Time.DeltaTime;
        }
    }
}
```

## 🛠 Optimizasyon Teknikleri

- **Burst Compiler kullanarak kod performansını artırın.**
- **Paralel işlemler için `Job System` kullanın.**
- **`NativeArray`, `NativeList` gibi veri yapılarını tercih edin.**
- **Hafıza erişimini minimize edin ve cache dostu kod yazın.**

## 🤝 Katkıda Bulunma

Eğer projeye katkıda bulunmak istiyorsanız:
1. **Fork yapın** 🍴
2. **Yeni bir dal (branch) oluşturun** 🌿
3. **Değişiklikleri yapın ve commit atın** 💾
4. **Pull Request gönderin** 🔄

## 📜 Lisans

Bu proje [MIT Lisansı](LICENSE) ile lisanslanmıştır.

---
📌 **Repo sahibi:** [Kullanıcı Adı](https://github.com/Gadaffi508)
