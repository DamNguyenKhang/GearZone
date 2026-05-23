import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Product {
  id: string;
  name: string;
  basePrice: number;
  brandName: string;
  storeName: string;
  isActive: boolean;
  isApproved: boolean;
}

export default function AdminProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchProducts = (q = '') => {
    setLoading(true);
    adminApi.products.list({ search: q })
      .then(d => setProducts((d as { items?: Product[] }).items ?? (d as Product[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchProducts(); }, []);

  const handleApprove = async (id: string) => {
    setProcessing(id);
    try { await adminApi.products.approve(id); fetchProducts(search); }
    finally { setProcessing(null); }
  };

  const handleToggleActive = async (id: string, activate: boolean) => {
    setProcessing(id);
    try {
      await (activate ? adminApi.products.activate(id) : adminApi.products.deactivate(id));
      fetchProducts(search);
    } finally { setProcessing(null); }
  };

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Products</h1>
      <input placeholder="Search products…" value={search} onChange={e => setSearch(e.target.value)}
        onKeyDown={e => e.key === 'Enter' && fetchProducts(search)}
        style={{ padding: '0.5rem', width: '100%', maxWidth: 400, marginBottom: '1rem' }} />

      {loading ? <div>Loading…</div> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#f0f0f0' }}>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Product</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Price</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Store</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Approved</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Active</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {products.map(p => (
              <tr key={p.id} style={{ borderBottom: '1px solid #eee' }}>
                <td style={{ padding: '0.75rem' }}>
                  <div style={{ fontWeight: 500 }}>{p.name}</div>
                  <div style={{ fontSize: 12, color: '#888' }}>{p.brandName}</div>
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'right' }}>{p.basePrice?.toLocaleString()} VND</td>
                <td style={{ padding: '0.75rem', fontSize: 13, color: '#555' }}>{p.storeName}</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <span style={{ color: p.isApproved ? '#38a169' : '#ed8936', fontWeight: 600 }}>{p.isApproved ? '✓' : 'Pending'}</span>
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <span style={{ color: p.isActive ? '#38a169' : '#e53e3e', fontWeight: 600 }}>{p.isActive ? 'Yes' : 'No'}</span>
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <div style={{ display: 'flex', gap: '0.4rem', justifyContent: 'center', flexWrap: 'wrap' }}>
                    {!p.isApproved && (
                      <button onClick={() => handleApprove(p.id)} disabled={processing === p.id}
                        style={{ padding: '0.2rem 0.6rem', background: '#c6f6d5', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#276749', fontSize: 12 }}>
                        Approve
                      </button>
                    )}
                    <button onClick={() => handleToggleActive(p.id, !p.isActive)} disabled={processing === p.id}
                      style={{ padding: '0.2rem 0.6rem', background: p.isActive ? '#fed7d7' : '#bee3f8', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 12 }}>
                      {processing === p.id ? '…' : p.isActive ? 'Deactivate' : 'Activate'}
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {products.length === 0 && (
              <tr><td colSpan={6} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No products found.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
