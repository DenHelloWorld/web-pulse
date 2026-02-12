# 🛰️ Web Pulse: Heaven vs Hell
### *Real-Time Sentiment Battleground Visualization*

## 1. Project Concept
**Web Pulse** is an interactive, full-screen data visualization that transforms the internet's emotional state into a physical "battle." By analyzing live streams of comments through a **Machine Learning** pipeline, the system generates physical entities that collide, merge, or annihilate based on their sentiment score.

The core objective is to showcase high-performance web engineering by merging an **Indie Game Engine** with a **Modern Enterprise Framework**.

---

## 2. Technical Architecture

### **Backend (.NET Core & ML)**
* **Engine:** .NET 9 Web API.
* **Intelligence:** **ML.NET** for Binary Classification (Sentiment Analysis).
* **Communication:** **SignalR Hub** for low-latency, full-duplex WebSocket streaming.
* **Worker:** Background Service "Data Harvester" for external API simulation.

### **Frontend (Angular & Phaser)**
* **Framework:** **Angular 18+**.
* **State:** **Angular Signals** for reactive UI updates and optimized state management.
* **Graphics:** **Phaser 3** (Game Engine) rendering to a full-screen WebGL/Canvas.
* **Physics:** **Arcade Physics** engine managing collisions, mass, and velocity.

---

## 3. Data Flow Specification
1.  **Ingestion:** Backend receives a string (e.g., *"This is amazing!"*).
2.  **Scoring:** ML.NET assigns a score $S \in [-1.0, 1.0]$.
3.  **Transmission:** SignalR sends a `Pulse` object to the Frontend.
4.  **Reaction:** * **Angular** updates the "Global Toxicity" Signal.
    * **Phaser** spawns a sphere:
        * 🔴 **Red (Hate):** Heavy mass ($setMass(10)$), aggressive movement, fire particles.
        * 🔵 **Blue (Love):** High velocity, glowing trails, seeking to neutralize red spheres.

---

## 4. Key Engineering Features
* **High-Performance Rendering:** Using Phaser's Canvas/WebGL bridge to maintain 60 FPS regardless of DOM complexity.
* **Full-Screen Immersion:** Phaser `Scale.RESIZE` mode for a responsive, cinematic experience across all devices.
* **Event-Driven Bridge:** Decoupling Angular's business logic from the game's render loop via an observable-based event bus.

---

## 5. Development Roadmap
- [ ] **Infrastructure:** Set up .NET Hub and Angular Boilerplate.
- [ ] **Physics Scene:** Implement the "Digital Void" in Phaser with collision logic.
- [ ] **ML Integration:** Integrate the `SentimentModel.zip` into the .NET pipeline.
- [ ] **Visual Juice:** Add neon shaders and particle emitters.
- [ ] **Deployment:** Containerize via **Docker** and deploy to a Linux VPS.
