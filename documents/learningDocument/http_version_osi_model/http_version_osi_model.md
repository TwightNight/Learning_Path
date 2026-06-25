## HTTP Versions & OSI Model

---

## OSI Model First (The Foundation)

All HTTP versions ride on top of the OSI stack. Understanding the layers explains *why* each HTTP version was designed the way it was.

```
Layer 7  │ Application  │ HTTP, DNS, FTP, SMTP
Layer 6  │ Presentation │ TLS/SSL, encryption, compression
Layer 5  │ Session      │ Connection management, auth sessions
Layer 4  │ Transport    │ TCP, UDP — reliability & ports
Layer 3  │ Network      │ IP — routing packets across networks
Layer 2  │ Data Link    │ Ethernet, WiFi — node-to-node on same network
Layer 1  │ Physical     │ Cables, radio waves, bits on wire
```

Each layer only talks to the layer directly above/below it. When you make an HTTP request, data travels **down** all 7 layers on the sender, across the wire, then **up** all 7 layers on the receiver.

### What Each Layer Actually Does

**Layer 1 — Physical**
Raw bits transmitted as electrical signals, light pulses, or radio waves. Defines voltage levels, cable specs, pin layouts. No concept of addresses or meaning — just 0s and 1s.

**Layer 2 — Data Link**
Groups bits into **frames**. Adds MAC addresses for node-to-node delivery on the *same* local network. Ethernet switches operate here. Error detection via CRC checksums. Two sublayers: LLC (logical link control) and MAC (media access control).

**Layer 3 — Network**
Adds **IP addresses** and handles routing between different networks. Routers operate here. Breaks data into **packets** and finds a path across multiple hops to reach the destination. No guarantee of delivery or order.

**Layer 4 — Transport**
Adds **ports** to identify which application gets the data. Two main protocols:
- **TCP** — connection-oriented, guarantees delivery, ordering, and error recovery via handshake + ACKs
- **UDP** — connectionless, fire-and-forget, no guarantees, low overhead

**Layer 5 — Session**
Manages opening, maintaining, and closing communication sessions. In practice, TCP/TLS handles most of this. Concepts like session tokens and auth cookies live conceptually here.

**Layer 6 — Presentation**
Data translation, encryption (TLS), and compression. Ensures data sent by one system can be read by another. HTTPS encryption happens here.

**Layer 7 — Application**
The protocol the application uses to communicate. HTTP, WebSockets, DNS, FTP all live here. This is where your browser and server exchange meaningful messages.

---

## HTTP/1.0 (1996)

### How it works
One request = one TCP connection. The connection closes after every response.

```
Client                    Server
  │── TCP Handshake ──────▶│
  │── GET /index.html ────▶│
  │◀─ 200 OK + HTML ───────│
  │── TCP Close ───────────│

  │── TCP Handshake ──────▶│   ← new connection for EVERY resource
  │── GET /style.css ─────▶│
  │◀─ 200 OK + CSS ────────│
  │── TCP Close ───────────│
```

### Problems
- TCP 3-way handshake overhead on every single request
- No persistent connections → massive latency on pages with many assets
- No host header → one IP = one website

---

## HTTP/1.1 (1997, still dominant)

### Key improvements

**Persistent connections (`keep-alive`)** — TCP connection stays open and reuses across multiple requests.

**Pipelining** — theoretically send multiple requests without waiting for each response. In practice, broken due to **Head-of-Line (HOL) blocking**: if request #1 is slow, requests #2 and #3 wait behind it in the queue regardless.

```
Client                         Server
  │── TCP Handshake ──────────▶│
  │── GET /index.html ────────▶│
  │◀─ 200 OK + HTML ───────────│
  │── GET /style.css ─────────▶│  ← same connection, no handshake
  │◀─ 200 OK + CSS ────────────│
  │── GET /app.js ────────────▶│
  │◀─ 200 OK + JS ─────────────│
```

**Chunked transfer encoding** — server can stream a response in chunks without knowing total size upfront.

**Host header** — multiple virtual hosts on one IP address. Enables modern web hosting.

### Problems that remain
- **HOL blocking** at the application layer — one slow response blocks the queue
- **Headers are uncompressed, verbose, and repetitive** — cookies and user-agent sent on every request
- Browsers work around HOL by opening **6 parallel TCP connections per domain** — wasteful
- No server push — server can only respond, never initiate

---

## HTTP/2 (2015)

HTTP/2's entire purpose: fix HTTP/1.1's performance problems **without changing semantics** (methods, status codes, headers stay the same).

### Core concept: Binary Framing

HTTP/1.1 is plain text. HTTP/2 is **binary**, split into small frames.

```
HTTP/1.1 (text):              HTTP/2 (binary frames):
GET /index.html               HEADERS frame  (stream 1)
Host: example.com             DATA frame     (stream 1)
Accept: text/html             HEADERS frame  (stream 3)
                              DATA frame     (stream 3)
```

### Multiplexing — the big win

Multiple requests and responses **interleave on a single TCP connection** using **streams**. Each stream has an ID. Frames from different streams can be interleaved freely.

```
Single TCP connection:
  ──[Stream 1: HTML]──[Stream 3: CSS]──[Stream 1: HTML]──[Stream 5: JS]──▶
      request/response frames interleaved, no blocking each other
```

No more 6 parallel connections hack. One connection handles everything concurrently.

### Header Compression (HPACK)

Headers are compressed using a **shared lookup table** between client and server. If you send `User-Agent` in request #1, request #2 just references an index number. Saves significant bandwidth on cookie-heavy APIs.

### Server Push

Server can proactively send resources the client hasn't asked for yet:

```
Client: GET /index.html
Server: here's index.html
         + PUSH /style.css   ← server knows you'll need this
         + PUSH /app.js      ← sent before client even parses HTML
```

### Stream Prioritization

Clients can assign weights and dependencies to streams, telling the server to send critical CSS before lower-priority images.

### Still built on TCP — the remaining problem

HTTP/2 solves **application-layer HOL blocking**, but TCP itself still has **transport-layer HOL blocking**. TCP is an ordered byte stream — if one packet is lost, **all streams on that connection stall** waiting for retransmission, even streams whose data arrived fine.

On a lossy network (mobile, WiFi), HTTP/2 can actually perform *worse* than HTTP/1.1's multiple TCP connections.

---

## HTTP/3 (2022, QUIC-based)

### Root cause fix: replace TCP with QUIC

HTTP/3 drops TCP entirely and runs on **QUIC** (Quick UDP Internet Connections), which runs on **UDP**.

```
HTTP/1.1:  HTTP → TCP → IP
HTTP/2:    HTTP → TCP → IP
HTTP/3:    HTTP → QUIC → UDP → IP
           (QUIC bundles: streams + reliability + TLS 1.3)
```

QUIC reimplements the reliable, ordered delivery that TCP provides, but **per stream independently**. A lost packet only blocks the stream it belongs to, not all streams.

### 0-RTT and 1-RTT Connection Setup

TCP requires a 3-way handshake, then TLS adds another 1-2 round trips. That's 2-3 RTTs before any data flows.

QUIC combines the transport and crypto handshakes:

```
HTTP/1.1 + TLS:               HTTP/3 + QUIC:
  SYN                           Initial (crypto)
  SYN-ACK                       ◀ Handshake
  ACK                           Request  ← data on 3rd message
  ClientHello
  ◀ ServerHello
  Finished
  Request  ← data on 7th message

First request: ~3 RTT           First request: ~1 RTT
Resuming:      ~2 RTT           Resuming:       0 RTT
```

**0-RTT resumption**: if client has visited before, it can send data in the very first packet using cached session keys.

### Connection Migration

TCP connections are identified by `(src IP, src port, dst IP, dst port)`. If your IP changes (switching from WiFi to 4G), the connection breaks.

QUIC connections use a **Connection ID** — a random token independent of IP/port. Your download continues seamlessly when switching networks. Critical for mobile users.

### TLS 1.3 is Mandatory and Baked In

QUIC has encryption built-in at the protocol level. You can't run unencrypted HTTP/3. The handshake metadata itself is encrypted (preventing middlebox interference).

---

## Full Comparison

| Feature | HTTP/1.1 | HTTP/2 | HTTP/3 |
|---|---|---|---|
| **Transport** | TCP | TCP | QUIC (UDP) |
| **Format** | Text | Binary frames | Binary frames |
| **Connections** | 6× per domain | 1 per domain | 1 per domain |
| **Multiplexing** | ❌ (pipelining broken) | ✅ streams | ✅ streams |
| **HOL Blocking** | App + Transport | Transport only | ✅ None |
| **Header compression** | ❌ | ✅ HPACK | ✅ QPACK |
| **Server Push** | ❌ | ✅ | ✅ (limited adoption) |
| **TLS** | Optional | Optional (required in practice) | Mandatory, built-in |
| **Connection setup** | 2-3 RTT | 2-3 RTT | 0-1 RTT |
| **Connection migration** | ❌ breaks on IP change | ❌ breaks on IP change | ✅ Connection ID |
| **Adoption** | Universal | ~65% of sites | ~30% of sites |
| **Lossy network perf** | Medium | Can degrade | Best |

---

## Where Each Protocol Lives in OSI

```
Layer 7  Application   │  HTTP/1.1, HTTP/2, HTTP/3 (the semantics: methods, headers, status)
Layer 6  Presentation  │  TLS 1.2/1.3 (for HTTP/1.1 & 2) │ QUIC has TLS 1.3 built in
Layer 5  Session       │  TCP sessions (1.1/2) │ QUIC Connection IDs (3)
Layer 4  Transport     │  TCP (HTTP/1.1 & 2)   │ UDP + QUIC (HTTP/3)
Layer 3  Network       │  IP (all versions)
Layer 2  Data Link     │  Ethernet / WiFi (all versions)
Layer 1  Physical      │  Cables / Radio (all versions)
```

HTTP/3's major architectural shift is that **QUIC fuses Layers 4, 5, and 6** into one protocol, bypassing the ossified TCP stack that's baked into every OS kernel worldwide. This is also why QUIC runs in **userspace** — updating QUIC doesn't require an OS kernel update, making iteration much faster.