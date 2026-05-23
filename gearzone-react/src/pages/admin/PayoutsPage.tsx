import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Payout {
  id: string;
  storeName: string;
  amount: number;
  status: string;
  requestedAt: string;
  processedAt?: string;
  bankName?: string;
  bankAccount?: string;
}

export default function AdminPayoutsPage() {
  const [payouts, setPayouts] = useState<Payout[]>([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchPayouts = () => {
    setLoading(true);
    adminApi.payouts.list()
      .then(d => setPayouts((d as { items?: Payout[] }).items ?? (d as Payout[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchPayouts(); }, []);

  const handleProcess = async (id: string) => {
    setProcessing(id);
    try { await adminApi.payouts.process(id); fetchPayouts(); }
    finally { setProcessing(null); }
  };

  const handleReject = async (id: string) => {
    setProcessing(id);
    try { await adminApi.payouts.reject(id, 'Rejected by admin'); fetchPayouts(); }
    finally { setProcessing(null); }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Payout Requests</h1>
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Store</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Amount</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Bank Info</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Requested</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {payouts.map(p => (
            <tr key={p.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem', fontWeight: 500 }}>{p.storeName}</td>
              <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 700 }}>{p.amount?.toLocaleString()} VND</td>
              <td style={{ padding: '0.75rem', fontSize: 13, color: '#555' }}>
                {p.bankName && <div>{p.bankName}</div>}
                {p.bankAccount && <div style={{ color: '#888' }}>{p.bankAccount}</div>}
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ color: p.status === 'Completed' ? '#38a169' : p.status === 'Pending' ? '#ed8936' : '#e53e3e', fontWeight: 600 }}>{p.status}</span>
              </td>
              <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(p.requestedAt).toLocaleDateString()}</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                {p.status === 'Pending' && (
                  <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'center' }}>
                    <button onClick={() => handleProcess(p.id)} disabled={processing === p.id}
                      style={{ padding: '0.25rem 0.75rem', background: '#c6f6d5', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#276749' }}>
                      {processing === p.id ? '…' : 'Process'}
                    </button>
                    <button onClick={() => handleReject(p.id)} disabled={processing === p.id}
                      style={{ padding: '0.25rem 0.75rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030' }}>
                      Reject
                    </button>
                  </div>
                )}
              </td>
            </tr>
          ))}
          {payouts.length === 0 && (
            <tr><td colSpan={6} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No payout requests.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
