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
    <div className="container">
      <h2 className="mb-4">Products</h2>

      <div className="input-group mb-4" style={{ maxWidth: 400 }}>
        <input className="form-control" placeholder="Search products…" value={search}
          onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchProducts(search)} />
        <button className="btn btn-outline-secondary" onClick={() => fetchProducts(search)}>Search</button>
      </div>

      {loading ? (
        <div className="text-center py-4"><div className="spinner-border text-primary" /></div>
      ) : (
        <div className="card shadow-sm">
          <div className="table-responsive">
            <table className="table table-hover mb-0">
              <thead className="table-light">
                <tr>
                  <th>Product</th><th className="text-end">Price</th><th>Store</th>
                  <th className="text-center">Approved</th>
                  <th className="text-center">Active</th>
                  <th className="text-center">Actions</th>
                </tr>
              </thead>
              <tbody>
                {products.map(p => (
                  <tr key={p.id}>
                    <td>
                      <div className="fw-semibold">{p.name}</div>
                      <small className="text-muted">{p.brandName}</small>
                    </td>
                    <td className="text-end">{p.basePrice?.toLocaleString()} ₫</td>
                    <td className="text-muted small">{p.storeName}</td>
                    <td className="text-center">
                      <span className={`badge bg-${p.isApproved ? 'success' : 'warning'}`}>
                        {p.isApproved ? 'Approved' : 'Pending'}
                      </span>
                    </td>
                    <td className="text-center">
                      <span className={`badge bg-${p.isActive ? 'success' : 'danger'}`}>
                        {p.isActive ? 'Yes' : 'No'}
                      </span>
                    </td>
                    <td className="text-center">
                      <div className="d-flex gap-1 justify-content-center flex-wrap">
                        {!p.isApproved && (
                          <button className="btn btn-sm btn-outline-success" disabled={processing === p.id}
                            onClick={() => handleApprove(p.id)}>Approve</button>
                        )}
                        <button
                          className={`btn btn-sm ${p.isActive ? 'btn-outline-danger' : 'btn-outline-primary'}`}
                          disabled={processing === p.id}
                          onClick={() => handleToggleActive(p.id, !p.isActive)}>
                          {processing === p.id ? <span className="spinner-border spinner-border-sm" /> : (p.isActive ? 'Deactivate' : 'Activate')}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
                {products.length === 0 && (
                  <tr><td colSpan={6} className="text-center text-muted py-4">No products found.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
