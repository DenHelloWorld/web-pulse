# 🛰️ Web-Pulse Global Backend Implementation Plan

## 1. Core Architecture (Provider-Consumer Pattern)
The goal is to move from dummy data to a scalable multi-stream engine using `System.Threading.Channels`.

* **`ICommentProvider` Interface**: Standardizes how data is fetched regardless of the source.
* **`StreamAggregator`**: A background worker that runs multiple providers in parallel using `Task.WhenAll`.
* **High-Performance Queue**: Using `System.Threading.Channels` to pass strings from providers to the ML engine without blocking.



---

## 2. Intelligence Layer (ML.NET)
Replacing `Random.Shared.Next()` with a real-time sentiment analysis engine.

* **`SentimentAnalysisService`**: A Singleton service that loads the pre-trained `.zip` model once.
* **Prediction Pipeline**:
    * Input: `string text`
    * Processing: Text featurization (normalization, tokenization).
    * Output: `float SentimentScore` (Range: -1.0 to 1.0).

---

## 3. Global Data Providers
The "tentacles" of the system that grab real-world data.

| Provider | Method | Logic |
| :--- | :--- | :--- |
| **Reddit** | `HttpClient` | Polls `/r/all/new.json` every 5-10 seconds. |
| **RSS News** | `SyndicationReader` | Parses headlines from Reuters, BBC, and AP News. |
| **YouTube Chat** | `LiveChat API` | Hooks into the most active news stream's chat ID. |

---

## 4. Real-time Communication (SignalR Hub)
Connecting the analyzed data to the Phaser frontend.

* **Throttling Logic**: Prevents "Spam Death" by limiting the hub to a maximum of $X$ messages per second.
* **Enhanced Payload**: Sending rich objects to Phaser:
    ```json
    {
      "sentiment": 0.75,
      "source": "Reddit",
      "text": "New battery tech discovered!",
      "timestamp": "2026-02-12T14:15:00Z"
    }
    ```



---

## 5. Administration & Monitoring
* **`/api/stats`**: Returns real-time metrics (Messages per minute, current global mood).
* **Dynamic Controls**: Toggle specific providers (e.g., "Mute Reddit") via HTTP POST.

---

## 🛠 Tech Stack
* **Runtime**: .NET 9
* **Real-time**: SignalR (WebSockets)
* **Machine Learning**: ML.NET
* **Networking**: HttpClient + Polly (for resilient retries)