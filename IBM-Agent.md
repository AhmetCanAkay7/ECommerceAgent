
## ÖDÜL ALAN PROJE SUNUMU

Session 5: Enterprise Agentic Application with MCPs on Java Server
**Konuşmacı:** Selçuk Şentürk
**Ana konu:** Kurumsal bir iş sürecini agentic mimari ile çözmek; MCP tabanlı tool çağrılarıyla birden fazla sistemi güvenli ve izlenebilir şekilde orkestre etmek

## Key Points:

1. Sistemin temel fikri şu: (Orchestrator Agent ve kullanabildiği toollar)
   + Kullanıcı bir hasar talebiyle geliyor
   + Agent bu talebi anlamaya çalışıyor
   + İhtiyaç duyduğu kurumsal sistemlere doğrudan kod yazmadan değil, **MCP tool’ları** üzerinden bağlanıyor
   + Sırasıyla:
     + poliçe bilgisi çekiliyor
     + hasar/fraud kontrolü yapılıyor
     + regülasyon uyumu kontrol ediliyor
     + ödeme onayı veriliyor
     + müşteriye yanıt hazırlanıyor

   Yani burada LLM tek başına “cevap yazan model” değil.
   Asıl rolü:
   + hangi tool çağrılacak,
   + hangi sırayla çağrılacak,
   + çıkan sonuçlarla nasıl karar verilecek  
    bunları orkestre etmek. Bu yüzden bu sunum aslında tam olarak bir **agentic orchestration** örneği.
    
2. Çözmeye çalıştıkları business problem neydi?
   **Sigorta Hasar Talep Süreci**
   
   Eski durumda görülen ana darboğazlar:
   + Veritabanı, Oracle, hasar sistemi ve IBM CICS gibi sistemler birbiriyle entegre ama uzmanların hepsine ayrı ayrı giriş yapması gerekiyor.
   + süreç uzun, insan eforu fazla, hata riski yüksek, entegrasyon maliyeti yüksek, operasyon yavaş
     
3. Neden MCP kullanmışlar?
   MCP öncesi dünya'da her AI modeli için özel entegrasyon yazmak gerekiyordu. MCP ile tek standart protokol üzerinden tool’ları expose ediyorsun. Ajan değişse bile altyapı korunabiliyor.
 
4. Tool Tanımları Nasıl Yapıldı?
   + tool adı
   + açıklaması
   + input argümanları
   + description
   + dönüş tipi

5. Karşılaştıkları asıl teknik zorluklar neler?
   A) **Yanlış tool seçimi**: Tool açıklamaları birbirine yakınsa model yanlış tool çağırabiliyor, Bir tool birden fazla iş yapıyorsa karışıklık artıyor, Tool sayısı büyüdükçe seçim problemi zorlaşıyor.
   
   Çözüm: 
   + Scope’u net tool açıklamaları
   + Tool’ların tek iş yapması
   + Tool’ları gruplara ayırma
   + Gerekirse önce grup seçimi, sonra tool seçimi (Tool katalogu büyüdükçe router / grouping şart olur)

   B) **Timeout problemi**: Kurumsal sistemlerde bazı servisler yavaş olabilir. LLM tarafı tool çağrısının sonucunu bekliyor. Tool süresinde dönmezse senaryo bozuluyor.
     
   Çözüm:
   + timeout ayarlarını optimize etmek
   + non-blocking tool
   + streaming response yaklaşımı
   + Agentic sistemlerde sadece model değil, altyapı latency’si de başarıyı belirler.

   C) **Session / state yönetimi:** MCP akışında bir adım fail olursa tüm iş akışı baştan başlamak zorunda kalabilir.
   
   Çözüm:
   + session store / state yönetimi
   + sessionId ile context tutmak
   + Production-grade agent = stateful orchestration ister.

   D) **Parametre halüsinasyonu**: LLM yanlış claimId, yanlış format, hatalı alanlar üretebiliyor.
   
   Çözüm:
   + JSON schema strict validation
   + tool entry seviyesinde doğrulama
   + Her tool girişi doğrulanmalı

 6. Güvenlik neden burada bu kadar merkezi?
   + Tool çağrıları doğrudan iş etkisi yaratıyor. (Ödeme onayı, fraud kararı vs.)
   + Güvenlik > Kurum Uyumluluğu > Geliştirici Deneyimi > Performans şeklinde öncelik sıralaması

    Güvenlikten çıkan dersler:
   + Her tool çağrısında authN / authZ gerekli
   + Role-based erişim gerekli
   + Kim hangi tool’u çağırdı izlenmeli
   + Governance sonradan eklenmemeli, baştan tasarlanmalı
   + Agentic AI sadece akıllı davranmak değil, izlenebilir davranmak zorunda.

7. LLM'ler deterministic değil bu yüzden onu sınırlayan, doğrulayan ve gözlemleyen katmanlar kurarsın.
   Bunun için kullanılan kontrol katmanları:
   + tool sınırları
   + auth
   + schema validation
   + idempotency
   + session state
   + timeout management
   + governance
   + human-in-the-loop
   
   
   