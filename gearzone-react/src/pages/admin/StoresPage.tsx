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
    try { await (activate ? adminApi.stores.activate(id) : adminApi.stores.deactivate(id)); fetchStores(); }
    finally { setProcessing(null); }
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  return (
    <div className="container">
      <h2 className="mb-4">Stores</h2>
      <div className="card shadow-sm">
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr>
                <th>Store</th><th>Owner</th><th className="text-end">Products</th>
                <th className="text-center">Status</th><th>Joined</th><th className="text-center">Actions</th>
              </tr>
            </thead>
            <tbody>
              {stores.map(s => (
                <tr key={s.id}>
                  <td>
                    <div className="fw-semibold">{s.name}</div>
                    <small className="text-muted">{s.slug}</small>
                  </td>
                  <td>
                    <div>{s.ownerName}</div>
                    <small className="text-muted">{s.ownerEmail}</small>
                  </td>
                  <td className="text-end">{s.productCount}</td>
                  <td className="text-center">
                    <span className={`badge bg-${s.isActive ? 'success' : 'danger'}`}>{s.isActive ? 'Active' : 'Inactive'}</span>
                  </td>
                  <td className="text-muted small">{new Date(s.createdAt).toLocaleDateString()}</td>
                  <td className="text-center">
                    <button
                      className={`btn btn-sm ${s.isActive ? 'btn-outline-danger' : 'btn-outline-success'}`}
                      onClick={() => handleToggle(s.id, !s.isActive)}
                      disabled={processing === s.id}>
                      {processing === s.id ? <span className="spinner-border spinner-border-sm" /> : (s.isActive ? 'Deactivate' : 'Activate')}
                    </button>
                  </td>
                </tr>
              ))}
              {stores.length === 0 && <tr><td colSpan={6} className="text-center text-muted py-4">No stores found.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
