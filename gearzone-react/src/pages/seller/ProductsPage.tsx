import { useEffect, useState } from 'react';
import { sellerApi } from '../../api/seller';

interface Product {
  id: string;
  name: string;
  basePrice: number;
  imageUrl?: string;
  isActive: boolean;
  stockQuantity: number;
}

export default function SellerProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [form, setForm] = useState({ name: '', basePrice: '', stockQuantity: '', description: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const fetchProducts = () => {
    sellerApi.products.list()
      .then(d => setProducts((d as { items?: Product[] }).items ?? (d as Product[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchProducts(); }, []);

  const resetForm = () => {
    setShowForm(false); setEditId(null);
    setForm({ name: '', basePrice: '', stockQuantity: '', description: '' });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    const payload = { name: form.name, basePrice: Number(form.basePrice), stockQuantity: Number(form.stockQuantity), description: form.description };
    try {
      if (editId) await sellerApi.products.update(editId, payload);
      else await sellerApi.products.create(payload);
      resetForm(); fetchProducts();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save product.');
    } finally { setSaving(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this product?')) return;
    await sellerApi.products.delete(id);
    fetchProducts();
  };

  const handleEdit = (p: Product) => {
    setEditId(p.id);
    setForm({ name: p.name, basePrice: String(p.basePrice), stockQuantity: String(p.stockQuantity), description: '' });
    setShowForm(true);
  };

  if (loading) return <div className="container text-center py-5"><div className="spinner-border text-primary" /></div>;

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="mb-0">My Products</h2>
        <button className="btn btn-primary" onClick={() => { resetForm(); setShowForm(true); }}>+ Add Product</button>
      </div>

      {showForm && (
        <div className="card shadow-sm mb-4">
          <div className="card-header"><h5 className="mb-0">{editId ? 'Edit Product' : 'New Product'}</h5></div>
          <div className="card-body">
            <form onSubmit={handleSubmit}>
              <div className="row g-3">
                <div className="col-md-6">
                  <label className="form-label">Product Name</label>
                  <input className="form-control" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required />
                </div>
                <div className="col-md-3">
                  <label className="form-label">Base Price (₫)</label>
                  <input className="form-control" type="number" value={form.basePrice} onChange={e => setForm(f => ({ ...f, basePrice: e.target.value }))} required />
                </div>
                <div className="col-md-3">
                  <label className="form-label">Stock</label>
                  <input className="form-control" type="number" value={form.stockQuantity} onChange={e => setForm(f => ({ ...f, stockQuantity: e.target.value }))} required />
                </div>
                <div className="col-12">
                  <label className="form-label">Description</label>
                  <textarea className="form-control" rows={2} value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} />
                </div>
              </div>
              {error && <div className="alert alert-danger mt-3 py-2">{error}</div>}
              <div className="d-flex gap-2 mt-3">
                <button type="submit" className="btn btn-success" disabled={saving}>
                  {saving ? <span className="spinner-border spinner-border-sm me-1" /> : null}Save
                </button>
                <button type="button" className="btn btn-outline-secondary" onClick={resetForm}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      <div className="card shadow-sm">
        <div className="table-responsive">
          <table className="table table-hover mb-0">
            <thead className="table-light">
              <tr>
                <th>Product</th>
                <th className="text-end">Price</th>
                <th className="text-end">Stock</th>
                <th className="text-center">Status</th>
                <th className="text-center">Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map(p => (
                <tr key={p.id}>
                  <td>
                    <div className="d-flex align-items-center gap-2">
                      {p.imageUrl && <img src={p.imageUrl} alt={p.name} className="rounded" style={{ width: 40, height: 40, objectFit: 'cover' }} />}
                      <span className="fw-semibold">{p.name}</span>
                    </div>
                  </td>
                  <td className="text-end">{p.basePrice?.toLocaleString()} ₫</td>
                  <td className="text-end">{p.stockQuantity}</td>
                  <td className="text-center">
                    <span className={`badge bg-${p.isActive ? 'success' : 'danger'}`}>{p.isActive ? 'Active' : 'Inactive'}</span>
                  </td>
                  <td className="text-center">
                    <button className="btn btn-sm btn-outline-primary me-1" onClick={() => handleEdit(p)}>Edit</button>
                    <button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(p.id)}>Delete</button>
                  </td>
                </tr>
              ))}
              {products.length === 0 && (
                <tr><td colSpan={5} className="text-center text-muted py-4">No products yet.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
