import { useState, useEffect } from "react";

// AuditEntry-shape (från backend):
// {
//   id: string,
//   timestamp: string (ISO),
//   question: string,
//   summary: string,
//   confidence: string
// }
const TEXT_DARK = "#000";
const TEXT_LIGHT = "#fff";
const TEXT_MUTED_DARK = "#000";
const TEXT_MUTED_LIGHT = "#fff";

const API_BASE = "http://localhost:5222";

export default function App() {
  const [question, setQuestion] = useState("");
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState(null);
  const [error, setError] = useState("");
  const [audit, setAudit] = useState([]);
  const [selectedAudit, setSelectedAudit] = useState(null);

  async function ask() {
    setError("");
    setLoading(true);
    setResult(null);

    try {
      const res = await fetch(`${API_BASE}/api/copilot/query`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ text: question }),
      });

      if (!res.ok) throw new Error("Backend svarade inte OK");

      const data = await res.json();
      setResult(data);

      await loadAudit();
    } catch (e) {
      setError(
        "Något gick fel. Kontrollera att backend kör och att porten stämmer.",
      );
    } finally {
      setLoading(false);
    }
  }

  async function loadAudit() {
    try {
      const res = await fetch(`${API_BASE}/api/copilot/audit`);
      if (!res.ok) throw new Error("Audit endpoint svarade inte OK");

      const data = await res.json();
      const list = Array.isArray(data) ? data : [];
      setAudit(list);

      if (list.length > 0 && !selectedAudit) {
        setSelectedAudit(list[0]);
      }
    } catch {
      setAudit([]);
      setSelectedAudit(null);
    }
  }

  useEffect(() => {
    loadAudit();
  }, []);

  return (
    <div
      style={{
        padding: 24,
        fontFamily: "system-ui",
        maxWidth: 900,
        margin: "0 auto",
      }}
    >
      <header style={{ marginBottom: 16 }}>
        <h1 style={{ margin: 0 }}>Enterprise Data Copilot</h1>
        <p style={{ marginTop: 8, color: "#fff" }}>
          Ställ en fråga och få insikter med källor och spårbarhet.
        </p>
      </header>

      <section style={{ display: "flex", gap: 12 }}>
        <input
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          placeholder="Ex: Varför ökade kostnaderna i Produktion förra månaden?"
          style={{
            flex: 1,
            padding: 12,
            borderRadius: 10,
            border: "1px solid #ccc",
            fontSize: 16,
          }}
        />
        <button
          onClick={ask}
          disabled={loading || question.trim().length === 0}
          style={{
            padding: "12px 16px",
            color: "#000",
            borderRadius: 10,
            border: "1px solid #eee",
            background: loading ? "#eee" : "white",
            cursor: loading ? "not-allowed" : "pointer",
            fontSize: 16,
          }}
        >
          {loading ? "Frågar..." : "Fråga"}
        </button>
      </section>

      {error && <p style={{ marginTop: 12, color: "crimson" }}>{error}</p>}

      {result && (
        <main style={{ marginTop: 24 }}>
          <SummaryCard
            summary={result.summary}
            confidence={result.confidence}
          />

          <h2 style={{ marginTop: 24 }}>Insikter</h2>
          <div style={{ display: "grid", gap: 12 }}>
            {result.insights?.map((ins, idx) => (
              <InsightCard key={idx} insight={ins} />
            ))}
          </div>

          <h2 style={{ marginTop: 24 }}>Källor</h2>
          <div style={{ display: "grid", gap: 12 }}>
            {result.citations?.map((c) => (
              <CitationCard key={c.id} citation={c} />
            ))}
          </div>

          <h2 style={{ marginTop: 24 }}>Följdfrågor</h2>
          <ul>
            {result.followUps?.map((q, idx) => (
              <li key={idx}>{q}</li>
            ))}
          </ul>

          <h2 style={{ marginTop: 24 }}>Audit-logg</h2>
          <p style={{ marginTop: 6, color: "#fff" }}>
            Tidigare frågor och svar (spårbarhet).
          </p>

          <div
            style={{
              display: "grid",
              gridTemplateColumns: "1fr 1fr",
              gap: 12,
              marginTop: 12,
            }}
          >
            <AuditList
              audit={audit}
              selectedId={selectedAudit?.id}
              onSelect={(entry) => setSelectedAudit(entry)}
            />

            <AuditDetails entry={selectedAudit} />
          </div>
        </main>
      )}
    </div>
  );
}

function SummaryCard({ summary, confidence }) {
  return (
    <div
      style={{
        border: "1px solid #ddd",
        color: "#000",
        borderRadius: 14,
        padding: 16,
        background: "white",
        boxShadow: "0 1px 6px rgba(0,0,0,0.06)",
      }}
    >
      <div
        style={{ display: "flex", justifyContent: "space-between", gap: 12 }}
      >
        <strong>Sammanfattning</strong>
        <span style={{ color: "#000" }}>Säkerhet: {confidence}</span>
      </div>
      <p style={{ marginTop: 10 }}>{summary}</p>
    </div>
  );
}

function InsightCard({ insight }) {
  return (
    <div
      style={{
        border: "1px solid #ddd",
        color: "#000",
        borderRadius: 14,
        padding: 16,
        background: "white",
        boxShadow: "0 1px 6px rgba(0,0,0,0.06)",
      }}
    >
      <div
        style={{ display: "flex", justifyContent: "space-between", gap: 12 }}
      >
        <strong>{insight.title}</strong>
        <span style={{ color: "#000" }}>{insight.type}</span>
      </div>

      <div style={{ marginTop: 10, color: "#000" }}>
        <div>
          <strong>Metric:</strong> {insight.metric}
        </div>
        {typeof insight.delta === "number" && (
          <div>
            <strong>Delta:</strong> {(insight.delta * 100).toFixed(1)}%
          </div>
        )}
        <div style={{ marginTop: 8 }}>
          <strong>Varför:</strong> {insight.why}
        </div>
      </div>

      <div style={{ marginTop: 12 }}>
        <div style={{ fontSize: 12, color: "#000" }}>Evidens</div>
        <div
          style={{ display: "flex", flexWrap: "wrap", gap: 8, marginTop: 6 }}
        >
          {insight.evidence?.map((e) => (
            <span
              key={e}
              style={{
                fontSize: 12,
                padding: "4px 8px",
                border: "1px solid #ccc",
                borderRadius: 999,
              }}
            >
              {e}
            </span>
          ))}
        </div>
      </div>
    </div>
  );
}

function CitationCard({ citation }) {
  return (
    <div
      style={{
        border: "1px solid #ddd",
        borderRadius: 14,
        padding: 16,
        background: "white",
        color: "#000",
      }}
    >
      <strong>{citation.label}</strong>
      <div style={{ fontSize: 12, color: "#000", marginTop: 6 }}>
        {citation.id}
      </div>
      <p style={{ marginTop: 8 }}>{citation.snippet}</p>
    </div>
  );
}

function AuditList({ audit, selectedId, onSelect }) {
  if (!audit || audit.length === 0) {
    return (
      <div
        style={{
          border: "1px solid #ddd",
          borderRadius: 14,
          padding: 16,
          background: "white",
          color: "#000",
        }}
      >
        <strong>Inga audit-rader ännu</strong>
        <p style={{ marginTop: 8, color: "#fff" }}>
          Ställ en fråga för att skapa en audit-post.
        </p>
      </div>
    );
  }

  return (
    <div
      style={{
        border: "1px solid #ddd",
        borderRadius: 14,
        background: "white",
        overflow: "hidden",
        color: "#000",
      }}
    >
      {audit.map((entry) => {
        const active = entry.id === selectedId;
        return (
          <button
            key={entry.id}
            onClick={() => onSelect(entry)}
            style={{
              width: "100%",
              textAlign: "left",
              border: "none",
              borderBottom: "1px solid #eee",
              background: active ? "#f5f5f5" : "white",
              padding: 12,
              cursor: "pointer",
              color: "#000",
            }}
          >
            <div
              style={{
                display: "flex",
                justifyContent: "space-between",
                gap: 12,
              }}
            >
              <strong style={{ fontSize: 14, lineHeight: 1.2 }}>
                {truncate(entry.question, 60)}
              </strong>
              <span style={{ fontSize: 12, color: "#000" }}>
                {entry.confidence}
              </span>
            </div>

            <div style={{ marginTop: 6, fontSize: 12, color: "#000" }}>
              {formatTimestamp(entry.timestamp)}
            </div>
          </button>
        );
      })}
    </div>
  );
}

function AuditDetails({ entry }) {
  if (!entry) {
    return (
      <div
        style={{
          border: "1px solid #ddd",
          borderRadius: 14,
          padding: 16,
          background: "white",
        }}
      >
        <strong>Välj en rad</strong>
        <p style={{ marginTop: 8, color: "#000" }}>
          Klicka på en fråga i listan för att se detaljer.
        </p>
      </div>
    );
  }

  return (
    <div
      style={{
        border: "1px solid #ddd",
        borderRadius: 14,
        padding: 16,
        background: "white",
      }}
    >
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          gap: 12,
          color: "#000",
        }}
      >
        <strong>Audit-detaljer</strong>
        <span style={{ fontSize: 12, color: "#000" }}>
          Säkerhet: {entry.confidence}
        </span>
      </div>

      <div style={{ marginTop: 10, fontSize: 12, color: "#000" }}>
        {formatTimestamp(entry.timestamp)}
      </div>

      <div style={{ marginTop: 12, color: "#000" }}>
        <div style={{ fontSize: 12 }}>Fråga</div>
        <div style={{ marginTop: 6 }}>{entry.question}</div>
      </div>

      <div style={{ marginTop: 12, color: "#000" }}>
        <div style={{ fontSize: 12, color: "#000" }}>Sammanfattning</div>
        <div style={{ marginTop: 6 }}>{entry.summary}</div>
      </div>

      <div style={{ marginTop: 12, fontSize: 12, color: "#000" }}>
        ID: {entry.id}
      </div>
    </div>
  );
}

function truncate(text, maxLen) {
  if (!text) return "";
  return text.length > maxLen ? text.slice(0, maxLen - 1) + "…" : text;
}

function formatTimestamp(isoString) {
  if (!isoString) return "";
  const date = new Date(isoString);

  // Om datumet blir "Invalid Date" av någon anledning
  if (Number.isNaN(date.getTime())) return isoString;

  // Visa lokalt datum + tid, svensk stil
  return date.toLocaleString("sv-SE", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
}
