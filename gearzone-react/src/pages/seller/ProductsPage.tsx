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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    const payload = { name: form.name, basePrice: Number(form.basePrice), stockQuantity: Number(form.stockQuantity), description: form.description };
    try {
      if (editId) {
        await sellerApi.products.update(editId, payload);
      } else {
        await sellerApi.products.create(payload);
      }
      setShowForm(false);
      setEditId(null);
      setForm({ name: '', basePrice: '', stockQuantity: '', description: '' });
      fetchProducts();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save product.');
    } finally {
      setSaving(false);
    }
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

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>My Products</h1>
        <button onClick={() => { setShowForm(true); setEditId(null); setForm({ name: '', basePrice: '', stockQuantity: '', description: '' }); }}
          style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          + Add Product
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} style={{ padding: '1.5rem', background: '#f9f9f9', borderRadius: 8, marginBottom: '1.5rem', display: 'flex', flexDirection: 'column', gap: '0.75rem', maxWidth: 500 }}>
          <h3 style={{ margin: 0 }}>{editId ? 'Edit Product' : 'New Product'}</h3>
          <input placeholder="Product Name" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required style={{ padding: '0.5rem' }} />
          <input type="number" placeholder="Base Price (VND)" value={form.basePrice} onChange={e => setForm(f => ({ ...f, basePrice: e.target.value }))} required style={{ padding: '0.5rem' }} />
          <input type="number" placeholder="Stock Quantity" value={form.stockQuantity} onChange={e => setForm(f => ({ ...f, stockQuantity: e.target.value }))} required style={{ padding: '0.5rem' }} />
          <textarea placeholder="Description" value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} rows={3} style={{ padding: '0.5rem' }} />
          {error && <p style={{ color: 'red', margin: 0 }}>{error}</p>}
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button type="submit" disabled={saving} style={{ flex: 1, padding: '0.5rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
              {saving ? 'Saving…' : 'Save'}
            </button>
            <button type="button" onClick={() => setShowForm(false)} style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
              Cancel
            </button>
          </div>
        </form>
      )}

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Product</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Price</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Stock</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {products.map(p => (
            <tr key={p.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                {p.imageUrl && <img src={p.imageUrl} alt={p.name} style={{ width: 40, height: 40, objectFit: 'cover', borderRadius: 4 }} />}
                {p.name}
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'right' }}>{p.basePrice?.toLocaleString()} VND</td>
              <td style={{ padding: '0.75rem', textAlign: 'right' }}>{p.stockQuantity}</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ color: p.isActive ? '#38a169' : '#e53e3e', fontWeight: 600 }}>{p.isActive ? 'Active' : 'Inactive'}</span>
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <button onClick={() => handleEdit(p)} style={{ marginRight: '0.5rem', padding: '0.25rem 0.75rem', background: '#bee3f8', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Edit</button>
                <button onClick={() => handleDelete(p.id)} style={{ padding: '0.25rem 0.75rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030' }}>Delete</button>
              </td>
            </tr>
          ))}
          {products.length === 0 && (
            <tr><td colSpan={5} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No products yet.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
