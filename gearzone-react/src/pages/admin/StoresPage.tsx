import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Store {
  id: string;
  name: string;
  slug: string;
  ownerName: string;
  ownerEmail: string;
  productCount: number;
  isActive: boolean;
  createdAt: string;
}

export default function AdminStoresPage() {
  const [stores, setStores] = useState<Store[]>([]);
  const [loading, setLoading] = useState(true);
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchStores = () => {
    setLoading(true);
    adminApi.stores.list()
      .then(d => setStores((d as { items?: Store[] }).items ?? (d as Store[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchStores(); }, []);

  const handleToggle = async (id: string, activate: boolean) => {
    setProcessing(id);
    try {
      await (activate ? adminApi.stores.activate(id) : adminApi.stores.deactivate(id));
      fetchStores();
    } finally { setProcessing(null); }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Stores</h1>
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Store</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Owner</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Products</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Joined</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {stores.map(s => (
            <tr key={s.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem' }}>
                <div style={{ fontWeight: 600 }}>{s.name}</div>
                <div style={{ fontSize: 12, color: '#888' }}>{s.slug}</div>
              </td>
              <td style={{ padding: '0.75rem' }}>
                <div>{s.ownerName}</div>
                <div style={{ fontSize: 12, color: '#888' }}>{s.ownerEmail}</div>
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'right' }}>{s.productCount}</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ color: s.isActive ? '#38a169' : '#e53e3e', fontWeight: 600 }}>{s.isActive ? 'Active' : 'Inactive'}</span>
              </td>
              <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(s.createdAt).toLocaleDateString()}</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <button onClick={() => handleToggle(s.id, !s.isActive)} disabled={processing === s.id}
                  style={{ padding: '0.25rem 0.75rem', background: s.isActive ? '#fed7d7' : '#c6f6d5', border: 'none', borderRadius: 4, cursor: 'pointer', color: s.isActive ? '#c53030' : '#276749' }}>
                  {processing === s.id ? '…' : s.isActive ? 'Deactivate' : 'Activate'}
                </button>
              </td>
            </tr>
          ))}
          {stores.length === 0 && (
            <tr><td colSpan={6} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No stores found.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
